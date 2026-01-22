using MediatR;
using Microsoft.AspNetCore.Http;
using TRRCMS.Application.Evidences.Dtos;

namespace TRRCMS.Application.Surveys.Commands.UploadPropertyPhoto;

/// <summary>
/// Command to upload property photo in survey context
/// Photos can be general property photos or linked to specific property units
/// </summary>
public class UploadPropertyPhotoCommand : IRequest<EvidenceDto>
{
    /// <summary>
    /// Survey ID for authorization and organization
    /// </summary>
    public Guid SurveyId { get; set; }

    /// <summary>
    /// Property unit ID to link photo to (optional - can link later)
    /// </summary>
    public Guid? PropertyUnitId { get; set; }

    /// <summary>
    /// Person-Property relation ID if linking to specific ownership/tenancy (optional)
    /// </summary>
    public Guid? PersonPropertyRelationId { get; set; }

    /// <summary>
    /// Photo file to upload
    /// </summary>
    public IFormFile File { get; set; } = null!;

    /// <summary>
    /// Photo description (e.g., "Front facade", "Interior - living room", "Damage to roof")
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Additional notes about the photo
    /// </summary>
    public string? Notes { get; set; }
}