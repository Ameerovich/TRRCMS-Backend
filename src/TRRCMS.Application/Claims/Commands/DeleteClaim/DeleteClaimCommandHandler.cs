using MediatR;
using TRRCMS.Application.Common.Exceptions;
using TRRCMS.Application.Common.Extensions;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Application.Common.Models;

namespace TRRCMS.Application.Claims.Commands.DeleteClaim;

public class DeleteClaimCommandHandler : IRequestHandler<DeleteClaimCommand, DeleteResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditService _auditService;

    public DeleteClaimCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuditService auditService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }

    public async Task<DeleteResultDto> Handle(DeleteClaimCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // Get claim
        var claim = await _unitOfWork.Claims.GetByIdAsync(request.ClaimId, cancellationToken)
            ?? throw new NotFoundException($"Claim with ID {request.ClaimId} not found");

        if (claim.IsDeleted)
            throw new ValidationException("Claim is already deleted");

        // Soft delete the claim
        claim.MarkAsDeleted(currentUserId);
        await _unitOfWork.Claims.UpdateAsync(claim, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Audit log
        await _auditService.LogClaimDeletedAsync(
            claim.Id,
            claim.ClaimNumber,
            request.DeletionReason,
            cancellationToken);

        return new DeleteResultDto
        {
            PrimaryEntityId = claim.Id,
            PrimaryEntityType = "Claim",
            AffectedEntities = new List<DeletedEntityInfo>
            {
                new()
                {
                    EntityId = claim.Id,
                    EntityType = "Claim",
                    EntityIdentifier = claim.ClaimNumber
                }
            },
            TotalAffected = 1,
            DeletedAtUtc = claim.DeletedAtUtc!.Value,
            Message = $"Claim {claim.ClaimNumber} deleted successfully"
        };
    }
}
