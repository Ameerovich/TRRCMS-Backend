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
    Claims_ViewAll = 1000,

    /// <summary>
    /// View only claims assigned to the current user (Field Collector, Field Supervisor)
    /// </summary>
    Claims_ViewAssigned = 1001,

    /// <summary>
    /// Create new claims (Office Clerk, Data Manager, Admin)
    /// Field Collectors create via surveys, not direct API
    /// </summary>
    Claims_Create = 1002,

    /// <summary>
    /// Update claim fields (UC-006) (Office Clerk, Data Manager, Admin)
    /// </summary>
    Claims_Update = 1003,

    /// <summary>
    /// Delete/archive claims (Admin only)
    /// </summary>
    Claims_Delete = 1004,

    /// <summary>
    /// Submit claims for processing (Office Clerk, Data Manager, Field Supervisor)
    /// </summary>
    Claims_Submit = 1005,

    /// <summary>
    /// Assign claims to case officers (Data Manager, Admin)
    /// </summary>
    Claims_Assign = 1006,

    /// <summary>
    /// Reassign claims between officers (Data Manager, Admin)
    /// </summary>
    Claims_Reassign = 1007,

    /// <summary>
    /// Verify claims and evidence (Data Manager, Admin)
    /// </summary>
    Claims_Verify = 1008,

    /// <summary>
    /// Approve claims - adjudication decision (Admin only)
    /// IMPORTANT: This approves social tenure recognition, not legal ownership
    /// </summary>
    Claims_Approve = 1009,

    /// <summary>
    /// Reject claims - adjudication decision (Admin only)
    /// </summary>
    Claims_Reject = 1010,

    /// <summary>
    /// Transition claims between lifecycle stages (Data Manager, Admin)
    /// </summary>
    Claims_Transition = 1011,

    /// <summary>
    /// Export claims data (Field Supervisor, Data Manager, Analyst, Admin)
    /// </summary>
    Claims_Export = 1012,

    /// <summary>
    /// View claim history/audit trail (Data Manager, Analyst, Admin)
    /// </summary>
    Claims_ViewHistory = 1013,

    // ==================== EVIDENCE PERMISSIONS ====================

    /// <summary>
    /// View evidence (all roles except Field Collector on desktop)
    /// </summary>
    Evidence_View = 2000,

    /// <summary>
    /// Upload evidence documents (Field Collector, Office Clerk, Data Manager, Admin)
    /// </summary>
    Evidence_Upload = 2001,

    /// <summary>
    /// Verify evidence (Data Manager, Admin)
    /// </summary>
    Evidence_Verify = 2002,

    /// <summary>
    /// Delete evidence (Admin only)
    /// </summary>
    Evidence_Delete = 2003,

    // ==================== DOCUMENT PERMISSIONS ====================

    /// <summary>
    /// View sensitive personal documents (Office Clerk, Data Manager, Admin)
    /// </summary>
    Documents_ViewSensitive = 3000,

    /// <summary>
    /// Download documents (Office Clerk, Data Manager, Admin)
    /// </summary>
    Documents_Download = 3001,

    /// <summary>
    /// Upload documents (Office Clerk, Data Manager, Admin)
    /// </summary>
    Documents_Upload = 3002,

    /// <summary>
    /// Delete documents (Admin only)
    /// </summary>
    Documents_Delete = 3003,

    // ==================== BUILDING PERMISSIONS ====================

    /// <summary>
    /// View buildings (all roles)
    /// </summary>
    Buildings_View = 4000,

    /// <summary>
    /// Create buildings (Data Manager, Admin)
    /// </summary>
    Buildings_Create = 4001,

    /// <summary>
    /// Update building details (Data Manager, Admin)
    /// </summary>
    Buildings_Update = 4002,

    /// <summary>
    /// Assign buildings to field teams (Data Manager, Admin)
    /// </summary>
    Buildings_Assign = 4003,

    /// <summary>
    /// Delete buildings (Admin only)
    /// </summary>
    Buildings_Delete = 4004,

    // ==================== PERSON PERMISSIONS ====================

    /// <summary>
    /// View persons (all desktop users)
    /// </summary>
    Persons_View = 5000,

    /// <summary>
    /// Create persons (Office Clerk, Data Manager, Admin)
    /// </summary>
    Persons_Create = 5001,

    /// <summary>
    /// Update person details (Office Clerk, Data Manager, Admin)
    /// </summary>
    Persons_Update = 5002,

    /// <summary>
    /// Merge duplicate persons (Data Manager, Admin)
    /// Requires UC-008 duplicate resolution
    /// </summary>
    Persons_Merge = 5003,

    /// <summary>
    /// Delete persons (Admin only)
    /// </summary>
    Persons_Delete = 5004,

    // ==================== PROPERTY UNIT PERMISSIONS ====================

    /// <summary>
    /// View property units (all desktop users)
    /// </summary>
    PropertyUnits_View = 6000,

    /// <summary>
    /// Create property units (Data Manager, Admin)
    /// </summary>
    PropertyUnits_Create = 6001,

    /// <summary>
    /// Update property unit details (Data Manager, Admin)
    /// </summary>
    PropertyUnits_Update = 6002,

    /// <summary>
    /// Merge duplicate property units (Data Manager, Admin)
    /// Requires UC-007 duplicate resolution
    /// </summary>
    PropertyUnits_Merge = 6003,

    /// <summary>
    /// Delete property units (Admin only)
    /// </summary>
    PropertyUnits_Delete = 6004,

    // ==================== SURVEY PERMISSIONS ====================

    /// <summary>
    /// Create surveys (Field Collector via mobile)
    /// </summary>
    Surveys_Create = 7000,

    /// <summary>
    /// View surveys (Field Supervisor, Data Manager, Admin)
    /// </summary>
    Surveys_View = 7001,

    /// <summary>
    /// Export surveys (Field Collector, Field Supervisor)
    /// </summary>
    Surveys_Export = 7002,

    // ==================== ADMIN PERMISSIONS ====================

    /// <summary>
    /// View users (Admin only)
    /// </summary>
    Users_View = 8000,

    /// <summary>
    /// Create users (Admin only)
    /// </summary>
    Users_Create = 8001,

    /// <summary>
    /// Update user details (Admin only)
    /// </summary>
    Users_Update = 8002,

    /// <summary>
    /// Deactivate users (Admin only)
    /// </summary>
    Users_Deactivate = 8003,

    /// <summary>
    /// Manage roles and permissions (Admin only)
    /// </summary>
    Roles_Manage = 8100,

    /// <summary>
    /// Manage vocabularies (Admin only)
    /// UC-010 Vocabulary Management
    /// </summary>
    Vocabularies_Manage = 8200,

    /// <summary>
    /// Configure security settings (Admin only)
    /// UC-011 Security Settings
    /// </summary>
    Security_Settings = 8300,

    /// <summary>
    /// View all audit logs (Admin, Data Manager)
    /// </summary>
    Audit_ViewAll = 8400,

    // ==================== SYSTEM PERMISSIONS ====================

    /// <summary>
    /// Import data packages (Data Manager, Admin)
    /// UC-003 Import pipeline
    /// </summary>
    System_Import = 9000,

    /// <summary>
    /// Export system data (Data Manager, Analyst, Admin)
    /// </summary>
    System_Export = 9001,

    /// <summary>
    /// Create system backups (Admin only)
    /// </summary>
    System_Backup = 9002,

    /// <summary>
    /// Restore from backups (Admin only)
    /// </summary>
    System_Restore = 9003,
}