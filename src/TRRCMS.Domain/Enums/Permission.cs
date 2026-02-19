namespace TRRCMS.Domain.Enums;

/// <summary>
/// Granular permission system for role-based access control (RBAC)
/// Each permission represents a specific action a user can perform
/// Referenced in FSD Section 3: Stakeholders & Roles
/// </summary>
public enum Permission
{
    // ==================== CLAIM PERMISSIONS ====================

    /// <summary>
    /// View all claims in the system (Data Manager, Analyst, Admin)
    /// </summary>
    [ArabicLabel("عرض جميع المطالبات")]
    Claims_ViewAll = 1000,

    /// <summary>
    /// View only claims assigned to the current user (Field Collector, Field Supervisor)
    /// </summary>
    [ArabicLabel("عرض المطالبات المعينة")]
    Claims_ViewAssigned = 1001,

    /// <summary>
    /// Create new claims (Office Clerk, Data Manager, Admin)
    /// Field Collectors create via surveys, not direct API
    /// </summary>
    [ArabicLabel("إنشاء مطالبة")]
    Claims_Create = 1002,

    /// <summary>
    /// Update claim fields (UC-006) (Office Clerk, Data Manager, Admin)
    /// </summary>
    [ArabicLabel("تعديل مطالبة")]
    Claims_Update = 1003,

    /// <summary>
    /// Delete/archive claims (Admin only)
    /// </summary>
    [ArabicLabel("حذف مطالبة")]
    Claims_Delete = 1004,

    /// <summary>
    /// Submit claims for processing (Office Clerk, Data Manager, Field Supervisor)
    /// </summary>
    [ArabicLabel("تقديم مطالبة")]
    Claims_Submit = 1005,

    /// <summary>
    /// Assign claims to case officers (Data Manager, Admin)
    /// </summary>
    [ArabicLabel("تعيين مطالبة")]
    Claims_Assign = 1006,

    /// <summary>
    /// Reassign claims between officers (Data Manager, Admin)
    /// </summary>
    [ArabicLabel("إعادة تعيين مطالبة")]
    Claims_Reassign = 1007,

    /// <summary>
    /// Verify claims and evidence (Data Manager, Admin)
    /// </summary>
    [ArabicLabel("التحقق من مطالبة")]
    Claims_Verify = 1008,

    /// <summary>
    /// Approve claims - adjudication decision (Admin only)
    /// IMPORTANT: This approves social tenure recognition, not legal ownership
    /// </summary>
    [ArabicLabel("الموافقة على مطالبة")]
    Claims_Approve = 1009,

    /// <summary>
    /// Reject claims - adjudication decision (Admin only)
    /// </summary>
    [ArabicLabel("رفض مطالبة")]
    Claims_Reject = 1010,

    /// <summary>
    /// Transition claims between lifecycle stages (Data Manager, Admin)
    /// </summary>
    [ArabicLabel("نقل حالة مطالبة")]
    Claims_Transition = 1011,

    /// <summary>
    /// Export claims data (Field Supervisor, Data Manager, Analyst, Admin)
    /// </summary>
    [ArabicLabel("تصدير مطالبات")]
    Claims_Export = 1012,

    /// <summary>
    /// View claim history/audit trail (Data Manager, Analyst, Admin)
    /// </summary>
    [ArabicLabel("عرض سجل المطالبات")]
    Claims_ViewHistory = 1013,

    // ==================== EVIDENCE PERMISSIONS ====================

    /// <summary>
    /// View evidence (all roles except Field Collector on desktop)
    /// </summary>
    [ArabicLabel("عرض الأدلة")]
    Evidence_View = 2000,

    /// <summary>
    /// Upload evidence documents (Field Collector, Office Clerk, Data Manager, Admin)
    /// </summary>
    [ArabicLabel("رفع دليل")]
    Evidence_Upload = 2001,

    /// <summary>
    /// Verify evidence (Data Manager, Admin)
    /// </summary>
    [ArabicLabel("التحقق من دليل")]
    Evidence_Verify = 2002,

    /// <summary>
    /// Delete evidence (Admin only)
    /// </summary>
    [ArabicLabel("حذف دليل")]
    Evidence_Delete = 2003,

    // ==================== DOCUMENT PERMISSIONS ====================

    /// <summary>
    /// View sensitive personal documents (Office Clerk, Data Manager, Admin)
    /// </summary>
    [ArabicLabel("عرض المستندات الحساسة")]
    Documents_ViewSensitive = 3000,

    /// <summary>
    /// Download documents (Office Clerk, Data Manager, Admin)
    /// </summary>
    [ArabicLabel("تنزيل مستندات")]
    Documents_Download = 3001,

    /// <summary>
    /// Upload documents (Office Clerk, Data Manager, Admin)
    /// </summary>
    [ArabicLabel("رفع مستندات")]
    Documents_Upload = 3002,

    /// <summary>
    /// Delete documents (Admin only)
    /// </summary>
    [ArabicLabel("حذف مستندات")]
    Documents_Delete = 3003,

    // ==================== BUILDING PERMISSIONS ====================

    /// <summary>
    /// View buildings (all roles)
    /// </summary>
    [ArabicLabel("عرض المباني")]
    Buildings_View = 4000,

    /// <summary>
    /// Create buildings (Data Manager, Admin)
    /// </summary>
    [ArabicLabel("إنشاء مبنى")]
    Buildings_Create = 4001,

    /// <summary>
    /// Update building details (Data Manager, Admin)
    /// </summary>
    [ArabicLabel("تعديل مبنى")]
    Buildings_Update = 4002,

    /// <summary>
    /// Assign buildings to field teams (Data Manager, Admin)
    /// </summary>
    [ArabicLabel("تعيين مبنى")]
    Buildings_Assign = 4003,

    /// <summary>
    /// Delete buildings (Admin only)
    /// </summary>
    [ArabicLabel("حذف مبنى")]
    Buildings_Delete = 4004,

    // ==================== PERSON PERMISSIONS ====================

    /// <summary>
    /// View persons (all desktop users)
    /// </summary>
    [ArabicLabel("عرض الأشخاص")]
    Persons_View = 5000,

    /// <summary>
    /// Create persons (Office Clerk, Data Manager, Admin)
    /// </summary>
    [ArabicLabel("إنشاء شخص")]
    Persons_Create = 5001,

    /// <summary>
    /// Update person details (Office Clerk, Data Manager, Admin)
    /// </summary>
    [ArabicLabel("تعديل شخص")]
    Persons_Update = 5002,

    /// <summary>
    /// Merge duplicate persons (Data Manager, Admin)
    /// Requires UC-008 duplicate resolution
    /// </summary>
    [ArabicLabel("دمج أشخاص")]
    Persons_Merge = 5003,

    /// <summary>
    /// Delete persons (Admin only)
    /// </summary>
    [ArabicLabel("حذف شخص")]
    Persons_Delete = 5004,

    // ==================== PROPERTY UNIT PERMISSIONS ====================

    /// <summary>
    /// View property units (all desktop users)
    /// </summary>
    [ArabicLabel("عرض الوحدات")]
    PropertyUnits_View = 6000,

    /// <summary>
    /// Create property units (Data Manager, Admin)
    /// </summary>
    [ArabicLabel("إنشاء وحدة")]
    PropertyUnits_Create = 6001,

    /// <summary>
    /// Update property unit details (Data Manager, Admin)
    /// </summary>
    [ArabicLabel("تعديل وحدة")]
    PropertyUnits_Update = 6002,

    /// <summary>
    /// Merge duplicate property units (Data Manager, Admin)
    /// Requires UC-007 duplicate resolution
    /// </summary>
    [ArabicLabel("دمج وحدات")]
    PropertyUnits_Merge = 6003,

    /// <summary>
    /// Delete property units (Admin only)
    /// </summary>
    [ArabicLabel("حذف وحدة")]
    PropertyUnits_Delete = 6004,

    // ==================== SURVEY PERMISSIONS ====================

    /// <summary>
    /// Create new surveys (Field Collector, Data Manager, Admin)
    /// UC-001 Field Survey, UC-004 Office Survey
    /// </summary>
    [ArabicLabel("إنشاء مسح")]
    Surveys_Create = 7000,

    /// <summary>
    /// View all surveys in the system (Data Manager, Admin)
    /// </summary>
    [ArabicLabel("عرض المسوحات")]
    Surveys_View = 7001,

    /// <summary>
    /// Export surveys to .uhc container (Field Collector)
    /// UC-003 Export surveys
    /// </summary>
    [ArabicLabel("تصدير مسوحات")]
    Surveys_Export = 7002,

    /// <summary>
    /// View own surveys only (Field Collector)
    /// </summary>
    [ArabicLabel("عرض مسوحاتي")]
    Surveys_ViewOwn = 7003,

    /// <summary>
    /// View all surveys in the system (Data Manager, Admin)
    /// Alternative naming for Surveys_View for clarity
    /// </summary>
    [ArabicLabel("عرض جميع المسوحات")]
    Surveys_ViewAll = 7004,

    /// <summary>
    /// Edit own surveys (Field Collector)
    /// UC-002 Save and resume drafts
    /// </summary>
    [ArabicLabel("تعديل مسوحاتي")]
    Surveys_EditOwn = 7005,

    /// <summary>
    /// Edit any survey (Data Manager, Admin)
    /// </summary>
    [ArabicLabel("تعديل جميع المسوحات")]
    Surveys_EditAll = 7006,

    /// <summary>
    /// Delete surveys (Admin only)
    /// </summary>
    [ArabicLabel("حذف مسح")]
    Surveys_Delete = 7007,

    /// <summary>
    /// Finalize surveys for export (Field Collector, Data Manager)
    /// UC-002 Complete and finalize
    /// </summary>
    [ArabicLabel("إنهاء مسح")]
    Surveys_Finalize = 7008,

    /// <summary>
    /// Import surveys from .uhc packages (Data Manager, Admin)
    /// UC-003 Import surveys
    /// </summary>
    [ArabicLabel("استيراد مسوحات")]
    Surveys_Import = 7009,

    // ==================== ADMIN PERMISSIONS ====================

    /// <summary>
    /// View users (Admin only)
    /// </summary>
    [ArabicLabel("عرض المستخدمين")]
    Users_View = 8000,

    /// <summary>
    /// Create users (Admin only)
    /// </summary>
    [ArabicLabel("إنشاء مستخدم")]
    Users_Create = 8001,

    /// <summary>
    /// Update user details (Admin only)
    /// </summary>
    [ArabicLabel("تعديل مستخدم")]
    Users_Update = 8002,

    /// <summary>
    /// Deactivate users (Admin only)
    /// </summary>
    [ArabicLabel("تعطيل مستخدم")]
    Users_Deactivate = 8003,

    /// <summary>
    /// Manage roles and permissions (Admin only)
    /// </summary>
    [ArabicLabel("إدارة الأدوار")]
    Roles_Manage = 8100,

    /// <summary>
    /// Manage vocabularies (Admin only)
    /// UC-010 Vocabulary Management
    /// </summary>
    [ArabicLabel("إدارة المفردات")]
    Vocabularies_Manage = 8200,

    /// <summary>
    /// Configure security settings (Admin only)
    /// UC-011 Security Settings
    /// </summary>
    [ArabicLabel("إعدادات الأمان")]
    Security_Settings = 8300,

    /// <summary>
    /// View all audit logs (Admin, Data Manager)
    /// </summary>
    [ArabicLabel("عرض جميع السجلات")]
    Audit_ViewAll = 8400,

    // ==================== SYSTEM PERMISSIONS ====================

    /// <summary>
    /// Import data packages (Data Manager, Admin)
    /// UC-003 Import pipeline
    /// </summary>
    [ArabicLabel("استيراد النظام")]
    System_Import = 9000,

    /// <summary>
    /// Export system data (Data Manager, Analyst, Admin)
    /// </summary>
    [ArabicLabel("تصدير النظام")]
    System_Export = 9001,

    /// <summary>
    /// Create system backups (Admin only)
    /// </summary>
    [ArabicLabel("نسخ احتياطي")]
    System_Backup = 9002,

    /// <summary>
    /// Restore from backups (Admin only)
    /// </summary>
    [ArabicLabel("استعادة النظام")]
    System_Restore = 9003,

    /// <summary>
    /// Sync data over LAN (Field Collector, Field Supervisor, Admin)
    /// </summary>
    [ArabicLabel("مزامنة النظام")]
    System_Sync = 9010,

}
