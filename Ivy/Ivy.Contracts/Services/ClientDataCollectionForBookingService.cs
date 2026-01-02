using Ivy.Contracts.Models;
using Ivy.Core.DataContext;
using Ivy.Core.Entities;
using IvyBackend;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Contracts.Services;

public interface IClientDataCollectionForBookingService
{
    // Clinic Methods
    Task<Result<PaginatedResult<ClinicForBookingLocalizedDto>>> GetClinicsLocalizedAsync(
        string language = "en",
        int page = 1,
        int pageSize = 10,
        string? name = null,
        int? cityId = null,
        bool? isActive = null,
        int? governorateId = null,
        int? doctorId = null
    );

    // Doctor Methods
    Task<Result<PaginatedResult<DoctorForBookingLocalizedDto>>> GetDoctorsLocalizedAsync(
        string language = "en",
        int page = 1,
        int pageSize = 10,
        string? name = null,
        int? clinicId = null,
        int? specialty = null,
        bool? isActive = null
    );

    // Working Days Methods
    Task<Result<List<DateTime>>> GetWorkingDaysAsync(int doctorId, int clinicId);
    Task<Result<List<TimeSegmentDto>>> GetWorkingDaysByDateAsync(
        int doctorId,
        int clinicId,
        int segmentDurationMinutes,
        int stepInMinutes,
        DateTime date
    );
}

public class ClientDataCollectionForBookingService : IClientDataCollectionForBookingService
{
    private readonly IvyContext _context;

    public ClientDataCollectionForBookingService(IvyContext context)
    {
        _context = context;
    }

    public async Task<
        Result<PaginatedResult<ClinicForBookingLocalizedDto>>
    > GetClinicsLocalizedAsync(
        string language = "en",
        int page = 1,
        int pageSize = 10,
        string? name = null,
        int? cityId = null,
        bool? isActive = null,
        int? governorateId = null,
        int? doctorId = null
    )
    {
        var query = _context.Set<Clinic>().AsQueryable();

        // Filtering
        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        if (cityId.HasValue)
        {
            query = query.Where(x => x.Location.CityId == cityId.Value);
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(x => x.NameAr.Contains(name) || x.NameEn.Contains(name));
        }
        if (governorateId.HasValue)
        {
            query = query.Where(x => x.Location.City.GovernorateId == governorateId.Value);
        }
        if (doctorId.HasValue)
        {
            query = query.Where(x => x.DoctorClinics.Any(dc => dc.Doctor.UserId == doctorId.Value));
        }

        // Pagination
        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ClinicForBookingLocalizedDto
            {
                Id = x.Id,
                Name = language == "ar" ? x.NameAr : x.NameEn,
                Description = language == "ar" ? x.DescriptionAr : x.DescriptionEn,
                ContactEmail = x.ContactEmail,
                ContactPhoneNumber = x.ContactPhoneNumber,
                Location = new LocationLocalizedDto
                {
                    Id = x.Location.Id,
                    Latitude = x.Location.Latitude,
                    Longitude = x.Location.Longitude,
                    Name = language == "ar" ? x.Location.NameAr : x.Location.NameEn,
                    City = new CityLocalizedDto
                    {
                        Id = x.Location.CityId,
                        Name = language == "ar" ? x.Location.City.NameAr : x.Location.City.NameEn,
                        Governorate = new GovernorateLocalizedDto
                        {
                            Id = x.Location.City.GovernorateId,
                            Name =
                                language == "ar"
                                    ? x.Location.City.Governorate.NameAr
                                    : x.Location.City.Governorate.NameEn,
                            IsActive = x.Location.City.Governorate.IsActive,
                        },
                        IsActive = x.Location.City.IsActive,
                    },
                },
            })
            .ToListAsync();

        var result = PaginatedResult<ClinicForBookingLocalizedDto>.Create(
            items,
            totalCount,
            page,
            pageSize
        );

        return Result<PaginatedResult<ClinicForBookingLocalizedDto>>.Ok(
            ClientDataCollectionForBookingServiceMessageCodes.SUCCESS,
            result
        );
    }

    public async Task<
        Result<PaginatedResult<DoctorForBookingLocalizedDto>>
    > GetDoctorsLocalizedAsync(
        string language = "en",
        int page = 1,
        int pageSize = 10,
        string? name = null,
        int? clinicId = null,
        int? specialty = null,
        bool? isActive = null
    )
    {
        var query = _context.Set<Doctor>().AsQueryable();

        // Filtering
        if (isActive.HasValue)
        {
            query = query.Where(x => x.User.IsActive == isActive.Value);
        }

        if (clinicId.HasValue)
        {
            query = query.Where(x => x.DoctorClinics.Any(dc => dc.ClinicId == clinicId.Value));
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(x =>
                x.DisplayNameAr.Contains(name) || x.DisplayNameEn.Contains(name)
            );
        }

        if (specialty.HasValue)
        {
            query = query.Where(x =>
                x.DoctorDynamicProfileHistories.Any(h =>
                    h.IsLatest
                    && h.DoctorMedicalSpecialities.Any(ms =>
                        ms.MedicalSpecialityId == specialty.Value
                    )
                )
            );
        }

        // Pagination
        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.UserId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new DoctorForBookingLocalizedDto
            {
                Id = x.UserId,
                DisplayName = language == "ar" ? x.DisplayNameAr : x.DisplayNameEn,
                Email = x.User.Email,
                PhoneNumber = x.User.PhoneNumber,
                ProfileImageUrl = x.ProfileImageUrl,
                Gender = x.User.Gender,
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
                        Description =
                            language == "ar"
                                ? ms.MedicalSpeciality.DescriptionAr
                                : ms.MedicalSpeciality.DescriptionEn,
                        IsActive = ms.MedicalSpeciality.IsActive,
                    })
                    .ToList(),
                Rating = x.Rating,
                DateOfBirth = x.User.DateOfBirth,
            })
            .ToListAsync();

        var result = PaginatedResult<DoctorForBookingLocalizedDto>.Create(
            items,
            totalCount,
            page,
            pageSize
        );

        return Result<PaginatedResult<DoctorForBookingLocalizedDto>>.Ok(
            ClientDataCollectionForBookingServiceMessageCodes.SUCCESS,
            result
        );
    }

    public async Task<Result<List<DateTime>>> GetWorkingDaysAsync(int doctorId, int clinicId)
    {
        var doctorClinic = await _context
            .Set<DoctorClinic>()
            .FirstOrDefaultAsync(dc => dc.DoctorId == doctorId && dc.ClinicId == clinicId);

        if (doctorClinic == null)
        {
            return Result<List<DateTime>>.Error(
                ClientDataCollectionForBookingServiceMessageCodes.NOT_FOUND,
                new List<DateTime>()
            );
        }

        var today = DateTime.Now.Date;
        var endDate = today.AddMonths(1);

        var workingDays = await _context
            .Set<DoctorWorkingTimes>()
            .Where(wt =>
                wt.DoctorClinicId == doctorClinic.Id
                && wt.IsActive
                && !wt.IsDeleted
                && wt.StartTime.Date >= today
                && wt.StartTime.Date <= endDate
            )
            .Select(wt => wt.StartTime.Date)
            .Distinct()
            .OrderBy(date => date)
            .ToListAsync();

        return Result<List<DateTime>>.Ok(
            ClientDataCollectionForBookingServiceMessageCodes.SUCCESS,
            workingDays
        );
    }

    public async Task<Result<List<TimeSegmentDto>>> GetWorkingDaysByDateAsync(
        int doctorId,
        int clinicId,
        int segmentDurationMinutes,
        int stepInMinutes,
        DateTime date
    )
    {
        var doctorClinic = await _context
            .Set<DoctorClinic>()
            .FirstOrDefaultAsync(dc => dc.DoctorId == doctorId && dc.ClinicId == clinicId);

        if (doctorClinic == null)
        {
            return Result<List<TimeSegmentDto>>.Error(
                ClientDataCollectionForBookingServiceMessageCodes.NOT_FOUND,
                []
            );
        }

        var targetDate = date.Date;
        var nextDay = targetDate.AddDays(1);

        var workingTimes = await _context
            .Set<DoctorWorkingTimes>()
            .Where(wt =>
                wt.DoctorClinicId == doctorClinic.Id
                && wt.IsActive
                && !wt.IsDeleted
                && wt.StartTime >= targetDate
                && wt.StartTime < nextDay
            )
            .OrderBy(wt => wt.StartTime)
            .ToListAsync();

        var result = new List<TimeSegmentDto>();

        foreach (var wt in workingTimes)
        {
            var slotStart = wt.StartTime;
            var workEnd = wt.EndTime;

            while (slotStart.AddMinutes(segmentDurationMinutes) <= workEnd)
            {
                result.Add(
                    new TimeSegmentDto
                    {
                        StartTime = slotStart.TimeOfDay,
                        EndTime = slotStart.AddMinutes(segmentDurationMinutes).TimeOfDay,
                    }
                );

                slotStart = slotStart.AddMinutes(stepInMinutes);
            }
        }

        return Result<List<TimeSegmentDto>>.Ok(
            ClientDataCollectionForBookingServiceMessageCodes.SUCCESS,
            result
        );
    }
}

public static class ClientDataCollectionForBookingServiceMessageCodes
{
    public const string SUCCESS = "CLIENT_DATA_COLLECTION_FOR_BOOKING_SUCCESS";
    public const string NOT_FOUND = "CLIENT_DATA_COLLECTION_FOR_BOOKING_NOT_FOUND";
}
