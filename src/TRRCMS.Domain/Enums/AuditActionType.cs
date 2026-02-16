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
    [ArabicLabel("إنشاء")]
    Create = 1,

    /// <summary>
    /// Entity updated/modified (تعديل)
    /// </summary>
    [ArabicLabel("تعديل")]
    Update = 2,

    /// <summary>
    /// Entity deleted (حذف)
    /// </summary>
    [ArabicLabel("حذف")]
    Delete = 3,

    /// <summary>
    /// Entity viewed/accessed (عرض)
    /// </summary>
    [ArabicLabel("عرض")]
    View = 4,

    // ==================== WORKFLOW ACTIONS ====================

    /// <summary>
    /// Status changed (تغيير الحالة)
    /// </summary>
    [ArabicLabel("تغيير الحالة")]
    StatusChange = 10,

    /// <summary>
    /// Case submitted (تقديم)
    /// </summary>
    [ArabicLabel("تقديم")]
    Submit = 11,

    /// <summary>
    /// Case approved (موافقة)
    /// </summary>
    [ArabicLabel("موافقة")]
    Approve = 12,

    /// <summary>
    /// Case rejected (رفض)
    /// </summary>
    [ArabicLabel("رفض")]
    Reject = 13,

    /// <summary>
    /// Case reassigned (إعادة تعيين)
    /// </summary>
    [ArabicLabel("إعادة تعيين")]
    Reassign = 14,

    /// <summary>
    /// Case escalated (تصعيد)
    /// </summary>
    [ArabicLabel("تصعيد")]
    Escalate = 15,

    /// <summary>
    /// Case referred (إحالة)
    /// </summary>
    [ArabicLabel("إحالة")]
    Refer = 16,

    // ==================== NEW: ADDITIONAL WORKFLOW ACTIONS ====================

    /// <summary>
    /// Case assigned to user (تعيين)
    /// </summary>
    [ArabicLabel("تعيين")]
    Assign = 17,

    /// <summary>
    /// Case/document verified (التحقق والمصادقة)
    /// </summary>
    [ArabicLabel("التحقق والمصادقة")]
    Verify = 18,

    // ==================== DATA OPERATIONS ====================

    /// <summary>
    /// Data imported (.uhc container) (استيراد)
    /// </summary>
    [ArabicLabel("استيراد")]
    Import = 20,

    /// <summary>
    /// Data exported (تصدير)
    /// </summary>
    [ArabicLabel("تصدير")]
    Export = 21,

    /// <summary>
    /// Data merged (duplicate resolution) (دمج)
    /// </summary>
    [ArabicLabel("دمج")]
    Merge = 22,

    /// <summary>
    /// Data validated (تحقق)
    /// </summary>
    [ArabicLabel("تحقق")]
    Validate = 23,

    /// <summary>
    /// Data synchronized (tablet sync) (مزامنة)
    /// </summary>
    [ArabicLabel("مزامنة")]
    Synchronize = 24,

    // ==================== DOCUMENT OPERATIONS ====================

    /// <summary>
    /// Document uploaded (رفع مستند)
    /// </summary>
    [ArabicLabel("رفع مستند")]
    DocumentUpload = 30,

    /// <summary>
    /// Document downloaded (تنزيل مستند)
    /// </summary>
    [ArabicLabel("تنزيل مستند")]
    DocumentDownload = 31,

    /// <summary>
    /// Document verified (توثيق مستند)
    /// </summary>
    [ArabicLabel("توثيق مستند")]
    DocumentVerify = 32,

    /// <summary>
    /// Document deleted (حذف مستند)
    /// </summary>
    [ArabicLabel("حذف مستند")]
    DocumentDelete = 33,

    // ==================== AUTHENTICATION & AUTHORIZATION ====================

    /// <summary>
    /// User login (تسجيل دخول)
    /// </summary>
    [ArabicLabel("تسجيل دخول")]
    Login = 40,

    /// <summary>
    /// User logout (تسجيل خروج)
    /// </summary>
    [ArabicLabel("تسجيل خروج")]
    Logout = 41,

    /// <summary>
    /// Login failed (فشل تسجيل الدخول)
    /// </summary>
    [ArabicLabel("فشل تسجيل الدخول")]
    LoginFailed = 42,

    /// <summary>
    /// Password changed (تغيير كلمة المرور)
    /// </summary>
    [ArabicLabel("تغيير كلمة المرور")]
    PasswordChange = 43,

    /// <summary>
    /// Permission granted (منح صلاحية)
    /// </summary>
    [ArabicLabel("منح صلاحية")]
    PermissionGranted = 44,

    /// <summary>
    /// Permission revoked (إلغاء صلاحية)
    /// </summary>
    [ArabicLabel("إلغاء صلاحية")]
    PermissionRevoked = 45,

    /// <summary>
    /// Access denied - unauthorized attempt (رفض الوصول)
    /// </summary>
    [ArabicLabel("رفض الوصول")]
    AccessDenied = 46,

    // ==================== CONFIGURATION ====================

    /// <summary>
    /// System configuration changed (تغيير إعدادات النظام)
    /// </summary>
    [ArabicLabel("تغيير إعدادات النظام")]
    ConfigurationChange = 50,

    /// <summary>
    /// Vocabulary updated (تحديث المفردات)
    /// </summary>
    [ArabicLabel("تحديث المفردات")]
    VocabularyUpdate = 51,

    /// <summary>
    /// User created (إنشاء مستخدم)
    /// </summary>
    [ArabicLabel("إنشاء مستخدم")]
    UserCreated = 52,

    /// <summary>
    /// User deactivated (تعطيل مستخدم)
    /// </summary>
    [ArabicLabel("تعطيل مستخدم")]
    UserDeactivated = 53,

    /// <summary>
    /// Role assigned (تعيين دور)
    /// </summary>
    [ArabicLabel("تعيين دور")]
    RoleAssigned = 54,

    // ==================== REPORTS ====================

    /// <summary>
    /// Report generated (إنشاء تقرير)
    /// </summary>
    [ArabicLabel("إنشاء تقرير")]
    ReportGenerated = 60,

    /// <summary>
    /// Report downloaded (تنزيل تقرير)
    /// </summary>
    [ArabicLabel("تنزيل تقرير")]
    ReportDownloaded = 61,

    // ==================== CERTIFICATE ====================

    /// <summary>
    /// Certificate issued (إصدار شهادة)
    /// </summary>
    [ArabicLabel("إصدار شهادة")]
    CertificateIssued = 70,

    /// <summary>
    /// Certificate voided (إلغاء شهادة)
    /// </summary>
    [ArabicLabel("إلغاء شهادة")]
    CertificateVoided = 71,

    /// <summary>
    /// Certificate reissued (إعادة إصدار شهادة)
    /// </summary>
    [ArabicLabel("إعادة إصدار شهادة")]
    CertificateReissued = 72,

    // ==================== BACKUP & RECOVERY ====================

    /// <summary>
    /// Backup created (إنشاء نسخة احتياطية)
    /// </summary>
    [ArabicLabel("إنشاء نسخة احتياطية")]
    BackupCreated = 80,

    /// <summary>
    /// Data restored (استعادة بيانات)
    /// </summary>
    [ArabicLabel("استعادة بيانات")]
    DataRestored = 81,

    // ==================== OTHER ====================

    /// <summary>
    /// Comment added (إضافة تعليق)
    /// </summary>
    [ArabicLabel("إضافة تعليق")]
    CommentAdded = 90,

    /// <summary>
    /// Notification sent (إرسال إشعار)
    /// </summary>
    [ArabicLabel("إرسال إشعار")]
    NotificationSent = 91,

    /// <summary>
    /// Email sent (إرسال بريد إلكتروني)
    /// </summary>
    [ArabicLabel("إرسال بريد إلكتروني")]
    EmailSent = 92,

    /// <summary>
    /// Print action (طباعة)
    /// </summary>
    [ArabicLabel("طباعة")]
    Print = 93,

    /// <summary>
    /// StateTransition action (تغيير الحالة)
    /// </summary>
    [ArabicLabel("انتقال حالة")]
    StateTransition = 94,

    // ==================== NEW: CONFLICT MANAGEMENT ====================

    /// <summary>
    /// Conflict detected between claims (اكتشاف تعارض)
    /// </summary>
    [ArabicLabel("اكتشاف تعارض")]
    ConflictDetected = 95,

    /// <summary>
    /// Conflict resolved (حل التعارض)
    /// </summary>
    [ArabicLabel("حل التعارض")]
    ConflictResolved = 96,

    /// <summary>
    /// Other action not categorized
    /// </summary>

    /// <summary>
    /// File/Evidence uploaded (رفع ملف/دليل)
    /// </summary>
    [ArabicLabel("رفع ملف")]
    Upload = 97,

    /// <summary>
    /// Grand Permision(s) to a user (منح تصريح)
    /// </summary>
    [ArabicLabel("منح تصريح")]
    PermissionGrant = 98,

    /// <summary>
    /// Revoke Permision from a user (سحب تصريح)
    /// </summary>
    [ArabicLabel("سحب تصريح")]
    PermissionRevoke = 99,

    [ArabicLabel("أخرى")]
    Other = 999,
}
