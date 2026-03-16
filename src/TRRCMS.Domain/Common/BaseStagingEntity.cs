using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Common;

/// <summary>
/// Abstract base class for all staging entities in the import pipeline.
/// Provides common staging metadata (import package link, original entity ID,
/// validation status, and commit tracking) shared across all staging entity types.
/// </summary>
public abstract class BaseStagingEntity : BaseEntity
{
    /// <summary>
    /// FK to the ImportPackage that owns this staging record.
    /// </summary>
    public Guid ImportPackageId { get; private set; }

    /// <summary>
    /// Original entity UUID from the .uhc SQLite package.
    /// Unique per ImportPackageId to prevent duplicate staging of the same record.
    /// </summary>
    public Guid OriginalEntityId { get; private set; }

    /// <summary>
    /// Current validation status of this staging record.
    /// </summary>
    public StagingValidationStatus ValidationStatus { get; private set; }

    /// <summary>
    /// JSON array of blocking validation error messages.
    /// </summary>
    public string? ValidationErrors { get; private set; }

    /// <summary>
    /// JSON array of non-blocking validation warning messages.
    /// </summary>
    public string? ValidationWarnings { get; private set; }

    /// <summary>
    /// Whether this record has been approved for commit to production.
    /// </summary>
    public bool IsApprovedForCommit { get; private set; }

    /// <summary>
    /// Production entity ID created during commit. Null until committed.
    /// </summary>
    public Guid? CommittedEntityId { get; private set; }

    /// <summary>
    /// UTC timestamp when this record was staged (unpacked from .uhc into staging table).
    /// </summary>
    public DateTime StagedAtUtc { get; private set; }

    /// <summary>
    /// EF Core parameterless constructor.
    /// </summary>
    protected BaseStagingEntity() : base()
    {
        ValidationStatus = StagingValidationStatus.Pending;
        IsApprovedForCommit = false;
    }

    /// <summary>
    /// Initialize the common staging metadata when first creating a staging record.
    /// Called by concrete entity factory methods.
    /// </summary>
    protected void InitializeStagingMetadata(Guid importPackageId, Guid originalEntityId)
    {
        ImportPackageId = importPackageId;
        OriginalEntityId = originalEntityId;
        ValidationStatus = StagingValidationStatus.Pending;
        IsApprovedForCommit = false;
        CommittedEntityId = null;
        StagedAtUtc = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark this staging record as having passed all validation checks.
    /// </summary>
    /// <param name="warningsJson">Optional JSON array of non-blocking warnings.</param>
    public void MarkAsValid(string? warningsJson = null)
    {
        ValidationStatus = string.IsNullOrWhiteSpace(warningsJson)
            ? StagingValidationStatus.Valid
            : StagingValidationStatus.Warning;
        ValidationErrors = null;
        ValidationWarnings = warningsJson;
    }

    /// <summary>
    /// Mark this staging record as invalid due to blocking validation errors.
    /// </summary>
    /// <param name="errorsJson">JSON array of blocking error messages (required).</param>
    /// <param name="warningsJson">Optional JSON array of non-blocking warnings.</param>
    public void MarkAsInvalid(string errorsJson, string? warningsJson = null)
    {
        if (string.IsNullOrWhiteSpace(errorsJson))
            throw new ArgumentException("Validation errors JSON is required when marking as invalid.", nameof(errorsJson));

        ValidationStatus = StagingValidationStatus.Invalid;
        ValidationErrors = errorsJson;
        ValidationWarnings = warningsJson;
        IsApprovedForCommit = false; // Invalid records cannot be approved
    }

    /// <summary>
    /// Mark this staging record as skipped (intentionally excluded from commit).
    /// </summary>
    /// <param name="reason">Reason for skipping (stored in ValidationWarnings).</param>
    public void MarkAsSkipped(string? reason = null)
    {
        ValidationStatus = StagingValidationStatus.Skipped;
        IsApprovedForCommit = false;

        if (!string.IsNullOrWhiteSpace(reason))
        {
            ValidationWarnings = $"[\"Skipped: {reason.Replace("\"", "\\\"")}\"]";
        }
    }

    /// <summary>
    /// Approve this staging record for commit to production.
    /// Only Valid or Warning records can be approved.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the record is Invalid or Skipped.</exception>
    public void ApproveForCommit()
    {
        if (ValidationStatus == StagingValidationStatus.Invalid)
            throw new InvalidOperationException(
                "Cannot approve an invalid staging record for commit. Resolve validation errors first.");

        if (ValidationStatus == StagingValidationStatus.Skipped)
            throw new InvalidOperationException(
                "Cannot approve a skipped staging record for commit.");

        IsApprovedForCommit = true;
    }

    /// <summary>
    /// Revoke approval for commit (e.g. if new validation issues are discovered).
    /// </summary>
    public void RevokeApproval()
    {
        IsApprovedForCommit = false;
    }

    /// <summary>
    /// Record the production entity ID after successful commit.
    /// Establishes the staging → production traceability link.
    /// </summary>
    /// <param name="productionEntityId">ID of the production entity created during commit.</param>
    public void SetCommittedEntityId(Guid productionEntityId)
    {
        if (productionEntityId == Guid.Empty)
            throw new ArgumentException("Production entity ID cannot be empty.", nameof(productionEntityId));

        CommittedEntityId = productionEntityId;
    }

    /// <summary>
    /// Reset validation state back to Pending (e.g. for re-validation after data correction).
    /// </summary>
    public void ResetValidation()
    {
        ValidationStatus = StagingValidationStatus.Pending;
        ValidationErrors = null;
        ValidationWarnings = null;
        IsApprovedForCommit = false;
    }
}
