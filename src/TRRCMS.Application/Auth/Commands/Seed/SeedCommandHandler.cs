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

        // System user ID for creation tracking (using admin's ID, or Guid.Empty if admin doesn't exist yet)
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
                    createdByUserId: systemUserId == Guid.Empty ? Guid.NewGuid() : systemUserId // Use system user or self-reference
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

                // Admin doesn't need to change password
                if (testUser.Role == UserRole.Administrator)
                {
                    // Use reflection or create a method to set MustChangePassword to false
                    // For now, admin will have MustChangePassword = true by default
                }

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
}