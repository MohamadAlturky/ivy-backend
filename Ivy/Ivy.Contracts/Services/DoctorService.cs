using Ivy.Contracts.Models;
using Ivy.Core.DataContext;
using Ivy.Core.Entities;
using IvyBackend; // Assuming Result<T> is here
using Microsoft.EntityFrameworkCore;

namespace Ivy.Contracts.Services;

public interface IDoctorService
{
    Task<Result<PaginatedResult<DoctorDto>>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? name = null,
        int? specialityId = null
    );

    Task<Result<PaginatedResult<DoctorLocalizedDto>>> GetAllLocalizedAsync(
        string language = "en",
        int page = 1,
        int pageSize = 10,
        string? name = null,
        int? specialityId = null
    );

    Task<Result<DoctorDto>> CreateAsync(CreateDoctorDto dto);
    Task<Result<DoctorDto>> UpdateAsync(int id, UpdateDoctorDto dto);
    Task<Result> DeleteAsync(int id);
}

public class DoctorService : IDoctorService
{
    private readonly IvyContext _context;

    public DoctorService(IvyContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedResult<DoctorDto>>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? name = null,
        int? specialityId = null
    )
    {
        // Start from Doctor to ensure we only get doctors
        var query = _context
            .Set<Doctor>()
            .Include(d => d.User)
            .Include(d => d.DoctorDynamicProfileHistories)
            .ThenInclude(h => h.DoctorMedicalSpecialities)
            .ThenInclude(dms => dms.MedicalSpeciality)
            .AsQueryable();

        // Filtering
        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(x =>
                x.User.FirstName.Contains(name)
                || x.User.LastName.Contains(name)
                || x.User.UserName.Contains(name)
            );
        }

        if (specialityId.HasValue)
        {
            query = query.Where(x =>
                x.DoctorDynamicProfileHistories.Any(h =>
                    h.IsLatest
                    && h.DoctorMedicalSpecialities.Any(ms =>
                        ms.MedicalSpecialityId == specialityId.Value
                    )
                )
            );
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.UserId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new DoctorDto
            {
                Id = x.UserId,
                FirstName = x.User.FirstName,
                MiddleName = x.User.MiddleName,
                LastName = x.User.LastName,
                Email = x.User.Email,
                PhoneNumber = x.User.PhoneNumber,
                ProfileImageUrl = x.ProfileImageUrl,
                Gender = x.User.Gender,
                DisplayNameAr = x.DisplayNameAr,
                DisplayNameEn = x.DisplayNameEn,
                DateOfBirth = x.User.DateOfBirth,
                Specialities = x
                    .DoctorDynamicProfileHistories.Where(h => h.IsLatest)
                    .SelectMany(h => h.DoctorMedicalSpecialities)
                    .Select(ms => new MedicalSpecialityDto
                    {
                        Id = ms.MedicalSpeciality.Id,
                        NameAr = ms.MedicalSpeciality.NameAr,
                        NameEn = ms.MedicalSpeciality.NameEn,
                    })
                    .ToList(),
            })
            .ToListAsync();

        var result = PaginatedResult<DoctorDto>.Create(items, totalCount, page, pageSize);

        return Result<PaginatedResult<DoctorDto>>.Ok(DoctorServiceMessageCodes.SUCCESS, result);
    }

    public async Task<Result<PaginatedResult<DoctorLocalizedDto>>> GetAllLocalizedAsync(
        string language = "en",
        int page = 1,
        int pageSize = 10,
        string? name = null,
        int? specialityId = null
    )
    {
        var query = _context
            .Set<Doctor>()
            .Include(d => d.User)
            .Include(d => d.DoctorDynamicProfileHistories)
            .ThenInclude(h => h.DoctorMedicalSpecialities)
            .ThenInclude(dms => dms.MedicalSpeciality)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(x =>
                x.User.FirstName.Contains(name) || x.User.LastName.Contains(name)
            );
        }

        if (specialityId.HasValue)
        {
            query = query.Where(x =>
                x.DoctorDynamicProfileHistories.Any(h =>
                    h.IsLatest
                    && h.DoctorMedicalSpecialities.Any(ms =>
                        ms.MedicalSpecialityId == specialityId.Value
                    )
                )
            );
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.UserId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new DoctorLocalizedDto
            {
                Id = x.UserId,
                FullName = language == "ar" ? x.DisplayNameAr : x.DisplayNameEn,
                ProfileImageUrl = x.ProfileImageUrl,
                Specialities = x
                    .DoctorDynamicProfileHistories.Where(h => h.IsLatest)
                    .SelectMany(h => h.DoctorMedicalSpecialities)
                    .Select(ms => new MedicalSpecialityLocalizedDto
                    {
                        Id = ms.MedicalSpeciality.Id,
                        Name =
                            language == "ar"
                                ? ms.MedicalSpeciality.NameAr
                                : ms.MedicalSpeciality.NameEn,
                    })
                    .ToList(),
            })
            .ToListAsync();

        var result = PaginatedResult<DoctorLocalizedDto>.Create(items, totalCount, page, pageSize);

        return Result<PaginatedResult<DoctorLocalizedDto>>.Ok(
            DoctorServiceMessageCodes.SUCCESS,
            result
        );
    }

    public async Task<Result<DoctorDto>> CreateAsync(CreateDoctorDto dto)
    {
        // 1. Validation (Email/Username uniqueness)
        bool userExists = await _context
            .Set<User>()
            .AnyAsync(u => u.Email == dto.Email || u.UserName == dto.UserName);

        if (userExists)
        {
            return Result<DoctorDto>.Error(DoctorServiceMessageCodes.DUPLICATE_USER, null!);
        }

        // 2. Create User Entity
        var user = new User
        {
            FirstName = dto.FirstName,
            MiddleName = dto.MiddleName,
            LastName = dto.LastName,
            UserName = dto.UserName,
            Email = dto.Email,
            Password = dto.Password, // Note: In production, hash this password!
            PhoneNumber = dto.PhoneNumber,
            Gender = dto.Gender,
            DateOfBirth = dto.DateOfBirth,
            UserType = UserType.Doctor,
            IsPhoneVerified = false, // Default
        };

        await _context.Set<User>().AddAsync(user);
        await _context.SaveChangesAsync(); // Save to generate UserId

        // 3. Create Doctor Entity
        var doctor = new Doctor { UserId = user.Id, ProfileImageUrl = dto.ProfileImageUrl };

        await _context.Set<Doctor>().AddAsync(doctor);

        // 4. Create Initial Dynamic Profile History
        var history = new DoctorDynamicProfileHistory
        {
            DoctorId = doctor.UserId,
            IsLatest = true,
            JsonData = "{}", // Empty JSON or serialized additional data
        };

        // 5. Link Specialities
        if (dto.MedicalSpecialityIds != null && dto.MedicalSpecialityIds.Any())
        {
            foreach (var specId in dto.MedicalSpecialityIds)
            {
                history.DoctorMedicalSpecialities.Add(
                    new DoctorMedicalSpeciality { MedicalSpecialityId = specId }
                );
            }
        }

        await _context.Set<DoctorDynamicProfileHistory>().AddAsync(history);
        await _context.SaveChangesAsync();

        return await GetDoctorDtoById(user.Id);
    }

    public async Task<Result<DoctorDto>> UpdateAsync(int id, UpdateDoctorDto dto)
    {
        var doctor = await _context
            .Set<Doctor>()
            .Include(d => d.User)
            .Include(d => d.DoctorDynamicProfileHistories)
            .ThenInclude(h => h.DoctorMedicalSpecialities)
            .FirstOrDefaultAsync(d => d.UserId == id);

        if (doctor == null)
        {
            return Result<DoctorDto>.Error(DoctorServiceMessageCodes.NOT_FOUND, null!);
        }

        // 1. Update User Details
        doctor.User.FirstName = dto.FirstName;
        doctor.User.MiddleName = dto.MiddleName;
        doctor.User.LastName = dto.LastName;
        doctor.User.PhoneNumber = dto.PhoneNumber;

        // 2. Update Doctor Details
        doctor.ProfileImageUrl = dto.ProfileImageUrl;

        // 3. Handle History Versioning (Pattern: Archive old, Create new)
        var currentHistory = doctor.DoctorDynamicProfileHistories.FirstOrDefault(h => h.IsLatest);
        if (currentHistory != null)
        {
            currentHistory.IsLatest = false;
        }

        var newHistory = new DoctorDynamicProfileHistory
        {
            DoctorId = doctor.UserId,
            IsLatest = true,
            JsonData = currentHistory?.JsonData ?? "{}",
        };

        // Add new specialities
        if (dto.MedicalSpecialityIds != null)
        {
            foreach (var specId in dto.MedicalSpecialityIds)
            {
                newHistory.DoctorMedicalSpecialities.Add(
                    new DoctorMedicalSpeciality { MedicalSpecialityId = specId }
                );
            }
        }

        await _context.Set<DoctorDynamicProfileHistory>().AddAsync(newHistory);
        await _context.SaveChangesAsync();

        return await GetDoctorDtoById(id);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        // Deleting the User will typically cascade delete the Doctor entity
        // depending on DB configuration, but we will delete explicitly to be safe.

        var doctor = await _context.Set<Doctor>().FindAsync(id);
        var user = await _context.Set<User>().FindAsync(id);

        if (doctor == null || user == null)
        {
            return Result.Error(DoctorServiceMessageCodes.NOT_FOUND);
        }

        _context.Set<Doctor>().Remove(doctor);
        _context.Set<User>().Remove(user);

        await _context.SaveChangesAsync();

        return Result.Ok(DoctorServiceMessageCodes.DELETED);
    }

    // Helper method to fetch and map a single DoctorDto
    private async Task<Result<DoctorDto>> GetDoctorDtoById(int id)
    {
        var doctorDto = await _context
            .Set<Doctor>()
            .Where(d => d.UserId == id)
            .Select(x => new DoctorDto
            {
                Id = x.UserId,
                FirstName = x.User.FirstName,
                MiddleName = x.User.MiddleName,
                LastName = x.User.LastName,
                Email = x.User.Email,
                PhoneNumber = x.User.PhoneNumber,
                ProfileImageUrl = x.ProfileImageUrl,
                Gender = x.User.Gender,
                DateOfBirth = x.User.DateOfBirth,
                Specialities = x
                    .DoctorDynamicProfileHistories.Where(h => h.IsLatest)
                    .SelectMany(h => h.DoctorMedicalSpecialities)
                    .Select(ms => new MedicalSpecialityDto
                    {
                        Id = ms.MedicalSpeciality.Id,
                        NameAr = ms.MedicalSpeciality.NameAr,
                        NameEn = ms.MedicalSpeciality.NameEn,
                    })
                    .ToList(),
                DisplayNameAr = x.DisplayNameAr,
                DisplayNameEn = x.DisplayNameEn,
            })
            .FirstOrDefaultAsync();

        if (doctorDto == null)
            return Result<DoctorDto>.Error(DoctorServiceMessageCodes.NOT_FOUND, null!);

        return Result<DoctorDto>.Ok(DoctorServiceMessageCodes.SUCCESS, doctorDto);
    }
}

public static class DoctorServiceMessageCodes
{
    public const string INVALID_DATA = "DOCTOR_INVALID_DATA";
    public const string DUPLICATE_USER = "DOCTOR_DUPLICATE_USER"; // Email or UserName exists
    public const string NOT_FOUND = "DOCTOR_NOT_FOUND";
    public const string CREATED = "DOCTOR_CREATED";
    public const string UPDATED = "DOCTOR_UPDATED";
    public const string DELETED = "DOCTOR_DELETED";
    public const string SUCCESS = "DOCTOR_SUCCESS";
}
