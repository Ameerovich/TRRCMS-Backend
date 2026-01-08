using MediatR;
using TRRCMS.Application.Documents.Dtos;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Documents.Commands.CreateDocument;

/// <summary>
/// Command to create a new document
/// </summary>
public class CreateDocumentCommand : IRequest<DocumentDto>
{
    // ==================== REQUIRED FIELDS ====================

    /// <summary>
    /// Document type (required)
    /// </summary>
    public DocumentType DocumentType { get; set; }

    /// <summary>
    /// User creating this document (required)
    /// </summary>
    public Guid CreatedByUserId { get; set; }

    // ==================== OPTIONAL FIELDS ====================

    /// <summary>
    /// Document number/reference (optional)
    /// Example: Tabu number, ID number, contract number
    /// </summary>
    public string? DocumentNumber { get; set; }

    /// <summary>
    /// Document title or description (optional)
    /// </summary>
    public string? DocumentTitle { get; set; }

    /// <summary>
    /// Date when document was issued (optional)
    /// </summary>
    public DateTime? IssueDate { get; set; }

    /// <summary>
    /// Date when document expires (optional)
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Issuing authority/organization (optional)
    /// </summary>
    public string? IssuingAuthority { get; set; }

    /// <summary>
    /// Place where document was issued (optional)
    /// </summary>
    public string? IssuingPlace { get; set; }

    /// <summary>
    /// Additional notes (optional)
    /// </summary>
    public string? Notes { get; set; }

    // ==================== OPTIONAL LINKING ====================

    /// <summary>
    /// Link to Evidence (optional) - the actual file/scan
    /// </summary>
    public Guid? EvidenceId { get; set; }

    /// <summary>
    /// SHA-256 hash of the document (optional)
    /// </summary>
    public string? DocumentHash { get; set; }

    /// <summary>
    /// Link to Person (optional) - if document belongs to a person
    /// </summary>
    public Guid? PersonId { get; set; }

    /// <summary>
    /// Link to PropertyUnit (optional) - if document relates to property
    /// </summary>
    public Guid? PropertyUnitId { get; set; }

    /// <summary>
    /// Link to PersonPropertyRelation (optional) - if document proves a relation
    /// </summary>
    public Guid? PersonPropertyRelationId { get; set; }

    /// <summary>
    /// Link to Claim (optional) - if document supports a claim
    /// </summary>
    public Guid? ClaimId { get; set; }
}
