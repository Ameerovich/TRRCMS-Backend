namespace TRRCMS.Domain.Enums;

/// <summary>
/// Audit action type for comprehensive audit trail
/// </summary>
public enum AuditActionType
{
    // ==================== ENTITY CRUD OPERATIONS ====================

    /// <summary>
    /// Entity created (إنشاء)
    /// </summary>
    Create = 1,

    /// <summary>
    /// Entity updated/modified (تعديل)
    /// </summary>
    Update = 2,

    /// <summary>
    /// Entity deleted (حذف)
    /// </summary>
    Delete = 3,

    /// <summary>
    /// Entity viewed/accessed (عرض)
    /// </summary>
    View = 4,

    // ==================== WORKFLOW ACTIONS ====================

    /// <summary>
    /// Status changed (تغيير الحالة)
    /// </summary>
    StatusChange = 10,

    /// <summary>
    /// Case submitted (تقديم)
    /// </summary>
    Submit = 11,

    /// <summary>
    /// Case approved (موافقة)
    /// </summary>
    Approve = 12,

    /// <summary>
    /// Case rejected (رفض)
    /// </summary>
    Reject = 13,

    /// <summary>
    /// Case reassigned (إعادة تعيين)
    /// </summary>
    Reassign = 14,

    /// <summary>
    /// Case escalated (تصعيد)
    /// </summary>
    Escalate = 15,

    /// <summary>
    /// Case referred (إحالة)
    /// </summary>
    Refer = 16,

    // ==================== NEW: ADDITIONAL WORKFLOW ACTIONS ====================

    /// <summary>
    /// Case assigned to user (تعيين)
    /// </summary>
    Assign = 17,

    /// <summary>
    /// Case/document verified (التحقق والمصادقة)
    /// </summary>
    Verify = 18,

    // ==================== DATA OPERATIONS ====================

    /// <summary>
    /// Data imported (.uhc container) (استيراد)
    /// </summary>
    Import = 20,

    /// <summary>
    /// Data exported (تصدير)
    /// </summary>
    Export = 21,

    /// <summary>
    /// Data merged (duplicate resolution) (دمج)
    /// </summary>
    Merge = 22,

    /// <summary>
    /// Data validated (تحقق)
    /// </summary>
    Validate = 23,

    /// <summary>
    /// Data synchronized (tablet sync) (مزامنة)
    /// </summary>
    Synchronize = 24,

    // ==================== DOCUMENT OPERATIONS ====================

    /// <summary>
    /// Document uploaded (رفع مستند)
    /// </summary>
    DocumentUpload = 30,

    /// <summary>
    /// Document downloaded (تنزيل مستند)
    /// </summary>
    DocumentDownload = 31,

    /// <summary>
    /// Document verified (توثيق مستند)
    /// </summary>
    DocumentVerify = 32,

    /// <summary>
    /// Document deleted (حذف مستند)
    /// </summary>
    DocumentDelete = 33,

    // ==================== AUTHENTICATION & AUTHORIZATION ====================

    /// <summary>
    /// User login (تسجيل دخول)
    /// </summary>
    Login = 40,

    /// <summary>
    /// User logout (تسجيل خروج)
    /// </summary>
    Logout = 41,

    /// <summary>
    /// Login failed (فشل تسجيل الدخول)
    /// </summary>
    LoginFailed = 42,

    /// <summary>
    /// Password changed (تغيير كلمة المرور)
    /// </summary>
    PasswordChange = 43,

    /// <summary>
    /// Permission granted (منح صلاحية)
    /// </summary>
    PermissionGranted = 44,

    /// <summary>
    /// Permission revoked (إلغاء صلاحية)
    /// </summary>
    PermissionRevoked = 45,

    /// <summary>
    /// Access denied - unauthorized attempt (رفض الوصول)
    /// </summary>
    AccessDenied = 46,

    // ==================== CONFIGURATION ====================

    /// <summary>
    /// System configuration changed (تغيير إعدادات النظام)
    /// </summary>
    ConfigurationChange = 50,

    /// <summary>
    /// Vocabulary updated (تحديث المفردات)
    /// </summary>
    VocabularyUpdate = 51,

    /// <summary>
    /// User created (إنشاء مستخدم)
    /// </summary>
    UserCreated = 52,

    /// <summary>
    /// User deactivated (تعطيل مستخدم)
    /// </summary>
    UserDeactivated = 53,

    /// <summary>
    /// Role assigned (تعيين دور)
    /// </summary>
    RoleAssigned = 54,

    // ==================== REPORTS ====================

    /// <summary>
    /// Report generated (إنشاء تقرير)
    /// </summary>
    ReportGenerated = 60,

    /// <summary>
    /// Report downloaded (تنزيل تقرير)
    /// </summary>
    ReportDownloaded = 61,

    // ==================== CERTIFICATE ====================

    /// <summary>
    /// Certificate issued (إصدار شهادة)
    /// </summary>
    CertificateIssued = 70,

    /// <summary>
    /// Certificate voided (إلغاء شهادة)
    /// </summary>
    CertificateVoided = 71,

    /// <summary>
    /// Certificate reissued (إعادة إصدار شهادة)
    /// </summary>
    CertificateReissued = 72,

    // ==================== BACKUP & RECOVERY ====================

    /// <summary>
    /// Backup created (إنشاء نسخة احتياطية)
    /// </summary>
    BackupCreated = 80,

    /// <summary>
    /// Data restored (استعادة بيانات)
    /// </summary>
    DataRestored = 81,

    // ==================== OTHER ====================

    /// <summary>
    /// Comment added (إضافة تعليق)
    /// </summary>
    CommentAdded = 90,

    /// <summary>
    /// Notification sent (إرسال إشعار)
    /// </summary>
    NotificationSent = 91,

    /// <summary>
    /// Email sent (إرسال بريد إلكتروني)
    /// </summary>
    EmailSent = 92,

    /// <summary>
    /// Print action (طباعة)
    /// </summary   
    Print = 93,

    /// <summary>
    /// StateTransition action (تغيير الحالة)
    /// </summary>
    StateTransition = 94,

    // ==================== NEW: CONFLICT MANAGEMENT ====================

    /// <summary>
    /// Conflict detected between claims (اكتشاف تعارض)
    /// </summary>
    ConflictDetected = 95,

    /// <summary>
    /// Conflict resolved (حل التعارض)
    /// </summary>
    ConflictResolved = 96,

    /// <summary>
    /// Other action not categorized
    /// </summary>
    /// 

    /// <summary>
    /// File/Evidence uploaded (رفع ملف/دليل)
    /// </summary>
    Upload = 97,

    /// <summary>
    /// Grand Permision(s) to a user (منح تصريح)
    /// </summary>
    PermissionGrant = 98,

    /// <summary>
    /// Revoke Permision from a user (سحب تصريح)
    /// </summary>

    PermissionRevoke = 99,


    Other = 999,
}