using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Surveys.Commands.DeleteIdentificationDocument;

/// <summary>
/// Handler for DeleteIdentificationDocumentCommand.
/// Soft-deletes the IdentificationDocument and removes the physical file.
/// </summary>
public class DeleteIdentificationDocumentCommandHandler : IRequestHandler<DeleteIdentificationDocumentCommand, Unit>
{
    private readonly ISurveyRepository _surveyRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IHouseholdRepository _householdRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IIdentificationDocumentRepository _idDocRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public DeleteIdentificationDocumentCommandHandler(
        ISurveyRepository surveyRepository,
        IPersonRepository personRepository,
        IHouseholdRepository householdRepository,
        IPropertyUnitRepository propertyUnitRepository,
        IIdentificationDocumentRepository idDocRepository,
        IFileStorageService fileStorageService,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _surveyRepository = surveyRepository ?? throw new ArgumentNullException(nameof(surveyRepository));
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _householdRepository = householdRepository ?? throw new ArgumentNullException(nameof(householdRepository));
        _propertyUnitRepository = propertyUnitRepository ?? throw new ArgumentNullException(nameof(propertyUnitRepository));
        _idDocRepository = idDocRepository ?? throw new ArgumentNullException(nameof(idDocRepository));
        _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    public async Task<Unit> Handle(DeleteIdentificationDocumentCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var survey = await _surveyRepository.GetByIdAsync(request.SurveyId, cancellationToken)
            ?? throw new NotFoundException($"Survey with ID {request.SurveyId} not found");

        if (survey.FieldCollectorId != currentUserId)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync(cancellationToken);
            if (currentUser == null || !currentUser.HasPermission(Permission.Surveys_EditAll))
                throw new UnauthorizedAccessException("You can only delete documents for your own surveys");
        }

        if (survey.Status != SurveyStatus.Draft)
            throw new ValidationException($"Cannot delete documents for survey in {survey.Status} status. Only Draft surveys can be modified.");

        var idDoc = await _idDocRepository.GetByIdAsync(request.DocumentId, cancellationToken)
            ?? throw new NotFoundException($"Identification document with ID {request.DocumentId} not found");

        // Validate document belongs to a person in this survey's building context
        var person = await _personRepository.GetByIdAsync(idDoc.PersonId, cancellationToken);
        if (person?.HouseholdId != null)
        {
            var household = await _householdRepository.GetByIdAsync(person.HouseholdId.Value, cancellationToken);
            if (household != null)
            {
                var propertyUnit = await _propertyUnitRepository.GetByIdAsync(household.PropertyUnitId, cancellationToken);
                if (propertyUnit == null || propertyUnit.BuildingId != survey.BuildingId)
                    throw new ValidationException("Document does not belong to this survey's building");
            }
        }

        var docInfo = new
        {
            idDoc.Id,
            idDoc.PersonId,
            idDoc.DocumentType,
            idDoc.OriginalFileName,
            idDoc.FilePath,
            idDoc.Description,
            idDoc.DocumentReferenceNumber
        };

        try
        {
            await _fileStorageService.DeleteFileAsync(idDoc.FilePath, cancellationToken);
        }
        catch (Exception ex)
        {
            // File may already be missing; do not block the soft-delete
            Console.WriteLine($"Warning: Could not delete physical file: {ex.Message}");
        }

        idDoc.MarkAsDeleted(currentUserId);
        await _idDocRepository.UpdateAsync(idDoc, cancellationToken);
        await _idDocRepository.SaveChangesAsync(cancellationToken);

        await _auditService.LogActionAsync(
            actionType: AuditActionType.Delete,
            actionDescription: $"Deleted identification document '{docInfo.OriginalFileName}' from survey {survey.ReferenceCode}",
            entityType: "IdentificationDocument",
            entityId: idDoc.Id,
            entityIdentifier: docInfo.OriginalFileName,
            oldValues: System.Text.Json.JsonSerializer.Serialize(docInfo),
            newValues: null,
            changedFields: "Deleted",
            cancellationToken: cancellationToken);

        return Unit.Value;
    }
}
