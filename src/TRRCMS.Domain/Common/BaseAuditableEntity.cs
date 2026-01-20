namespace TRRCMS.Domain.Common;

/// <summary>
/// Base auditable entity with comprehensive audit trail.
/// </summary>
public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTime CreatedAtUtc { get; private set; }
    public Guid CreatedBy { get; private set; }

    public DateTime? LastModifiedAtUtc { get; private set; }
    public Guid? LastModifiedBy { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }
    public Guid? DeletedBy { get; private set; }

    protected BaseAuditableEntity() : base()
    {
        IsDeleted = false;  // ✅ ADD THIS LINE
    }

    protected BaseAuditableEntity(Guid id) : base(id)
    {
        IsDeleted = false; 
    }

    public void MarkAsCreated(Guid userId)
    {
        CreatedAtUtc = DateTime.UtcNow;
        CreatedBy = userId;
        IsDeleted = false; 
    }

    public void MarkAsModified(Guid userId)
    {
        LastModifiedAtUtc = DateTime.UtcNow;
        LastModifiedBy = userId;
    }

    public void MarkAsDeleted(Guid userId)
    {
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
        DeletedBy = userId;
        MarkAsModified(userId);
    }

    public void Restore(Guid userId)
    {
        IsDeleted = false;
        DeletedAtUtc = null;
        DeletedBy = null;
        MarkAsModified(userId);
    }
}