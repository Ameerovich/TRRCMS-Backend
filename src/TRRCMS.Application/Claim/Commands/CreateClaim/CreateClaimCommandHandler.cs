using AutoMapper;
using MediatR;
using TRRCMS.Application.Claims.Dtos;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Claims.Commands.CreateClaim;

/// <summary>
/// Handler for CreateClaimCommand
/// Creates new claim with automatic conflict detection
/// </summary>
public class CreateClaimCommandHandler : IRequestHandler<CreateClaimCommand, ClaimDto>
{
    private readonly IClaimRepository _claimRepository;
    private readonly IPropertyUnitRepository _propertyUnitRepository;
    private readonly IMapper _mapper;
    
    public CreateClaimCommandHandler(
        IClaimRepository claimRepository,
        IPropertyUnitRepository propertyUnitRepository,
        IMapper mapper)
    {
        _claimRepository = claimRepository ?? throw new ArgumentNullException(nameof(claimRepository));
        _propertyUnitRepository = propertyUnitRepository ?? throw new ArgumentNullException(nameof(propertyUnitRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }
    
    public async Task<ClaimDto> Handle(CreateClaimCommand request, CancellationToken cancellationToken)
    {
        // Validate that property unit exists
        var propertyUnitExists = await _propertyUnitRepository.ExistsAsync(request.PropertyUnitId, cancellationToken);
        if (!propertyUnitExists)
        {
            throw new InvalidOperationException($"Property unit with ID {request.PropertyUnitId} does not exist.");
        }
        
        // Create claim using factory method
        var claim = Claim.Create(
            propertyUnitId: request.PropertyUnitId,
            primaryClaimantId: request.PrimaryClaimantId,
            claimType: request.ClaimType,
            claimSource: request.ClaimSource,
            createdByUserId: request.CreatedByUserId
        );

        // Set priority if different from default
        if (request.Priority != CasePriority.Normal)
        {
            claim.SetPriority(request.Priority, request.CreatedByUserId);
        }

        // Set optional tenure details if provided
        if (request.TenureContractType.HasValue || request.OwnershipShare.HasValue 
            || request.TenureStartDate.HasValue || request.TenureEndDate.HasValue)
        {
            claim.UpdateTenureDetails(
                tenureType: request.TenureContractType,
                ownershipShare: request.OwnershipShare,
                tenureStartDate: request.TenureStartDate,
                tenureEndDate: request.TenureEndDate,
                modifiedByUserId: request.CreatedByUserId
            );
        }
        
        // Set optional description fields if provided
        if (!string.IsNullOrWhiteSpace(request.ClaimDescription) 
            || !string.IsNullOrWhiteSpace(request.LegalBasis)
            || !string.IsNullOrWhiteSpace(request.SupportingNarrative))
        {
            claim.UpdateDescription(
                claimDescription: request.ClaimDescription,
                legalBasis: request.LegalBasis,
                supportingNarrative: request.SupportingNarrative,
                modifiedByUserId: request.CreatedByUserId
            );
        }
        
        // Set target completion date if provided
        if (request.TargetCompletionDate.HasValue)
        {
            claim.AssignTo(
                userId: request.CreatedByUserId, // Assign to creator initially
                targetCompletionDate: request.TargetCompletionDate,
                modifiedByUserId: request.CreatedByUserId
            );
        }
        
        // Add processing notes if provided
        if (!string.IsNullOrWhiteSpace(request.ProcessingNotes))
        {
            claim.AddProcessingNotes(request.ProcessingNotes, request.CreatedByUserId);
        }
        
        // Add public remarks if provided
        if (!string.IsNullOrWhiteSpace(request.PublicRemarks))
        {
            claim.AddPublicRemarks(request.PublicRemarks, request.CreatedByUserId);
        }
        
        // Check for conflicting claims on the same property unit
        var conflictCount = await _claimRepository.GetConflictCountAsync(
            request.PropertyUnitId, 
            cancellationToken
        );
        
        if (conflictCount > 0)
        {
            // Mark conflicts detected (adding this new claim will create conflicts)
            claim.MarkConflictsDetected(
                conflictCount: conflictCount, // Existing claims count
                modifiedByUserId: request.CreatedByUserId
            );
        }
        
        // Save claim to repository
        await _claimRepository.AddAsync(claim, cancellationToken);
        await _claimRepository.SaveChangesAsync(cancellationToken);
        
        // If conflicts exist, update existing claims to reflect the conflict
        if (conflictCount > 0)
        {
            var existingClaim = await _claimRepository.GetByPropertyUnitIdAsync(
                request.PropertyUnitId, 
                cancellationToken
            );
            
            if (existingClaim != null && existingClaim.Id != claim.Id)
            {
                existingClaim.MarkConflictsDetected(
                    conflictCount: conflictCount + 1, // Now includes the new claim
                    modifiedByUserId: request.CreatedByUserId
                );
                await _claimRepository.UpdateAsync(existingClaim, cancellationToken);
                await _claimRepository.SaveChangesAsync(cancellationToken);
            }
        }
        
        // Reload claim with navigation properties
        var createdClaim = await _claimRepository.GetByIdAsync(claim.Id, cancellationToken);
        
        // Map to DTO and return
        return _mapper.Map<ClaimDto>(createdClaim);
    }
}
