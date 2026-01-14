using MediatR;
using TRRCMS.Application.Common.Interfaces;
using TRRCMS.Domain.Entities;
using TRRCMS.Domain.Enums;

namespace TRRCMS.Application.Auth.Commands.Seed;

/// <summary>
/// Handler for seeding initial test users
/// </summary>
public class SeedCommandHandler : IRequestHandler<SeedCommand, SeedResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public SeedCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<SeedResult> Handle(SeedCommand request, CancellationToken cancellationToken)
    {
        var result = new SeedResult { Success = true };

        // Define test users
        var testUsers = new[]
        {
            new
            {
                Username = "admin",
                Password = "Admin@123",
                FullNameArabic = "المسؤول الرئيسي",
                FullNameEnglish = "System Administrator",
                Email = "admin@trrcms.local",
                PhoneNumber = "+963-11-1234567",
                Role = UserRole.Administrator,
                Organization = "UN-Habitat",
                JobTitle = "System Administrator",
                HasMobileAccess = false,
                HasDesktopAccess = true
            },
            new
            {
                Username = "datamanager",
                Password = "Data@123",
                FullNameArabic = "مدير البيانات",
                FullNameEnglish = "Data Manager",
                Email = "datamanager@trrcms.local",
                PhoneNumber = "+963-11-2234567",
                Role = UserRole.DataManager,
                Organization = "UN-Habitat",
                JobTitle = "Data Manager",
                HasMobileAccess = false,
                HasDesktopAccess = true
            },
            new
            {
                Username = "clerk",
                Password = "Clerk@123",
                FullNameArabic = "موظف المكتب",
                FullNameEnglish = "Office Clerk",
                Email = "clerk@trrcms.local",
                PhoneNumber = "+963-11-3234567",
                Role = UserRole.OfficeClerk,
                Organization = "Aleppo Municipality",
                JobTitle = "Office Clerk",
                HasMobileAccess = false,
                HasDesktopAccess = true
            },
            new
            {
                Username = "collector",
                Password = "Field@123",
                FullNameArabic = "جامع البيانات الميداني",
                FullNameEnglish = "Field Data Collector",
                Email = "collector@trrcms.local",
                PhoneNumber = "+963-11-4234567",
                Role = UserRole.FieldCollector,
                Organization = "UN-Habitat",
                JobTitle = "Field Collector",
                HasMobileAccess = true,
                HasDesktopAccess = false
            },
            new
            {
                Username = "supervisor",
                Password = "Super@123",
                FullNameArabic = "المشرف الميداني",
                FullNameEnglish = "Field Supervisor",
                Email = "supervisor@trrcms.local",
                PhoneNumber = "+963-11-5234567",
                Role = UserRole.FieldSupervisor,
                Organization = "UN-Habitat",
                JobTitle = "Field Supervisor",
                HasMobileAccess = false,
                HasDesktopAccess = true
            },
            new
            {
                Username = "analyst",
                Password = "Analyst@123",
                FullNameArabic = "المحلل",
                FullNameEnglish = "Data Analyst",
                Email = "analyst@trrcms.local",
                PhoneNumber = "+963-11-6234567",
                Role = UserRole.Analyst,
                Organization = "UN-Habitat",
                JobTitle = "Data Analyst",
                HasMobileAccess = false,
                HasDesktopAccess = true
            }
        };

        // System user ID for creation tracking
        Guid systemUserId = Guid.Empty;
        var existingAdmin = await _userRepository.GetByUsernameAsync("admin", cancellationToken);
        if (existingAdmin != null)
        {
            systemUserId = existingAdmin.Id;
        }

        // Create users
        foreach (var testUser in testUsers)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userRepository.GetByUsernameAsync(testUser.Username, cancellationToken);

                if (existingUser != null && !request.ForceReseed)
                {
                    result.SkippedUsers.Add($"{testUser.Username} (already exists)");
                    continue;
                }

                if (existingUser != null && request.ForceReseed)
                {
                    // Delete existing user if force reseed
                    existingUser.MarkAsDeleted(systemUserId == Guid.Empty ? existingUser.Id : systemUserId);
                    await _userRepository.UpdateAsync(existingUser, cancellationToken);
                    await _userRepository.SaveChangesAsync(cancellationToken);
                }

                // Hash password
                string passwordHash = _passwordHasher.HashPassword(testUser.Password, out string salt);

                // Create user
                var user = User.Create(
                    username: testUser.Username,
                    fullNameArabic: testUser.FullNameArabic,
                    passwordHash: passwordHash,
                    passwordSalt: salt,
                    role: testUser.Role,
                    hasMobileAccess: testUser.HasMobileAccess,
                    hasDesktopAccess: testUser.HasDesktopAccess,
                    email: testUser.Email,
                    phoneNumber: testUser.PhoneNumber,
                    createdByUserId: systemUserId == Guid.Empty ? Guid.NewGuid() : systemUserId
                );

                // Set additional properties
                user.UpdateProfile(
                    fullNameArabic: testUser.FullNameArabic,
                    fullNameEnglish: testUser.FullNameEnglish,
                    email: testUser.Email,
                    phoneNumber: testUser.PhoneNumber,
                    organization: testUser.Organization,
                    jobTitle: testUser.JobTitle,
                    modifiedByUserId: systemUserId == Guid.Empty ? user.Id : systemUserId
                );

                // ============== GRANT DEFAULT PERMISSIONS ==============
                var defaultPermissions = GetDefaultPermissionsForRole(testUser.Role);
                user.GrantPermissions(
                    permissions: defaultPermissions,
                    grantedBy: systemUserId == Guid.Empty ? user.Id : systemUserId,
                    reason: $"Default {testUser.Role} permissions"
                );
                // =======================================================

                await _userRepository.AddAsync(user, cancellationToken);
                await _userRepository.SaveChangesAsync(cancellationToken);

                result.CreatedUsers.Add($"{testUser.Username} ({testUser.Role}) - Password: {testUser.Password}");

                // Update systemUserId after creating admin
                if (testUser.Username == "admin" && systemUserId == Guid.Empty)
                {
                    systemUserId = user.Id;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.SkippedUsers.Add($"{testUser.Username} (error: {ex.Message})");
            }
        }

        result.Message = result.Success
            ? $"Seed completed successfully. Created {result.CreatedUsers.Count} users, skipped {result.SkippedUsers.Count}."
            : $"Seed completed with errors. Created {result.CreatedUsers.Count} users, skipped {result.SkippedUsers.Count}.";

        return result;
    }

    /// <summary>
    /// Get default permissions for a given role
    /// This duplicates the logic from PermissionSeeder to avoid circular dependencies
    /// </summary>
    private static List<Permission> GetDefaultPermissionsForRole(UserRole role)
    {
        return role switch
        {
            UserRole.Administrator => new List<Permission>
            {
                // Claims permissions (ALL 14)
                Permission.Claims_ViewAll, Permission.Claims_ViewAssigned, Permission.Claims_Create,
                Permission.Claims_Update, Permission.Claims_Delete, Permission.Claims_Submit,
                Permission.Claims_Assign, Permission.Claims_Reassign, Permission.Claims_Verify,
                Permission.Claims_Approve, Permission.Claims_Reject, Permission.Claims_Transition,
                Permission.Claims_Export, Permission.Claims_ViewHistory,
                
                // Evidence permissions (ALL 4)
                Permission.Evidence_View, Permission.Evidence_Upload, Permission.Evidence_Verify,
                Permission.Evidence_Delete,
                
                // Documents permissions (ALL 4)
                Permission.Documents_ViewSensitive, Permission.Documents_Download,
                Permission.Documents_Upload, Permission.Documents_Delete,
                
                // Buildings permissions (ALL 5)
                Permission.Buildings_View, Permission.Buildings_Create, Permission.Buildings_Update,
                Permission.Buildings_Assign, Permission.Buildings_Delete,
                
                // Persons permissions (ALL 5)
                Permission.Persons_View, Permission.Persons_Create, Permission.Persons_Update,
                Permission.Persons_Merge, Permission.Persons_Delete,
                
                // PropertyUnits permissions (ALL 5)
                Permission.PropertyUnits_View, Permission.PropertyUnits_Create,
                Permission.PropertyUnits_Update, Permission.PropertyUnits_Merge,
                Permission.PropertyUnits_Delete,
                
                // Surveys permissions (ALL 3)
                Permission.Surveys_Create, Permission.Surveys_View, Permission.Surveys_Export,
                
                // Admin permissions (ALL 9)
                Permission.Users_View, Permission.Users_Create, Permission.Users_Update,
                Permission.Users_Deactivate, Permission.Roles_Manage, Permission.Vocabularies_Manage,
                Permission.Security_Settings, Permission.Audit_ViewAll,
                
                // System permissions (ALL 4)
                Permission.System_Import, Permission.System_Export, Permission.System_Backup,
                Permission.System_Restore
            },

            UserRole.DataManager => new List<Permission>
            {
                // Claims (12 - no Approve/Reject)
                Permission.Claims_ViewAll, Permission.Claims_Create, Permission.Claims_Update,
                Permission.Claims_Delete, Permission.Claims_Submit, Permission.Claims_Assign,
                Permission.Claims_Reassign, Permission.Claims_Verify, Permission.Claims_Transition,
                Permission.Claims_Export, Permission.Claims_ViewHistory,
                
                // Evidence (3 - no Delete)
                Permission.Evidence_View, Permission.Evidence_Upload, Permission.Evidence_Verify,
                
                // Documents (3 - no Delete)
                Permission.Documents_ViewSensitive, Permission.Documents_Download, Permission.Documents_Upload,
                
                // Buildings (4 - no Delete)
                Permission.Buildings_View, Permission.Buildings_Create, Permission.Buildings_Update,
                Permission.Buildings_Assign,
                
                // Persons (4 - no Delete)
                Permission.Persons_View, Permission.Persons_Create, Permission.Persons_Update,
                Permission.Persons_Merge,
                
                // PropertyUnits (4 - no Delete)
                Permission.PropertyUnits_View, Permission.PropertyUnits_Create,
                Permission.PropertyUnits_Update, Permission.PropertyUnits_Merge,
                
                // Surveys (2)
                Permission.Surveys_View, Permission.Surveys_Export,
                
                // Admin (1)
                Permission.Audit_ViewAll,
                
                // System (2)
                Permission.System_Import, Permission.System_Export
            },

            UserRole.OfficeClerk => new List<Permission>
            {
                // Claims (5)
                Permission.Claims_ViewAll, Permission.Claims_Create, Permission.Claims_Update,
                Permission.Claims_Submit, Permission.Claims_Export,
                
                // Evidence (2)
                Permission.Evidence_View, Permission.Evidence_Upload,
                
                // Documents (3)
                Permission.Documents_ViewSensitive, Permission.Documents_Download, Permission.Documents_Upload,
                
                // Buildings (1)
                Permission.Buildings_View,
                
                // Persons (3)
                Permission.Persons_View, Permission.Persons_Create, Permission.Persons_Update,
                
                // PropertyUnits (1)
                Permission.PropertyUnits_View,
                
                // Surveys (1)
                Permission.Surveys_View
            },

            UserRole.FieldSupervisor => new List<Permission>
            {
                // Claims (4)
                Permission.Claims_ViewAll, Permission.Claims_Submit, Permission.Claims_Export,
                Permission.Claims_ViewHistory,
                
                // Evidence (1)
                Permission.Evidence_View,
                
                // Buildings (1)
                Permission.Buildings_View,
                
                // Persons (1)
                Permission.Persons_View,
                
                // PropertyUnits (1)
                Permission.PropertyUnits_View,
                
                // Surveys (3)
                Permission.Surveys_Create, Permission.Surveys_View, Permission.Surveys_Export
            },

            UserRole.FieldCollector => new List<Permission>
            {
                // Claims (1)
                Permission.Claims_ViewAssigned,
                
                // Evidence (2)
                Permission.Evidence_View, Permission.Evidence_Upload,
                
                // Surveys (2)
                Permission.Surveys_Create, Permission.Surveys_Export
            },

            UserRole.Analyst => new List<Permission>
            {
                // Claims (3)
                Permission.Claims_ViewAll, Permission.Claims_Export, Permission.Claims_ViewHistory,
                
                // Evidence (1)
                Permission.Evidence_View,
                
                // Buildings (1)
                Permission.Buildings_View,
                
                // Persons (1)
                Permission.Persons_View,
                
                // PropertyUnits (1)
                Permission.PropertyUnits_View,
                
                // Surveys (2)
                Permission.Surveys_View, Permission.Surveys_Export,
                
                // Admin (1)
                Permission.Audit_ViewAll,
                
                // System (1)
                Permission.System_Export
            },

            _ => new List<Permission>()
        };
    }
}