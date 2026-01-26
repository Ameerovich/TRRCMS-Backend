using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Common.Services;

/// <summary>
/// Provides default permission assignments for user roles
/// Moved from Infrastructure to Application (business logic, not infrastructure concern)
/// Updated: January 25, 2026 - Added Survey permissions for UC-001, UC-002, UC-004, UC-005
/// </summary>
public static class PermissionSeeder
{
    /// <summary>
    /// Get default permissions for a given user role
    /// Based on FSD Section 3: Stakeholders & Roles
    /// </summary>
    public static IEnumerable<Permission> GetDefaultPermissionsForRole(UserRole role)
    {
        return role switch
        {
            UserRole.Administrator => GetAdministratorPermissions(),
            UserRole.DataManager => GetDataManagerPermissions(),
            UserRole.FieldCollector => GetFieldCollectorPermissions(),
            UserRole.FieldSupervisor => GetFieldSupervisorPermissions(),
            UserRole.OfficeClerk => GetOfficeClerkPermissions(),
            UserRole.Analyst => GetAnalystPermissions(),
            _ => new List<Permission>()
        };
    }

    /// <summary>
    /// Administrator - Full system access
    /// All permissions including delete, backup, restore
    /// </summary>
    private static List<Permission> GetAdministratorPermissions()
    {
        return new List<Permission>
        {
            // ==================== CLAIMS ====================
            Permission.Claims_ViewAll,
            Permission.Claims_ViewAssigned,
            Permission.Claims_Create,
            Permission.Claims_Update,
            Permission.Claims_Delete,
            Permission.Claims_Submit,
            Permission.Claims_Assign,
            Permission.Claims_Reassign,
            Permission.Claims_Verify,
            Permission.Claims_Approve,
            Permission.Claims_Reject,
            Permission.Claims_Transition,
            Permission.Claims_Export,
            Permission.Claims_ViewHistory,

            // ==================== EVIDENCE ====================
            Permission.Evidence_View,
            Permission.Evidence_Upload,
            Permission.Evidence_Verify,
            Permission.Evidence_Delete,

            // ==================== DOCUMENTS ====================
            Permission.Documents_ViewSensitive,
            Permission.Documents_Download,
            Permission.Documents_Upload,
            Permission.Documents_Delete,

            // ==================== BUILDINGS ====================
            Permission.Buildings_View,
            Permission.Buildings_Create,
            Permission.Buildings_Update,
            Permission.Buildings_Assign,
            Permission.Buildings_Delete,

            // ==================== PERSONS ====================
            Permission.Persons_View,
            Permission.Persons_Create,
            Permission.Persons_Update,
            Permission.Persons_Merge,
            Permission.Persons_Delete,

            // ==================== PROPERTY UNITS ====================
            Permission.PropertyUnits_View,
            Permission.PropertyUnits_Create,
            Permission.PropertyUnits_Update,
            Permission.PropertyUnits_Merge,
            Permission.PropertyUnits_Delete,

            // ==================== SURVEYS (FULL ACCESS) ====================
            Permission.Surveys_Create,      // UC-001, UC-004: Create surveys
            Permission.Surveys_View,        // View all surveys
            Permission.Surveys_ViewAll,     // View all surveys (alias)
            Permission.Surveys_ViewOwn,     // View own surveys
            Permission.Surveys_EditOwn,     // UC-002: Edit own surveys
            Permission.Surveys_EditAll,     // Edit any survey
            Permission.Surveys_Finalize,    // UC-002, UC-005: Finalize surveys
            Permission.Surveys_Export,      // Export to .uhc
            Permission.Surveys_Import,      // UC-003: Import .uhc packages
            Permission.Surveys_Delete,      // Delete surveys

            // ==================== ADMIN ====================
            Permission.Users_View,
            Permission.Users_Create,
            Permission.Users_Update,
            Permission.Users_Deactivate,
            Permission.Roles_Manage,
            Permission.Vocabularies_Manage,
            Permission.Security_Settings,
            Permission.Audit_ViewAll,

            // ==================== SYSTEM ====================
            Permission.System_Import,
            Permission.System_Export,
            Permission.System_Backup,
            Permission.System_Restore
        };
    }

    /// <summary>
    /// Data Manager - Full operational access
    /// Cannot delete, backup, restore, or manage users
    /// </summary>
    private static List<Permission> GetDataManagerPermissions()
    {
        return new List<Permission>
        {
            // ==================== CLAIMS ====================
            Permission.Claims_ViewAll,
            Permission.Claims_ViewAssigned,
            Permission.Claims_Create,
            Permission.Claims_Update,
            Permission.Claims_Submit,
            Permission.Claims_Assign,
            Permission.Claims_Reassign,
            Permission.Claims_Verify,
            Permission.Claims_Transition,
            Permission.Claims_Export,
            Permission.Claims_ViewHistory,

            // ==================== EVIDENCE ====================
            Permission.Evidence_View,
            Permission.Evidence_Upload,
            Permission.Evidence_Verify,

            // ==================== DOCUMENTS ====================
            Permission.Documents_ViewSensitive,
            Permission.Documents_Download,
            Permission.Documents_Upload,

            // ==================== BUILDINGS ====================
            Permission.Buildings_View,
            Permission.Buildings_Create,
            Permission.Buildings_Update,
            Permission.Buildings_Assign,

            // ==================== PERSONS ====================
            Permission.Persons_View,
            Permission.Persons_Create,
            Permission.Persons_Update,
            Permission.Persons_Merge,

            // ==================== PROPERTY UNITS ====================
            Permission.PropertyUnits_View,
            Permission.PropertyUnits_Create,
            Permission.PropertyUnits_Update,
            Permission.PropertyUnits_Merge,

            // ==================== SURVEYS (MOST ACCESS) ====================
            Permission.Surveys_Create,      // UC-001, UC-004: Create surveys
            Permission.Surveys_View,        // View all surveys
            Permission.Surveys_ViewAll,     // View all surveys (alias)
            Permission.Surveys_ViewOwn,     // View own surveys
            Permission.Surveys_EditOwn,     // UC-002: Edit own surveys
            Permission.Surveys_EditAll,     // Edit any survey
            Permission.Surveys_Finalize,    // UC-002, UC-005: Finalize surveys
            Permission.Surveys_Export,      // Export to .uhc
            Permission.Surveys_Import,      // UC-003: Import .uhc packages
            // No Surveys_Delete - Admin only

            // ==================== SYSTEM ====================
            Permission.System_Import,
            Permission.System_Export,
            Permission.Audit_ViewAll
        };
    }

    /// <summary>
    /// Field Collector - Mobile field survey operations
    /// Primary responsibility: UC-001 Create Field Survey, UC-002 Resume Draft Survey
    /// Works primarily on tablet in the field
    /// </summary>
    private static List<Permission> GetFieldCollectorPermissions()
    {
        return new List<Permission>
        {
            // ==================== CLAIMS ====================
            // View only claims they created through surveys
            Permission.Claims_ViewAssigned,

            // ==================== EVIDENCE ====================
            Permission.Evidence_View,
            Permission.Evidence_Upload,     // Upload photos, documents in field

            // ==================== BUILDINGS ====================
            Permission.Buildings_View,      // View buildings for survey selection

            // ==================== PERSONS ====================
            Permission.Persons_View,        // View persons in surveys
            Permission.Persons_Create,      // Create persons during survey

            // ==================== PROPERTY UNITS ====================
            Permission.PropertyUnits_View,  // View property units

            // ==================== SURVEYS (PRIMARY ROLE) ====================
            Permission.Surveys_Create,      // UC-001: Create new field surveys
            Permission.Surveys_ViewOwn,     // UC-002: View own surveys to resume
            Permission.Surveys_EditOwn,     // UC-002: Edit/update own draft surveys
            Permission.Surveys_Finalize,    // UC-002: Finalize completed surveys
            Permission.Surveys_Export       // Export to .uhc for sync
            // No Surveys_View/ViewAll - cannot see other collectors' surveys
            // No Surveys_EditAll - cannot edit others' surveys
            // No Surveys_Import - done by office staff
            // No Surveys_Delete - Admin only
        };
    }

    /// <summary>
    /// Field Supervisor - Supervise field collection teams
    /// View and review surveys, submit claims, but not create surveys
    /// </summary>
    private static List<Permission> GetFieldSupervisorPermissions()
    {
        return new List<Permission>
        {
            // ==================== CLAIMS ====================
            Permission.Claims_ViewAll,      // View all claims for supervision
            Permission.Claims_Submit,       // Submit claims for processing
            Permission.Claims_Export,
            Permission.Claims_ViewHistory,

            // ==================== EVIDENCE ====================
            Permission.Evidence_View,

            // ==================== DOCUMENTS ====================
            Permission.Documents_ViewSensitive,
            Permission.Documents_Download,

            // ==================== BUILDINGS ====================
            Permission.Buildings_View,

            // ==================== PERSONS ====================
            Permission.Persons_View,

            // ==================== PROPERTY UNITS ====================
            Permission.PropertyUnits_View,

            // ==================== SURVEYS (SUPERVISORY) ====================
            Permission.Surveys_View,        // View all surveys for supervision
            Permission.Surveys_ViewAll,     // View all surveys (alias)
            Permission.Surveys_ViewOwn,     // View own if created any
            Permission.Surveys_Export       // Export for reporting
            // No Surveys_Create - supervisors don't create surveys
            // No Surveys_EditOwn/EditAll - supervisors review, not edit
            // No Surveys_Finalize - collectors finalize their own
            // No Surveys_Import - Data Manager responsibility
        };
    }

    /// <summary>
    /// Office Clerk - Office-based data entry and survey operations
    /// Primary responsibility: UC-004 Create Office Survey, UC-005 Finalize with Claim
    /// Works at registration office on desktop
    /// </summary>
    private static List<Permission> GetOfficeClerkPermissions()
    {
        return new List<Permission>
        {
            // ==================== CLAIMS ====================
            Permission.Claims_ViewAssigned, // View claims they created
            Permission.Claims_Create,       // Create claims directly or via survey finalization
            Permission.Claims_Update,       // UC-006: Update claim information
            Permission.Claims_Submit,       // Submit claims for processing

            // ==================== EVIDENCE ====================
            Permission.Evidence_View,
            Permission.Evidence_Upload,     // Upload supporting documents

            // ==================== DOCUMENTS ====================
            Permission.Documents_ViewSensitive,
            Permission.Documents_Download,
            Permission.Documents_Upload,

            // ==================== BUILDINGS ====================
            Permission.Buildings_View,

            // ==================== PERSONS ====================
            Permission.Persons_View,
            Permission.Persons_Create,      // Create persons during office survey
            Permission.Persons_Update,      // Update person details

            // ==================== PROPERTY UNITS ====================
            Permission.PropertyUnits_View,
            Permission.PropertyUnits_Create, // Create property units during survey

            // ==================== SURVEYS (OFFICE OPERATIONS) ====================
            Permission.Surveys_Create,      // UC-004: Create office surveys
            Permission.Surveys_ViewOwn,     // View own surveys
            Permission.Surveys_EditOwn,     // Edit own draft surveys
            Permission.Surveys_Finalize     // UC-005: Finalize with claim creation
            // No Surveys_View/ViewAll - cannot see all surveys
            // No Surveys_EditAll - cannot edit others' surveys
            // No Surveys_Export - field collector responsibility
            // No Surveys_Import - Data Manager responsibility
            // No Surveys_Delete - Admin only
        };
    }

    /// <summary>
    /// Analyst - Read-only access for reporting and analysis
    /// Can view and export data, but cannot modify anything
    /// </summary>
    private static List<Permission> GetAnalystPermissions()
    {
        return new List<Permission>
        {
            // ==================== CLAIMS (READ ONLY) ====================
            Permission.Claims_ViewAll,
            Permission.Claims_Export,
            Permission.Claims_ViewHistory,

            // ==================== EVIDENCE (READ ONLY) ====================
            Permission.Evidence_View,

            // ==================== DOCUMENTS (READ ONLY) ====================
            Permission.Documents_ViewSensitive,
            Permission.Documents_Download,

            // ==================== BUILDINGS (READ ONLY) ====================
            Permission.Buildings_View,

            // ==================== PERSONS (READ ONLY) ====================
            Permission.Persons_View,

            // ==================== PROPERTY UNITS (READ ONLY) ====================
            Permission.PropertyUnits_View,

            // ==================== SURVEYS (READ ONLY) ====================
            Permission.Surveys_View,        // View all surveys for analysis
            Permission.Surveys_ViewAll,     // View all surveys (alias)
            // No create, edit, finalize, export, import, delete

            // ==================== SYSTEM (EXPORT ONLY) ====================
            Permission.System_Export,
            Permission.Audit_ViewAll
        };
    }
}