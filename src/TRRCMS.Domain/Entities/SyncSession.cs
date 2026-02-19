using TRRCMS.Domain.Common;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Domain.Entities;

public class SyncSession : BaseAuditableEntity
{
    public Guid FieldCollectorId { get; private set; }
    public string DeviceId { get; private set; } = null!;
    public string? ServerIpAddress { get; private set; }

    public SyncSessionStatus SessionStatus { get; private set; }
    public DateTime StartedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }

    public int PackagesUploaded { get; private set; }
    public int PackagesFailed { get; private set; }
    public int AssignmentsDownloaded { get; private set; }
    public int AssignmentsAcknowledged { get; private set; }

    public string? VocabularyVersionsSent { get; private set; } // JSON
    public string? ErrorMessage { get; private set; }

    private SyncSession() : base() { }

    public static SyncSession Create(Guid fieldCollectorId, string deviceId, string? serverIpAddress, Guid createdBy)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("DeviceId is required.", nameof(deviceId));

        var session = new SyncSession
        {
            FieldCollectorId = fieldCollectorId,
            DeviceId = deviceId.Trim(),
            ServerIpAddress = serverIpAddress,
            SessionStatus = SyncSessionStatus.InProgress,
            StartedAtUtc = DateTime.UtcNow,
            PackagesUploaded = 0,
            PackagesFailed = 0,
            AssignmentsDownloaded = 0,
            AssignmentsAcknowledged = 0
        };

        session.MarkAsCreated(createdBy);
        return session;
    }

    public void RecordUploadResult(bool success)
    {
        if (success) PackagesUploaded++;
        else PackagesFailed++;

        MarkAsModified(CreatedBy);
    }

    public void RecordDownloadResult(int assignmentsCount, string? vocabVersionsSentJson)
    {
        if (assignmentsCount < 0) assignmentsCount = 0;
        AssignmentsDownloaded += assignmentsCount;
        VocabularyVersionsSent = vocabVersionsSentJson;

        MarkAsModified(CreatedBy);
    }

    public void RecordAcknowledgment(int assignmentsCount)
    {
        if (assignmentsCount < 0) assignmentsCount = 0;
        AssignmentsAcknowledged += assignmentsCount;

        MarkAsModified(CreatedBy);
    }

    public void MarkCompleted()
    {
        SessionStatus = (PackagesFailed > 0) ? SyncSessionStatus.PartiallyCompleted : SyncSessionStatus.Completed;
        CompletedAtUtc = DateTime.UtcNow;

        MarkAsModified(CreatedBy);
    }

    public void MarkFailed(string errorMessage)
    {
        SessionStatus = SyncSessionStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAtUtc = DateTime.UtcNow;

        MarkAsModified(CreatedBy);
    }
}
