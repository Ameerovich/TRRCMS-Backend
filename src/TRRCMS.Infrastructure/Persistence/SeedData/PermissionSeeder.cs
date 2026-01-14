using TRRCMS.Domain.Enums;

namespace TRRCMS.Infrastructure.Persistence.SeedData;

/// <summary>
/// Default permission mappings for each user role
/// Based on FSD Section 3: Stakeholders & Roles
/// </summary>
public static class PermissionSeeder
{
    /// <summary>
    /// Get default permissions for a specific role
    /// </summary>
    public static List<Permission> GetDefaultPermissionsForRole(UserRole role)
    {
        return role switch
        {
            UserRole.FieldCollector => GetFieldCollectorPermissions(),
            UserRole.FieldSupervisor => GetFieldSupervisorPermissions(),
            UserRole.OfficeClerk => GetOfficeClerkPermissions(),
            UserRole.DataManager => GetDataManagerPermissions(),
            UserRole.Analyst => GetAnalystPermissions(),
            UserRole.Administrator => GetAdministratorPermissions(),
            _ => new List<Permission>()
        };
    }

    /// <summary>
    /// Field Collector: Mobile app access only
    /// Can create surveys and export containers
    /// </summary>
    private static List<Permission> GetFieldCollectorPermissions()
    {
        return new List<Permission>
        {
            // Surveys
            Permission.Surveys_Create,
            Permission.Surveys_View,
            Permission.Surveys_Export,
            
            // Evidence (mobile only)
            Permission.Evidence_Upload,
            
            // View assigned items
            Permission.Claims_ViewAssigned,
        };
    }

    /// <summary>
    /// Field Supervisor: Read-only desktop access
    /// Monitor field operations and generate reports
    /// </summary>
    private static List<Permission> GetFieldSupervisorPermissions()
    {
        return new List<Permission>
        {
            // Claims (read-only)
            Permission.Claims_ViewAll,
            Permission.Claims_ViewHistory,
            Permission.Claims_Submit, // Can submit collected surveys
            Permission.Claims_Export,
            
            // Evidence (view only)
            Permission.Evidence_View,
            
            // Documents (view only) - ADDED
            Permission.Documents_ViewSensitive,
            
            // Surveys
            Permission.Surveys_View,
            Permission.Surveys_Export,
            
            // Buildings
            Permission.Buildings_View,
            
            // Persons
            Permission.Persons_View,
            
            // Property Units
            Permission.PropertyUnits_View,
        };
    }

    /// <summary>
    /// Office Clerk: Full desktop access for claim registration
    /// Register new claims, update, scan documents, print receipts
    /// </summary>
    private static List<Permission> GetOfficeClerkPermissions()
    {
        return new List<Permission>
        {
            // Claims
            Permission.Claims_ViewAll,
            Permission.Claims_Create,
            Permission.Claims_Update,
            Permission.Claims_Submit,
            Permission.Claims_ViewHistory,
            
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
            Permission.PropertyUnits_View,
            Permission.PropertyUnits_Create,
            Permission.PropertyUnits_Update,
        };
    }

    /// <summary>
    /// Data Manager: Full desktop access for data validation and import
    /// Import containers, review conflicts, finalize cases, generate reports
    /// </summary>
    private static List<Permission> GetDataManagerPermissions()
    {
        return new List<Permission>
        {
            // Claims (full management except approve/reject)
            Permission.Claims_ViewAll,
            Permission.Claims_Create,
            Permission.Claims_Update,
            Permission.Claims_Delete,
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
            Permission.Persons_Merge, // Duplicate resolution
            Permission.Persons_Delete,
            
            // Property Units
            Permission.PropertyUnits_View,
            Permission.PropertyUnits_Create,
            Permission.PropertyUnits_Update,
            Permission.PropertyUnits_Merge, // Duplicate resolution
            Permission.PropertyUnits_Delete,
            
            // Surveys
            Permission.Surveys_View,
            
            // System
            Permission.System_Import,
            Permission.System_Export,
            
            // Audit
            Permission.Audit_ViewAll,
        };
    }

    /// <summary>
    /// Analyst: Read-only access for reporting and analytics
    /// Run filters, export data, print signed PDFs
    /// </summary>
    private static List<Permission> GetAnalystPermissions()
    {
        return new List<Permission>
        {
            // Claims (read-only)
            Permission.Claims_ViewAll,
            Permission.Claims_ViewHistory,
            Permission.Claims_Export,
            
            // Evidence (view only)
            Permission.Evidence_View,
            
            // Documents (view only) - ADDED
            Permission.Documents_ViewSensitive,
            
            // Buildings
            Permission.Buildings_View,
            
            // Persons
            Permission.Persons_View,
            
            // Property Units
            Permission.PropertyUnits_View,
            
            // Surveys
            Permission.Surveys_View,
            
            // System
            Permission.System_Export,
            
            // Audit
            Permission.Audit_ViewAll,
        };
    }

    /// <summary>
    /// Administrator: Full access to all features
    /// Manage users/roles, security policies, configure forms, control vocabularies
    /// </summary>
    private static List<Permission> GetAdministratorPermissions()
    {
        // Administrator gets ALL permissions
        return Enum.GetValues<Permission>().ToList();
    }

    /// <summary>
    /// Get a human-readable description of each role's permissions
    /// Useful for documentation and UI display
    /// </summary>
    public static Dictionary<UserRole, string> GetRoleDescriptions()
    {
        return new Dictionary<UserRole, string>
        {
            [UserRole.FieldCollector] = "Mobile app access only. Can create surveys, capture evidence, export containers.",
            [UserRole.FieldSupervisor] = "Read-only desktop access. Monitor field operations, export reports.",
            [UserRole.OfficeClerk] = "Full desktop access for claim registration. Register claims, scan documents, update records.",
            [UserRole.DataManager] = "Full data management access. Import data, resolve conflicts, verify claims, manage duplicates.",
            [UserRole.Analyst] = "Read-only access for analytics. Export data, generate reports, view statistics.",
            [UserRole.Administrator] = "Full system access. Manage users, configure system, approve/reject claims."
        };
    }

    /// <summary>
    /// Validate that a role has a specific permission by default
    /// Useful for testing
    /// </summary>
    public static bool RoleHasPermission(UserRole role, Permission permission)
    {
        var permissions = GetDefaultPermissionsForRole(role);
        return permissions.Contains(permission);
    }
}