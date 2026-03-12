using AutoMapper;
using MediatR;
using System.Text.Json;
using TRRCMS.Application.Claims.Dtos;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Claims.Commands.UpdateClaim;

/// <summary>
/// Composite handler: updates the source PersonPropertyRelation + manages evidence links,
/// then re-derives claim state (ClaimType, CaseStatus) from the updated relation.
/// </summary>
public class UpdateClaimCommandHandler : IRequestHandler<UpdateClaimCommand, UpdateClaimResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    public UpdateClaimCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuditService auditService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<UpdateClaimResultDto> Handle(
        UpdateClaimCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Authenticate
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // 2. Load claim
        var claim = await _unitOfWork.Claims.GetByIdAsync(request.ClaimId, cancellationToken)
            ?? throw new NotFoundException($"Claim with ID {request.ClaimId} not found");

        if (!claim.PrimaryClaimantId.HasValue)
            throw new ValidationException("Claim has no primary claimant; cannot update relation.");

        // 3. Find or create source relation
        var relation = await _unitOfWork.PersonPropertyRelations
            .GetByPersonAndPropertyUnitAsync(claim.PrimaryClaimantId.Value, claim.PropertyUnitId, cancellationToken);

        bool relationWasCreated = false;
        if (relation == null)
        {
            // Auto-create relation for uhc-imported claims that have no relation yet
            var inferredRelationType = claim.ClaimType == ClaimType.OwnershipClaim
                ? RelationType.Owner
                : RelationType.Occupant;

            relation = PersonPropertyRelation.Create(
                claim.PrimaryClaimantId.Value,
                claim.PropertyUnitId,
                inferredRelationType,
                null,    // occupancyType
                false,   // hasEvidence
                currentUserId,
                claim.OriginatingSurveyId);

            await _unitOfWork.PersonPropertyRelations.AddAsync(relation, cancellationToken);
            relationWasCreated = true;
        }

        // 4. Snapshot old values for audit
        var oldValues = new
        {
            RelationType = (int)relation.RelationType,
            OccupancyType = relation.OccupancyType.HasValue ? (int?)relation.OccupancyType.Value : null,
            relation.OwnershipShare,
            relation.ContractDetails,
            relation.Notes,
            relation.HasEvidence,
            ClaimType = (int)claim.ClaimType,
            CaseStatus = (int)claim.CaseStatus,
            TenureContractType = claim.TenureContractType.HasValue ? (int?)claim.TenureContractType.Value : null
        };

        var changedFields = new List<string>();

        // 5. Update relation fields (partial update — only provided fields change)
        bool relationFieldsProvided = request.RelationType.HasValue
            || request.OccupancyType.HasValue || request.ClearOccupancyType
            || request.OwnershipShare.HasValue || request.ClearOwnershipShare
            || request.ContractDetails != null || request.ClearContractDetails
            || request.Notes != null || request.ClearNotes;

        RelationType effectiveRelationType = relation.RelationType;

        if (relationFieldsProvided)
        {
            effectiveRelationType = request.RelationType.HasValue
                ? (RelationType)request.RelationType.Value
                : relation.RelationType;

            var effectiveOccupancyType = request.ClearOccupancyType
                ? (OccupancyType?)null
                : request.OccupancyType.HasValue
                    ? (OccupancyType)request.OccupancyType.Value
                    : relation.OccupancyType;

            var effectiveOwnershipShare = request.ClearOwnershipShare
                ? (decimal?)null
                : request.OwnershipShare ?? relation.OwnershipShare;

            var effectiveContractDetails = request.ClearContractDetails
                ? null
                : request.ContractDetails ?? relation.ContractDetails;

            var effectiveNotes = request.ClearNotes
                ? null
                : request.Notes ?? relation.Notes;

            // Track changed fields
            if (request.RelationType.HasValue && (RelationType)request.RelationType.Value != relation.RelationType)
                changedFields.Add("RelationType");
            if (request.OccupancyType.HasValue || request.ClearOccupancyType)
                changedFields.Add("OccupancyType");
            if (request.OwnershipShare.HasValue || request.ClearOwnershipShare)
                changedFields.Add("OwnershipShare");
            if (request.ContractDetails != null || request.ClearContractDetails)
                changedFields.Add("ContractDetails");
            if (request.Notes != null || request.ClearNotes)
                changedFields.Add("Notes");

            relation.UpdateRelationDetails(
                effectiveRelationType,
                effectiveOccupancyType,
                relation.HasEvidence, // preserve current value — updated in step 7
                effectiveOwnershipShare,
                effectiveContractDetails,
                effectiveNotes,
                currentUserId);

            await _unitOfWork.PersonPropertyRelations.UpdateAsync(relation, cancellationToken);
        }
        else if (request.RelationType.HasValue)
        {
            effectiveRelationType = (RelationType)request.RelationType.Value;
        }

        // 6. Process evidence operations
        int newLinksCreated = 0;
        int existingLinksCreated = 0;
        int linksDeactivated = 0;

        // 6a. Create new evidence and link to relation
        if (request.NewEvidence != null)
        {
            foreach (var newEv in request.NewEvidence)
            {
                var evidence = Evidence.Create(
                    (EvidenceType)newEv.EvidenceType,
                    newEv.Description,
                    newEv.OriginalFileName,
                    newEv.FilePath,
                    newEv.FileSizeBytes,
                    newEv.MimeType,
                    newEv.FileHash,
                    currentUserId);

                evidence.LinkToClaim(claim.Id, currentUserId);

                // Apply optional metadata
                if (newEv.DocumentIssuedDate.HasValue || newEv.DocumentExpiryDate.HasValue
                    || newEv.IssuingAuthority != null || newEv.DocumentReferenceNumber != null)
                {
                    evidence.UpdateMetadata(
                        newEv.DocumentIssuedDate,
                        newEv.DocumentExpiryDate,
                        newEv.IssuingAuthority,
                        newEv.DocumentReferenceNumber,
                        null, // notes
                        currentUserId);
                }

                await _unitOfWork.Evidences.AddAsync(evidence, cancellationToken);

                var link = EvidenceRelation.Create(
                    evidence.Id,
                    relation.Id,
                    currentUserId,
                    newEv.LinkReason);

                await _unitOfWork.EvidenceRelations.AddAsync(link, cancellationToken);
                newLinksCreated++;
            }

            changedFields.Add("NewEvidence");
        }

        // 6b. Link existing evidence to relation
        if (request.LinkExistingEvidenceIds != null)
        {
            foreach (var evidenceId in request.LinkExistingEvidenceIds)
            {
                var evidence = await _unitOfWork.Evidences.GetByIdAsync(evidenceId, cancellationToken)
                    ?? throw new NotFoundException($"Evidence with ID {evidenceId} not found");

                // Check for existing link
                var existingLink = await _unitOfWork.EvidenceRelations
                    .GetByEvidenceAndRelationAsync(evidenceId, relation.Id, cancellationToken);

                if (existingLink != null && existingLink.IsActive)
                    continue; // already linked, skip

                if (existingLink != null && !existingLink.IsActive)
                {
                    existingLink.Reactivate(currentUserId, request.ReasonForModification);
                    await _unitOfWork.EvidenceRelations.UpdateAsync(existingLink, cancellationToken);
                }
                else
                {
                    var link = EvidenceRelation.Create(
                        evidenceId,
                        relation.Id,
                        currentUserId,
                        $"Linked via claim update: {request.ReasonForModification}");

                    await _unitOfWork.EvidenceRelations.AddAsync(link, cancellationToken);
                }

                existingLinksCreated++;
            }

            changedFields.Add("LinkedEvidence");
        }

        // 6c. Unlink evidence relations
        if (request.UnlinkEvidenceRelationIds != null)
        {
            foreach (var erIds in request.UnlinkEvidenceRelationIds)
            {
                var er = await _unitOfWork.EvidenceRelations.GetByIdAsync(erIds, cancellationToken)
                    ?? throw new NotFoundException($"EvidenceRelation with ID {erIds} not found");

                if (er.PersonPropertyRelationId != relation.Id)
                    throw new ValidationException(
                        $"EvidenceRelation {erIds} does not belong to this claim's source relation.");

                er.Deactivate(currentUserId, request.ReasonForModification);
                await _unitOfWork.EvidenceRelations.UpdateAsync(er, cancellationToken);
                linksDeactivated++;
            }

            changedFields.Add("UnlinkedEvidence");
        }

        // 7. Recompute HasEvidence using in-memory state
        // Query DB for existing active links, then adjust for in-memory adds/removes
        var existingActiveLinks = (await _unitOfWork.EvidenceRelations
            .GetActiveByRelationIdAsync(relation.Id, cancellationToken)).ToList();

        var unlinkIds = request.UnlinkEvidenceRelationIds ?? new List<Guid>();
        var remainingActiveCount = existingActiveLinks.Count(er => !unlinkIds.Contains(er.Id));
        var totalActiveCount = remainingActiveCount + newLinksCreated + existingLinksCreated;
        var hasEvidence = totalActiveCount > 0;

        relation.SetHasEvidence(hasEvidence, currentUserId);

        // 8. Derive claim state from relation type
        claim.DeriveStateFromRelation(effectiveRelationType, currentUserId);

        if ((int)claim.ClaimType != oldValues.ClaimType)
            changedFields.Add("ClaimType");
        if ((int)claim.CaseStatus != oldValues.CaseStatus)
            changedFields.Add("CaseStatus");

        // 9. Update TenureContractType if provided (claim-level)
        if (request.TenureContractType.HasValue)
        {
            claim.UpdateTenureContract(
                (TenureContractType)request.TenureContractType.Value,
                request.TenureContractDetails,
                currentUserId);

            changedFields.Add("TenureContractType");
            if (!string.IsNullOrWhiteSpace(request.TenureContractDetails))
                changedFields.Add("TenureContractDetails");
        }

        // 10. Persist atomically
        await _unitOfWork.Claims.UpdateAsync(claim, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 11. Audit log
        var newValues = new
        {
            RelationType = (int)effectiveRelationType,
            OccupancyType = relation.OccupancyType.HasValue ? (int?)relation.OccupancyType.Value : null,
            relation.OwnershipShare,
            relation.ContractDetails,
            relation.Notes,
            HasEvidence = hasEvidence,
            ClaimType = (int)claim.ClaimType,
            CaseStatus = (int)claim.CaseStatus,
            TenureContractType = claim.TenureContractType.HasValue ? (int?)claim.TenureContractType.Value : null,
            RelationCreated = relationWasCreated,
            NewEvidenceCount = newLinksCreated,
            LinkedEvidenceCount = existingLinksCreated,
            UnlinkedEvidenceCount = linksDeactivated
        };

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

        // 12. Build result
        return new UpdateClaimResultDto
        {
            Claim = _mapper.Map<ClaimDto>(claim),
            SourceRelationId = relation.Id,
            RelationType = (int)effectiveRelationType,
            HasEvidence = hasEvidence,
            ActiveEvidenceLinkCount = totalActiveCount
        };
    }
}
