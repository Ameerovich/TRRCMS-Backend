using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using TRRCMS.Application.BuildingAssignments.Dtos;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.BuildingAssignments.Commands.AssignBuildings;

/// <summary>
/// Handler for AssignBuildingsCommand
/// UC-012: Assign Buildings to Field Collectors
/// </summary>
public class AssignBuildingsCommandHandler : IRequestHandler<AssignBuildingsCommand, AssignBuildingsResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;
    private readonly ILogger<AssignBuildingsCommandHandler> _logger;

    public AssignBuildingsCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IMapper mapper,
        ILogger<AssignBuildingsCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AssignBuildingsResult> Handle(AssignBuildingsCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var result = new AssignBuildingsResult();
        var createdAssignments = new List<BuildingAssignment>();

        // Validate field collector exists and is a field collector
        var fieldCollector = await _unitOfWork.Users.GetByIdAsync(request.FieldCollectorId, cancellationToken);
        if (fieldCollector == null)
        {
            throw new NotFoundException($"Field collector with ID {request.FieldCollectorId} not found");
        }

        if (fieldCollector.Role != UserRole.FieldCollector)
        {
            throw new ValidationException($"User {fieldCollector.Username} is not a field collector");
        }

        if (!fieldCollector.IsActive)
        {
            throw new ValidationException($"Field collector {fieldCollector.Username} is not active");
        }

        _logger.LogInformation(
            "Assigning {BuildingCount} buildings to field collector {FieldCollectorId} ({FieldCollectorName})",
            request.Buildings.Count, request.FieldCollectorId, fieldCollector.FullNameArabic);

        // Process each building
        foreach (var buildingItem in request.Buildings)
        {
            try
            {
                // Validate building exists
                var building = await _unitOfWork.Buildings.GetByIdAsync(buildingItem.BuildingId, cancellationToken);
                if (building == null)
                {
                    result.Errors.Add(new AssignmentError
                    {
                        BuildingId = buildingItem.BuildingId,
                        BuildingCode = "Unknown",
                        ErrorMessage = $"Building with ID {buildingItem.BuildingId} not found"
                    });
                    result.FailedCount++;
                    continue;
                }

                // Determine if this is a revisit assignment
                bool isRevisit = buildingItem.PropertyUnitIdsForRevisit?.Any() == true;
                
                BuildingAssignment assignment;

                if (isRevisit)
                {
                    // Validate revisit reason is provided
                    if (string.IsNullOrWhiteSpace(buildingItem.RevisitReason))
                    {
                        result.Errors.Add(new AssignmentError
                        {
                            BuildingId = buildingItem.BuildingId,
                            BuildingCode = building.BuildingId,
                            ErrorMessage = "Revisit reason is required when property units are specified for revisit"
                        });
                        result.FailedCount++;
                        continue;
                    }

                    // Find original assignment (most recent completed one) - OPTIONAL
                    var originalAssignments = await _unitOfWork.BuildingAssignments
                        .GetByBuildingIdAsync(buildingItem.BuildingId, cancellationToken);
                    
                    var originalAssignment = originalAssignments
                        .Where(a => a.TransferStatus == TransferStatus.Synchronized || 
                                   a.ActualCompletionDate.HasValue)
                        .OrderByDescending(a => a.ActualCompletionDate ?? a.CreatedAtUtc)
                        .FirstOrDefault();

                    // =====================================================
                    // FIX: Use null instead of Guid.Empty when no original 
                    // assignment exists. Guid.Empty violates FK constraint!
                    // =====================================================
                    Guid? originalAssignmentId = originalAssignment?.Id;  // Can be null - that's OK!

                    // Serialize unit IDs for storage
                    var unitsForRevisit = System.Text.Json.JsonSerializer.Serialize(
                        buildingItem.PropertyUnitIdsForRevisit);

                    // Create revisit assignment
                    assignment = BuildingAssignment.CreateRevisit(
                        buildingId: buildingItem.BuildingId,
                        fieldCollectorId: request.FieldCollectorId,
                        originalAssignmentId: originalAssignmentId,  // Nullable - OK if null
                        unitsForRevisit: unitsForRevisit,
                        revisitReason: buildingItem.RevisitReason!,
                        totalPropertyUnits: buildingItem.PropertyUnitIdsForRevisit!.Count,
                        createdByUserId: currentUserId
                    );

                    _logger.LogDebug(
                        "Creating revisit assignment for building {BuildingCode}. Original assignment: {OriginalId}",
                        building.BuildingId, 
                        originalAssignmentId?.ToString() ?? "None (first visit)");
                }
                else
                {
                    // Create regular assignment
                    assignment = BuildingAssignment.Create(
                        buildingId: buildingItem.BuildingId,
                        fieldCollectorId: request.FieldCollectorId,
                        assignedByUserId: currentUserId,
                        totalPropertyUnits: building.NumberOfPropertyUnits,
                        targetCompletionDate: request.TargetCompletionDate,
                        assignmentNotes: CombineNotes(request.AssignmentNotes, buildingItem.Notes),
                        createdByUserId: currentUserId
                    );
                }

                // Set priority if different from default
                if (!string.IsNullOrWhiteSpace(request.Priority) && request.Priority != "Normal")
                {
                    assignment.SetPriority(request.Priority, currentUserId);
                }

                await _unitOfWork.BuildingAssignments.AddAsync(assignment, cancellationToken);
                createdAssignments.Add(assignment);
                
                result.CreatedAssignmentIds.Add(assignment.Id);
                result.AssignedCount++;

                _logger.LogDebug(
                    "Created {AssignmentType} assignment {AssignmentId} for building {BuildingCode} to collector {CollectorName}",
                    isRevisit ? "revisit" : "regular",
                    assignment.Id, building.BuildingId, fieldCollector.FullNameArabic);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning building {BuildingId}", buildingItem.BuildingId);
                result.Errors.Add(new AssignmentError
                {
                    BuildingId = buildingItem.BuildingId,
                    BuildingCode = "Unknown",
                    ErrorMessage = ex.Message
                });
                result.FailedCount++;
            }
        }

        // Save all changes
        if (createdAssignments.Any())
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Map to DTOs
            foreach (var assignment in createdAssignments)
            {
                var building = await _unitOfWork.Buildings.GetByIdAsync(assignment.BuildingId, cancellationToken);
                
                result.Assignments.Add(new BuildingAssignmentSummaryDto
                {
                    Id = assignment.Id,
                    BuildingId = assignment.BuildingId,
                    BuildingCode = building?.BuildingId ?? string.Empty,
                    BuildingAddress = building?.Address,
                    FieldCollectorId = assignment.FieldCollectorId,
                    FieldCollectorName = fieldCollector.FullNameArabic,
                    AssignedDate = assignment.AssignedDate,
                    TransferStatus = assignment.TransferStatus,
                    TransferStatusName = assignment.TransferStatus.ToString(),
                    TotalPropertyUnits = assignment.TotalPropertyUnits,
                    CompletedPropertyUnits = assignment.CompletedPropertyUnits,
                    CompletionPercentage = assignment.GetCompletionPercentage(),
                    IsActive = assignment.IsActive,
                    IsOverdue = assignment.IsOverdue(),
                    IsRevisit = assignment.IsRevisit,
                    Priority = assignment.Priority
                });
            }

            // Audit logging
            await _auditService.LogActionAsync(
                actionType: AuditActionType.Create,
                actionDescription: $"Assigned {result.AssignedCount} buildings to field collector {fieldCollector.FullNameArabic}",
                entityType: "BuildingAssignment",
                entityId: result.CreatedAssignmentIds.FirstOrDefault(),
                entityIdentifier: $"Batch: {result.AssignedCount} buildings",
                oldValues: null,
                newValues: System.Text.Json.JsonSerializer.Serialize(new
                {
                    FieldCollectorId = request.FieldCollectorId,
                    FieldCollectorName = fieldCollector.FullNameArabic,
                    BuildingCount = result.AssignedCount,
                    AssignmentIds = result.CreatedAssignmentIds
                }),
                changedFields: "New Assignment Batch",
                cancellationToken: cancellationToken
            );
        }

        result.Success = result.AssignedCount > 0;
        result.Message = result.Success
            ? $"Successfully assigned {result.AssignedCount} building(s) to {fieldCollector.FullNameArabic}"
            : "No buildings were assigned";

        if (result.FailedCount > 0)
        {
            result.Message += $". {result.FailedCount} building(s) failed to assign.";
        }

        return result;
    }

    private static string? CombineNotes(string? globalNotes, string? buildingNotes)
    {
        if (string.IsNullOrWhiteSpace(globalNotes) && string.IsNullOrWhiteSpace(buildingNotes))
            return null;

        if (string.IsNullOrWhiteSpace(globalNotes))
            return buildingNotes;

        if (string.IsNullOrWhiteSpace(buildingNotes))
            return globalNotes;

        return $"{globalNotes}\n---\n{buildingNotes}";
    }
}
