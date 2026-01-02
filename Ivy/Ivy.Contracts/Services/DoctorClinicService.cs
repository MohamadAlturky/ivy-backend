using Ivy.Contracts.Models;
using Ivy.Core.DataContext;
using Ivy.Core.Entities;
using IvyBackend;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Contracts.Services;

public interface IDoctorClinicService
{
    Task<Result<PaginatedResult<DoctorDto>>> GetDoctorsInClinicAsync(
        int clinicId,
        int page = 1,
        int pageSize = 10,
        string? doctorName = null
    );
    Task<Result<PaginatedResult<DoctorLocalizedDto>>> GetDoctorsInClinicLocalizedAsync(
        int clinicId,
        string language = "en",
        int page = 1,
        int pageSize = 10,
        string? doctorName = null
    );

    Task<Result> AddDoctorToClinicAsync(AddDoctorToClinicDto dto);
    Task<Result> RemoveDoctorFromClinicAsync(int doctorId, int clinicId);
}

public class DoctorClinicService : IDoctorClinicService
{
    private readonly IvyContext _context;

    public DoctorClinicService(IvyContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedResult<DoctorDto>>> GetDoctorsInClinicAsync(
        int clinicId,
        int page = 1,
        int pageSize = 10,
        string? doctorName = null
    )
    {
        // Query the Join Table
        var query = _context
            .Set<DoctorClinic>()
            .Include(dc => dc.Doctor)
            .ThenInclude(d => d.User)
            .Include(dc => dc.Clinic)
            .Where(dc => dc.ClinicId == clinicId)
            .AsQueryable();

        // Optional filtering by Doctor Name
        if (!string.IsNullOrWhiteSpace(doctorName))
        {
            query = query.Where(x =>
                x.Doctor.User.FirstName.Contains(doctorName)
                || x.Doctor.User.LastName.Contains(doctorName)
            );
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new DoctorDto
            {
                Id = x.Doctor.UserId,
                Rating = x.Doctor.Rating,
                DateOfBirth = x.Doctor.User.DateOfBirth,
                DisplayNameAr = x.Doctor.DisplayNameAr,
                DisplayNameEn = x.Doctor.DisplayNameEn,
                ProfileImageUrl = x.Doctor.ProfileImageUrl,
                Gender = x.Doctor.User.Gender,
                Specialities = x
                    .Doctor.DoctorDynamicProfileHistories.Where(h => h.IsLatest)
                    .SelectMany(h => h.DoctorMedicalSpecialities)
                    .Select(ms => new MedicalSpecialityDto
                    {
                        Id = ms.MedicalSpeciality.Id,
                        NameAr = ms.MedicalSpeciality.NameAr,
                        NameEn = ms.MedicalSpeciality.NameEn,
                        DescriptionAr = ms.MedicalSpeciality.DescriptionAr,
                        DescriptionEn = ms.MedicalSpeciality.DescriptionEn,
                    })
                    .ToList(),
                FirstName = x.Doctor.User.FirstName,
                MiddleName = x.Doctor.User.MiddleName,
                LastName = x.Doctor.User.LastName,
                Email = x.Doctor.User.Email,
                PhoneNumber = x.Doctor.User.PhoneNumber,
            })
            .ToListAsync();

        var result = PaginatedResult<DoctorDto>.Create(items, totalCount, page, pageSize);

        return Result<PaginatedResult<DoctorDto>>.Ok(DoctorClinicMessageCodes.SUCCESS, result);
    }

    public async Task<Result<PaginatedResult<DoctorLocalizedDto>>> GetDoctorsInClinicLocalizedAsync(
        int clinicId,
        string language = "en",
        int page = 1,
        int pageSize = 10,
        string? doctorName = null
    )
    {
        // Query the Join Table
        var query = _context
            .Set<DoctorClinic>()
            .Include(dc => dc.Doctor)
            .ThenInclude(d => d.User)
            .Include(dc => dc.Clinic)
            .Where(dc => dc.ClinicId == clinicId)
            .AsQueryable();

        // Optional filtering by Doctor Name
        if (!string.IsNullOrWhiteSpace(doctorName))
        {
            query = query.Where(x =>
                x.Doctor.User.FirstName.Contains(doctorName)
                || x.Doctor.User.LastName.Contains(doctorName)
            );
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new DoctorLocalizedDto
            {
                Id = x.Doctor.UserId,
                Rating = x.Doctor.Rating,
                FullName = language == "ar" ? x.Doctor.DisplayNameAr : x.Doctor.DisplayNameEn,
                ProfileImageUrl = x.Doctor.ProfileImageUrl,
                Specialities = x
                    .Doctor.DoctorDynamicProfileHistories.Where(h => h.IsLatest)
                    .SelectMany(h => h.DoctorMedicalSpecialities)
                    .Select(ms => new MedicalSpecialityLocalizedDto
                    {
                        Id = ms.MedicalSpeciality.Id,
                        IsActive = ms.MedicalSpeciality.IsActive,
                        Name =
                            language == "ar"
                                ? ms.MedicalSpeciality.NameAr
                                : ms.MedicalSpeciality.NameEn,
                        Description =
                            language == "ar"
                                ? ms.MedicalSpeciality.DescriptionAr
                                : ms.MedicalSpeciality.DescriptionEn,
                    })
                    .ToList(),
            })
            .ToListAsync();

        var result = PaginatedResult<DoctorLocalizedDto>.Create(items, totalCount, page, pageSize);

        return Result<PaginatedResult<DoctorLocalizedDto>>.Ok(
            DoctorClinicMessageCodes.SUCCESS,
            result
        );
    }

    public async Task<Result> AddDoctorToClinicAsync(AddDoctorToClinicDto dto)
    {
        // 1. Verify Doctor Exists
        var doctorExists = await _context.Set<Doctor>().AnyAsync(x => x.UserId == dto.DoctorId);
        if (!doctorExists)
            return Result.Error(DoctorClinicMessageCodes.DOCTOR_NOT_FOUND);

        // 2. Verify Clinic Exists
        var clinicExists = await _context.Set<Clinic>().AnyAsync(x => x.Id == dto.ClinicId);
        if (!clinicExists)
            return Result.Error(DoctorClinicMessageCodes.CLINIC_NOT_FOUND);

        // 3. Check for Duplicate Relationship
        var exists = await _context
            .Set<DoctorClinic>()
            .AnyAsync(x => x.DoctorId == dto.DoctorId && x.ClinicId == dto.ClinicId);

        if (exists)
        {
            return Result.Error(DoctorClinicMessageCodes.ALREADY_EXISTS);
        }

        // 4. Create Relationship
        var entity = new DoctorClinic { DoctorId = dto.DoctorId, ClinicId = dto.ClinicId };

        await _context.Set<DoctorClinic>().AddAsync(entity);
        await _context.SaveChangesAsync();

        return Result.Ok(DoctorClinicMessageCodes.ADDED);
    }

    public async Task<Result> RemoveDoctorFromClinicAsync(int doctorId, int clinicId)
    {
        var entity = await _context
            .Set<DoctorClinic>()
            .FirstOrDefaultAsync(x => x.DoctorId == doctorId && x.ClinicId == clinicId);

        if (entity == null)
        {
            return Result.Error(DoctorClinicMessageCodes.NOT_FOUND);
        }

        _context.Set<DoctorClinic>().Remove(entity);
        await _context.SaveChangesAsync();

        return Result.Ok(DoctorClinicMessageCodes.REMOVED);
    }
}

public static class DoctorClinicMessageCodes
{
    public const string DOCTOR_NOT_FOUND = "DOCTOR_NOT_FOUND";
    public const string CLINIC_NOT_FOUND = "CLINIC_NOT_FOUND";
    public const string ALREADY_EXISTS = "DOCTOR_ALREADY_IN_CLINIC";
    public const string NOT_FOUND = "RELATIONSHIP_NOT_FOUND";
    public const string ADDED = "DOCTOR_ADDED_TO_CLINIC";
    public const string REMOVED = "DOCTOR_REMOVED_FROM_CLINIC";
    public const string SUCCESS = "SUCCESS";
}
