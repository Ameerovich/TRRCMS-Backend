using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Common.Services;

/// <summary>
/// Provides default permission assignments for user roles.
/// </summary>
public static class PermissionSeeder
{
    /// <summary>
    /// Get default permissions for a given user role.
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

            Permission.Evidence_View,
            Permission.Evidence_Upload,
            Permission.Evidence_Verify,
            Permission.Evidence_Delete,

            Permission.Buildings_View,
            Permission.Buildings_Create,
            Permission.Buildings_Update,
            Permission.Buildings_Assign,
            Permission.Buildings_Delete,

            Permission.Persons_View,
            Permission.Persons_Create,
            Permission.Persons_Update,
            Permission.Persons_Merge,
            Permission.Persons_Delete,

            Permission.PropertyUnits_View,
            Permission.PropertyUnits_Create,
            Permission.PropertyUnits_Update,
            Permission.PropertyUnits_Merge,
            Permission.PropertyUnits_Delete,

            Permission.Surveys_Create,      // Create surveys
            Permission.Surveys_View,        // View all surveys
            Permission.Surveys_ViewAll,     // View all surveys (alias)
            Permission.Surveys_ViewOwn,     // View own surveys
            Permission.Surveys_EditOwn,     // Edit own surveys
            Permission.Surveys_EditAll,     // Edit any survey
            Permission.Surveys_Finalize,    // Finalize surveys
            Permission.Surveys_Export,      // Export to .uhc
            Permission.Surveys_Import,      // Import .uhc packages
            Permission.Surveys_Delete,      // Delete surveys

            Permission.Users_View,
            Permission.Users_Create,
            Permission.Users_Update,
            Permission.Users_Deactivate,
            Permission.Roles_Manage,
            Permission.Vocabularies_Manage,
            Permission.Security_Settings,
            Permission.Audit_ViewAll,

            Permission.Dashboard_View,

            Permission.System_Import,
            Permission.System_Export,
            Permission.System_Backup,
            Permission.System_Restore,
            Permission.System_Sync,   // Tablet LAN synchronisation (Sync Protocol Steps 1–4)
            Permission.Landmarks_Manage
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

            Permission.Evidence_View,
            Permission.Evidence_Upload,
            Permission.Evidence_Verify,

            Permission.Buildings_View,
            Permission.Buildings_Create,
            Permission.Buildings_Update,
            Permission.Buildings_Assign,

            Permission.Persons_View,
            Permission.Persons_Create,
            Permission.Persons_Update,
            Permission.Persons_Merge,

            Permission.PropertyUnits_View,
            Permission.PropertyUnits_Create,
            Permission.PropertyUnits_Update,
            Permission.PropertyUnits_Merge,

            Permission.Surveys_Create,      // Create surveys
            Permission.Surveys_View,        // View all surveys
            Permission.Surveys_ViewAll,     // View all surveys (alias)
            Permission.Surveys_ViewOwn,     // View own surveys
            Permission.Surveys_EditOwn,     // Edit own surveys
            Permission.Surveys_EditAll,     // Edit any survey
            Permission.Surveys_Finalize,    // Finalize surveys
            Permission.Surveys_Export,      // Export to .uhc
            Permission.Surveys_Import,      // Import .uhc packages
            // No Surveys_Delete - Admin only

            Permission.Dashboard_View,

            Permission.System_Import,
            Permission.System_Export,
            Permission.Audit_ViewAll,
            Permission.Landmarks_Manage
        };
    }

    /// <summary>
    /// Field Collector - Tablet-only user.
    /// All data operations happen offline on the tablet.
    /// Only interacts with the server via sync protocol and auth.
    /// Auth endpoints (login, refresh, logout) are [AllowAnonymous] or [Authorize] — no permissions needed.
    /// </summary>
    private static List<Permission> GetFieldCollectorPermissions()
    {
        return new List<Permission>
        {
            Permission.System_Sync          // Tablet LAN synchronisation (Sync Protocol Steps 1–4)
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
            Permission.Claims_ViewAll,      // View all claims for supervision
            Permission.Claims_Submit,       // Submit claims for processing
            Permission.Claims_Export,
            Permission.Claims_ViewHistory,

            Permission.Evidence_View,

            Permission.Buildings_View,
            Permission.Buildings_Assign,    // Assign buildings to field collectors

            Permission.Persons_View,

            Permission.PropertyUnits_View,

            Permission.Surveys_View,
            Permission.Surveys_ViewAll,
            Permission.Surveys_ViewOwn,
            Permission.Surveys_Export,

            Permission.Dashboard_View,

            Permission.System_Sync
        };
    }

    /// <summary>
    /// Office Clerk - Office-based data entry and survey operations.
    /// Works at registration office on desktop.
    /// </summary>
    private static List<Permission> GetOfficeClerkPermissions()
    {
        return new List<Permission>
        {
            Permission.Claims_ViewAssigned, // View claims they created
            Permission.Claims_Create,       // Create claims directly or via survey finalization
            Permission.Claims_Update,       // Update claim information
            Permission.Claims_Submit,       // Submit claims for processing

            Permission.Evidence_View,
            Permission.Evidence_Upload,     // Upload supporting documents

            Permission.Buildings_View,

            Permission.Persons_View,
            Permission.Persons_Create,      // Create persons during office survey
            Permission.Persons_Update,      // Update person details

            Permission.PropertyUnits_View,
            Permission.PropertyUnits_Create, // Create property units during survey

            Permission.Surveys_Create,      // Create office surveys
            Permission.Surveys_ViewOwn,     // View own surveys
            Permission.Surveys_EditOwn,     // Edit own draft surveys
            Permission.Surveys_Finalize     // Finalize with claim creation
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
            Permission.Claims_ViewAll,
            Permission.Claims_Export,
            Permission.Claims_ViewHistory,

            Permission.Evidence_View,

            Permission.Buildings_View,

            Permission.Persons_View,

            Permission.PropertyUnits_View,

            Permission.Surveys_View,        // View all surveys for analysis
            Permission.Surveys_ViewAll,     // View all surveys (alias)
            // No create, edit, finalize, export, import, delete

            Permission.Dashboard_View,

            Permission.System_Export,
            Permission.Audit_ViewAll
        };
    }
}