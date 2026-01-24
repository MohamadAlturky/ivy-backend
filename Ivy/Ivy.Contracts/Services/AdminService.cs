using Ivy.Contracts.Models;
using Ivy.Contracts.Services;
using Ivy.Core.DataContext;
using Ivy.Core.Entities;
using Ivy.Core.Jwt;
using IvyBackend;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Contracts.Services;

public interface IAdminService
{
    Task<Result<AdminLoginResponseDto>> LoginAsync(AdminLoginDto dto, string language = "en");

    Task<Result<AdminDto>> GetProfileAsync(int adminId, string language = "en");

    Task<Result<AdminDto>> UpdateProfileAsync(
        int adminId,
        UpdateAdminProfileDto dto,
        string language = "en"
    );

    Task<Result<bool>> EmailExistsAsync(string email, string language = "en");

    Task<Result> ChangePasswordAsync(int adminId, ChangePasswordDto dto, string language = "en");
}

public class AdminService : IAdminService
{
    private readonly IvyContext _context;
    private readonly JwtService _jwtService;

    public AdminService(IvyContext context, JwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<Result<AdminLoginResponseDto>> LoginAsync(
        AdminLoginDto dto,
        string language = "en"
    )
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return Result<AdminLoginResponseDto>.Error(
                AdminServiceMessageCodes.INVALID_LOGIN_DATA,
                null!
            );
        }

        // Find admin by email
        var admin = await _context
            .Set<Admin>()
            .Include(a => a.Clinic)
            .FirstOrDefaultAsync(a => a.Email == dto.Email && !a.IsDeleted);

        if (admin == null)
        {
            return Result<AdminLoginResponseDto>.Error(
                AdminServiceMessageCodes.INVALID_CREDENTIALS,
                null!
            );
        }

        // Verify password
        if (!VerifyPassword(dto.Password, admin.Password))
        {
            return Result<AdminLoginResponseDto>.Error(
                AdminServiceMessageCodes.INVALID_CREDENTIALS,
                null!
            );
        }

        // Check if account is active
        if (!admin.IsActive)
        {
            return Result<AdminLoginResponseDto>.Error(
                AdminServiceMessageCodes.ACCOUNT_INACTIVE,
                null!
            );
        }

        var token = _jwtService.GenerateToken(admin.Id, admin.Email, "admin");

        // Map to DTO
        var adminDto = new AdminDto
        {
            Id = admin.Id,
            Email = admin.Email,
            IsActive = admin.IsActive,
            IsClinicAdmin = admin.ClinicId != null,
            Clinic =
                admin.Clinic != null
                    ? new ClinicLocalizedDto
                    {
                        Id = admin.Clinic.Id,
                        Name = language == "ar" ? admin.Clinic.NameAr : admin.Clinic.NameEn,
                        Description =
                            language == "ar"
                                ? admin.Clinic.DescriptionAr
                                : admin.Clinic.DescriptionEn,
                        ContactPhoneNumber = admin.Clinic.ContactPhoneNumber,
                        ContactEmail = admin.Clinic.ContactEmail,
                        Location = new LocationLocalizedDto
                        {
                            Id = admin.Clinic.LocationId,
                            Name =
                                language == "ar"
                                    ? admin.Clinic.Location.NameAr
                                    : admin.Clinic.Location.NameEn,
                        },
                        IsActive = admin.Clinic.IsActive,
                    }
                    : null,
            CreatedAt = admin.CreatedAt,
        };

        return Result<AdminLoginResponseDto>.Ok(
            AdminServiceMessageCodes.LOGIN_SUCCESS,
            new AdminLoginResponseDto
            {
                Profile = adminDto,
                Token = token,
                IsClinicAdmin = admin.ClinicId != null,
            }
        );
    }

    public async Task<Result<AdminDto>> GetProfileAsync(int adminId, string language = "en")
    {
        if (adminId <= 0)
        {
            return Result<AdminDto>.Error(AdminServiceMessageCodes.INVALID_ID, null!);
        }

        var admin = await _context
            .Set<Admin>()
            .Include(a => a.Clinic)
            .FirstOrDefaultAsync(a => a.Id == adminId && !a.IsDeleted);

        if (admin == null)
        {
            return Result<AdminDto>.Error(AdminServiceMessageCodes.NOT_FOUND, null!);
        }

        if (!admin.IsActive)
        {
            return Result<AdminDto>.Error(AdminServiceMessageCodes.ACCOUNT_INACTIVE, null!);
        }
        var adminDto = new AdminDto
        {
            Id = admin.Id,
            Email = admin.Email,
            IsActive = admin.IsActive,
            CreatedAt = admin.CreatedAt,
            IsClinicAdmin = admin.ClinicId != null,
            Clinic =
                admin.Clinic != null
                    ? new ClinicLocalizedDto
                    {
                        Id = admin.Clinic.Id,
                        Name = language == "ar" ? admin.Clinic.NameAr : admin.Clinic.NameEn,
                        Description =
                            language == "ar"
                                ? admin.Clinic.DescriptionAr
                                : admin.Clinic.DescriptionEn,
                        ContactPhoneNumber = admin.Clinic.ContactPhoneNumber,
                        ContactEmail = admin.Clinic.ContactEmail,
                        Location = new LocationLocalizedDto
                        {
                            Id = admin.Clinic.LocationId,
                            Name =
                                language == "ar"
                                    ? admin.Clinic.Location.NameAr
                                    : admin.Clinic.Location.NameEn,
                        },
                        IsActive = admin.Clinic.IsActive,
                    }
                    : null,
        };

        return Result<AdminDto>.Ok(AdminServiceMessageCodes.PROFILE_RETRIEVED, adminDto);
    }

    public async Task<Result<AdminDto>> UpdateProfileAsync(
        int adminId,
        UpdateAdminProfileDto dto,
        string language = "en"
    )
    {
        if (adminId <= 0 || string.IsNullOrWhiteSpace(dto.Email))
        {
            return Result<AdminDto>.Error(AdminServiceMessageCodes.INVALID_DATA, null!);
        }

        var entity = await _context
            .Set<Admin>()
            .Include(a => a.Clinic)
            .FirstOrDefaultAsync(a => a.Id == adminId && !a.IsDeleted);

        if (entity == null)
        {
            return Result<AdminDto>.Error(AdminServiceMessageCodes.NOT_FOUND, null!);
        }

        // Check if email is already taken by another admin
        var emailExists = await _context
            .Set<Admin>()
            .AnyAsync(a => a.Id != adminId && a.Email == dto.Email && !a.IsDeleted);

        if (emailExists)
        {
            return Result<AdminDto>.Error(AdminServiceMessageCodes.EMAIL_ALREADY_EXISTS, null!);
        }

        // Handle Optional Password Change
        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
            {
                return Result<AdminDto>.Error(
                    AdminServiceMessageCodes.CURRENT_PASSWORD_REQUIRED,
                    null!
                );
            }

            if (!VerifyPassword(dto.CurrentPassword, entity.Password))
            {
                return Result<AdminDto>.Error(
                    AdminServiceMessageCodes.INVALID_CURRENT_PASSWORD,
                    null!
                );
            }

            entity.Password = HashPassword(dto.NewPassword);
        }

        // Update fields
        entity.Email = dto.Email;
        entity.UpdatedAt = DateTime.UtcNow;

        _context.Set<Admin>().Update(entity);
        await _context.SaveChangesAsync();

        var resultDto = new AdminDto
        {
            Id = entity.Id,
            Email = entity.Email,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            IsClinicAdmin = entity.ClinicId != null,
            Clinic =
                entity.Clinic != null
                    ? new ClinicLocalizedDto
                    {
                        Id = entity.Clinic.Id,
                        Name = language == "ar" ? entity.Clinic.NameAr : entity.Clinic.NameEn,
                        Description =
                            language == "ar"
                                ? entity.Clinic.DescriptionAr
                                : entity.Clinic.DescriptionEn,
                        ContactPhoneNumber = entity.Clinic.ContactPhoneNumber,
                        ContactEmail = entity.Clinic.ContactEmail,
                        Location = new LocationLocalizedDto
                        {
                            Id = entity.Clinic.LocationId,
                            Name =
                                language == "ar"
                                    ? entity.Clinic.Location.NameAr
                                    : entity.Clinic.Location.NameEn,
                        },
                        IsActive = entity.Clinic.IsActive,
                    }
                    : null,
        };

        return Result<AdminDto>.Ok(AdminServiceMessageCodes.UPDATED, resultDto);
    }

    public async Task<Result<bool>> EmailExistsAsync(string email, string language = "en")
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result<bool>.Error(AdminServiceMessageCodes.INVALID_EMAIL, false);
        }

        var exists = await _context.Set<Admin>().AnyAsync(a => a.Email == email && !a.IsDeleted);

        return Result<bool>.Ok(AdminServiceMessageCodes.EMAIL_CHECK_SUCCESS, exists);
    }

    public async Task<Result> ChangePasswordAsync(
        int adminId,
        ChangePasswordDto dto,
        string language = "en"
    )
    {
        if (
            adminId <= 0
            || string.IsNullOrWhiteSpace(dto.CurrentPassword)
            || string.IsNullOrWhiteSpace(dto.NewPassword)
        )
        {
            return Result.Error(AdminServiceMessageCodes.INVALID_DATA);
        }

        var entity = await _context
            .Set<Admin>()
            .Include(a => a.Clinic)
            .FirstOrDefaultAsync(a => a.Id == adminId && !a.IsDeleted);

        if (entity == null)
        {
            return Result.Error(AdminServiceMessageCodes.NOT_FOUND);
        }

        // Verify current password
        if (!VerifyPassword(dto.CurrentPassword, entity.Password))
        {
            return Result.Error(AdminServiceMessageCodes.INVALID_CURRENT_PASSWORD);
        }

        // Update password
        entity.Password = HashPassword(dto.NewPassword);
        entity.UpdatedAt = DateTime.UtcNow;

        _context.Set<Admin>().Update(entity);
        await _context.SaveChangesAsync();

        return Result.Ok(AdminServiceMessageCodes.PASSWORD_CHANGED);
    }

    // -- Helpers --
    private static string HashPassword(string password)
    {
        // In production, use a proper password hashing library like BCrypt, Argon2, etc.
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
    }

    private static bool VerifyPassword(string password, string hashedPassword)
    {
        var inputHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        return inputHash == hashedPassword;
    }
}

public static class AdminServiceMessageCodes
{
    public const string INVALID_LOGIN_DATA = "ADMIN_INVALID_LOGIN_DATA";
    public const string INVALID_CREDENTIALS = "ADMIN_INVALID_CREDENTIALS";
    public const string ACCOUNT_INACTIVE = "ADMIN_ACCOUNT_INACTIVE";
    public const string LOGIN_SUCCESS = "ADMIN_LOGIN_SUCCESS";

    public const string NOT_FOUND = "ADMIN_NOT_FOUND";
    public const string INVALID_ID = "ADMIN_INVALID_ID";
    public const string INVALID_DATA = "ADMIN_INVALID_DATA";
    public const string INVALID_EMAIL = "ADMIN_INVALID_EMAIL";

    public const string PROFILE_RETRIEVED = "ADMIN_PROFILE_RETRIEVED";
    public const string UPDATED = "ADMIN_UPDATED";
    public const string EMAIL_ALREADY_EXISTS = "ADMIN_EMAIL_ALREADY_EXISTS";
    public const string EMAIL_CHECK_SUCCESS = "ADMIN_EMAIL_CHECK_SUCCESS";

    public const string CURRENT_PASSWORD_REQUIRED = "ADMIN_CURRENT_PASSWORD_REQUIRED";
    public const string INVALID_CURRENT_PASSWORD = "ADMIN_INVALID_CURRENT_PASSWORD";
    public const string PASSWORD_CHANGED = "ADMIN_PASSWORD_CHANGED";
}
