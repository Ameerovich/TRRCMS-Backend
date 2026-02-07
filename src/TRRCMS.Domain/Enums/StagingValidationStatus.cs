namespace TRRCMS.Domain.Enums;

/// <summary>
/// Validation status for staging records during .uhc import pipeline.
/// Tracks each record through the validation lifecycle before commit to production.
/// Referenced in all Staging*Configuration files and UC-003 Stage 2 (S13).
/// </summary>
public enum StagingValidationStatus
{
    /// <summary>
    /// Pending validation — record staged but not yet validated (قيد الانتظار)
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Valid — record passed all validation checks (صالح)
    /// </summary>
    Valid = 1,

    /// <summary>
    /// Invalid — record has blocking validation errors (غير صالح)
    /// Cannot be committed to production until errors are resolved.
    /// </summary>
    Invalid = 2,

    /// <summary>
    /// Warning — record has non-blocking warnings but can still be committed (تحذير)
    /// Data manager may review and approve despite warnings.
    /// </summary>
    Warning = 3,

    /// <summary>
    /// Skipped — record intentionally excluded from commit (تم التخطي)
    /// Examples: duplicate already in production, superseded by newer data.
    /// </summary>
    Skipped = 4
}
