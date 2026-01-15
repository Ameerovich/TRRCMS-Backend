using AutoMapper;
using MediatR;
using System.Text.Json;
using TRRCMS.Application.Claims.Dtos;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Claims.Commands.UpdateClaim;

public class UpdateClaimCommandHandler : IRequestHandler<UpdateClaimCommand, ClaimDto>
{
    private readonly IClaimRepository _claimRepository;
    private readonly IPersonRepository _personRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public UpdateClaimCommandHandler(
        IClaimRepository claimRepository,
        IPersonRepository personRepository,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IMapper mapper)
    {
        _claimRepository = claimRepository;
        _personRepository = personRepository;
        _currentUserService = currentUserService;
        _auditService = auditService;
        _mapper = mapper;
    }

    public async Task<ClaimDto> Handle(
        UpdateClaimCommand request,
        CancellationToken cancellationToken)
    {
        // Get current user
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Get existing claim
        var claim = await _claimRepository.GetByIdAsync(request.ClaimId, cancellationToken)
            ?? throw new NotFoundException($"Claim with ID {request.ClaimId} not found");

        // Store old values for audit
        var oldValues = new
        {
            claim.PrimaryClaimantId,
            claim.ClaimType,
            claim.Priority,
            claim.TenureContractType,
            claim.Status,
            claim.ProcessingNotes,
            claim.PublicRemarks
        };

        var changedFields = new List<string>();

        // Validate and update PrimaryClaimantId if provided
        if (request.PrimaryClaimantId.HasValue)
        {
            var claimant = await _personRepository.GetByIdAsync(
                request.PrimaryClaimantId.Value,
                cancellationToken);

            if (claimant == null)
            {
                throw new ValidationException(
                    $"Person with ID {request.PrimaryClaimantId.Value} not found");
            }

            claim.UpdatePrimaryClaimant(request.PrimaryClaimantId.Value, currentUserId);
            changedFields.Add("PrimaryClaimantId");
        }

        // Update claim classification if provided
        if (!string.IsNullOrWhiteSpace(request.ClaimType) || request.Priority.HasValue)
        {
            claim.UpdateClassification(
                request.ClaimType ?? claim.ClaimType,
                request.Priority ?? claim.Priority,
                currentUserId);

            if (!string.IsNullOrWhiteSpace(request.ClaimType)) changedFields.Add("ClaimType");
            if (request.Priority.HasValue) changedFields.Add("Priority");
        }

        // Update tenure details if provided
        if (request.TenureContractType.HasValue)
        {
            claim.UpdateTenureContract(
                request.TenureContractType.Value,
                request.TenureContractDetails,
                currentUserId);

            changedFields.Add("TenureContractType");
            if (!string.IsNullOrWhiteSpace(request.TenureContractDetails))
                changedFields.Add("TenureContractDetails");
        }

        // Update status if provided (use with caution until state machine)
        if (request.Status.HasValue)
        {
            // Map status to lifecycle stage
            var lifecycleStage = request.Status.Value switch
            {
                ClaimStatus.Draft => LifecycleStage.DraftPendingSubmission,
                ClaimStatus.Finalized => LifecycleStage.Submitted,
                ClaimStatus.UnderReview => LifecycleStage.UnderReview,
                ClaimStatus.PendingEvidence => LifecycleStage.AwaitingDocuments,
                ClaimStatus.Disputed => LifecycleStage.ConflictDetected,
                ClaimStatus.Approved => LifecycleStage.Approved,
                ClaimStatus.Rejected => LifecycleStage.Rejected,
                ClaimStatus.Archived => LifecycleStage.Archived,
                _ => claim.LifecycleStage
            };

            claim.MoveToStage(lifecycleStage, currentUserId);
            changedFields.Add("Status");
            changedFields.Add("LifecycleStage");
        }

        // Update processing notes if provided
        if (!string.IsNullOrWhiteSpace(request.ProcessingNotes))
        {
            claim.AddProcessingNotes(request.ProcessingNotes, currentUserId);
            changedFields.Add("ProcessingNotes");
        }

        // Update public remarks if provided
        if (!string.IsNullOrWhiteSpace(request.PublicRemarks))
        {
            claim.AddPublicRemarks(request.PublicRemarks, currentUserId);
            changedFields.Add("PublicRemarks");
        }

        // Save changes
        await _claimRepository.UpdateAsync(claim, cancellationToken);

        // Store new values for audit
        var newValues = new
        {
            claim.PrimaryClaimantId,
            claim.ClaimType,
            claim.Priority,
            claim.TenureContractType,
            claim.Status,
            claim.ProcessingNotes,
            claim.PublicRemarks
        };

        // Audit logging using YOUR IAuditService signature
        await _auditService.LogActionAsync(
            actionType: AuditActionType.Update,
            actionDescription: $"Updated claim {claim.ClaimNumber}. Reason: {request.ReasonForModification}",
            entityType: "Claim",
            entityId: claim.Id,
            entityIdentifier: claim.ClaimNumber,
            oldValues: JsonSerializer.Serialize(oldValues),
            newValues: JsonSerializer.Serialize(newValues),
            changedFields: string.Join(", ", changedFields),
            cancellationToken: cancellationToken);

        // Return updated claim
        return _mapper.Map<ClaimDto>(claim);
    }
}