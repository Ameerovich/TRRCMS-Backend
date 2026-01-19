using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Common.Services;

/// <summary>
/// Provides default permission assignments for user roles
/// Moved from Infrastructure to Application (business logic, not infrastructure concern)
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

    private static List<Permission> GetAdministratorPermissions()
    {
        return new List<Permission>
        {
            // Claims - Full Access
            Permission.Claims_ViewAll,
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

            // Evidence
            Permission.Evidence_View,
            Permission.Evidence_Upload,
            Permission.Evidence_Verify,
            Permission.Evidence_Delete,

            // Documents
            Permission.Documents_ViewSensitive,
            Permission.Documents_Download,
            Permission.Documents_Upload,
            Permission.Documents_Delete,

            // Buildings
            Permission.Buildings_View,
            Permission.Buildings_Create,
            Permission.Buildings_Update,
            Permission.Buildings_Assign,
            Permission.Buildings_Delete,

            // Persons
            Permission.Persons_View,
            Permission.Persons_Create,
            Permission.Persons_Update,
            Permission.Persons_Merge,
            Permission.Persons_Delete,

            // Property Units
            Permission.PropertyUnits_View,
            Permission.PropertyUnits_Create,
            Permission.PropertyUnits_Update,
            Permission.PropertyUnits_Merge,
            Permission.PropertyUnits_Delete,

            // Surveys
            Permission.Surveys_View,
            Permission.Surveys_Export,

            // Admin
            Permission.Users_View,
            Permission.Users_Create,
            Permission.Users_Update,
            Permission.Users_Deactivate,
            Permission.Roles_Manage,
            Permission.Vocabularies_Manage,
            Permission.Security_Settings,
            Permission.Audit_ViewAll,

            // System
            Permission.System_Import,
            Permission.System_Export,
            Permission.System_Backup,
            Permission.System_Restore
        };
    }

    private static List<Permission> GetDataManagerPermissions()
    {
        return new List<Permission>
        {
            // Claims - Most Permissions
            Permission.Claims_ViewAll,
            Permission.Claims_Create,
            Permission.Claims_Update,
            Permission.Claims_Submit,
            Permission.Claims_Assign,
            Permission.Claims_Reassign,
            Permission.Claims_Verify,
            Permission.Claims_Transition,
            Permission.Claims_Export,
            Permission.Claims_ViewHistory,

            // Evidence
            Permission.Evidence_View,
            Permission.Evidence_Upload,
            Permission.Evidence_Verify,

            // Documents
            Permission.Documents_ViewSensitive,
            Permission.Documents_Download,
            Permission.Documents_Upload,

            // Buildings
            Permission.Buildings_View,
            Permission.Buildings_Create,
            Permission.Buildings_Update,
            Permission.Buildings_Assign,

            // Persons
            Permission.Persons_View,
            Permission.Persons_Create,
            Permission.Persons_Update,
            Permission.Persons_Merge,

            // Property Units
            Permission.PropertyUnits_View,
            Permission.PropertyUnits_Create,
            Permission.PropertyUnits_Update,
            Permission.PropertyUnits_Merge,

            // Surveys
            Permission.Surveys_View,
            Permission.Surveys_Export,

            // System
            Permission.System_Import,
            Permission.System_Export,
            Permission.Audit_ViewAll
        };
    }

    private static List<Permission> GetFieldCollectorPermissions()
    {
        return new List<Permission>
        {
            // Claims - View Own Only
            Permission.Claims_ViewAssigned,

            // Evidence
            Permission.Evidence_View,
            Permission.Evidence_Upload,

            // Surveys - Primary Responsibility
            Permission.Surveys_Create,
            Permission.Surveys_Export,

            // Buildings - View Only
            Permission.Buildings_View
        };
    }

    private static List<Permission> GetFieldSupervisorPermissions()
    {
        return new List<Permission>
        {
            // Claims
            Permission.Claims_ViewAll,
            Permission.Claims_Submit,
            Permission.Claims_Export,
            Permission.Claims_ViewHistory,

            // Evidence
            Permission.Evidence_View,

            // Buildings
            Permission.Buildings_View,

            // Surveys
            Permission.Surveys_View,
            Permission.Surveys_Export,

            // Persons
            Permission.Persons_View,

            // Property Units
            Permission.PropertyUnits_View
        };
    }

    private static List<Permission> GetOfficeClerkPermissions()
    {
        return new List<Permission>
        {
            // Claims - Create and Update Own
            Permission.Claims_ViewAssigned,
            Permission.Claims_Create,
            Permission.Claims_Update,
            Permission.Claims_Submit,

            // Evidence
            Permission.Evidence_View,
            Permission.Evidence_Upload,

            // Documents
            Permission.Documents_ViewSensitive,
            Permission.Documents_Download,
            Permission.Documents_Upload,

            // Buildings
            Permission.Buildings_View,

            // Persons
            Permission.Persons_View,
            Permission.Persons_Create,
            Permission.Persons_Update,

            // Property Units
            Permission.PropertyUnits_View
        };
    }

    private static List<Permission> GetAnalystPermissions()
    {
        return new List<Permission>
        {
            // Claims - Read Only
            Permission.Claims_ViewAll,
            Permission.Claims_Export,
            Permission.Claims_ViewHistory,

            // Evidence - View Only
            Permission.Evidence_View,

            // Buildings - View Only
            Permission.Buildings_View,

            // Persons - View Only
            Permission.Persons_View,

            // Property Units - View Only
            Permission.PropertyUnits_View,

            // Surveys - View Only
            Permission.Surveys_View,

            // System
            Permission.System_Export,
            Permission.Audit_ViewAll
        };
    }
}