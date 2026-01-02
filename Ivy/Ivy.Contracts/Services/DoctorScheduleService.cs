using Ivy.Contracts.Models;
using Ivy.Core.DataContext;
using Ivy.Core.Entities;
using IvyBackend;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Contracts.Services;

public interface IDoctorScheduleService
{
    Task<Result> AddWorkingHoursAsync(int doctorId, int clinicId, TimeRangeDto timeRange);
    Task<Result> RemoveWorkingHoursAsync(int workingHoursId);
    Task<Result> UpdateWorkingHoursAsync(int workingHoursId, TimeRangeDto timeRange);
    Task<Result<PaginatedResult<DoctorWorkingTimesDto>>> GetWorkingHoursAsync(
        int? doctorId,
        int? clinicId,
        string language,
        string? searchKey,
        int page = 1,
        int pageSize = 10
    );
}

public class DoctorScheduleService : IDoctorScheduleService
{
    private readonly IvyContext _context;

    public DoctorScheduleService(IvyContext context)
    {
        _context = context;
    }

    public async Task<Result> AddWorkingHoursAsync(
        int doctorId,
        int clinicId,
        TimeRangeDto timeRange
    )
    {
        if (timeRange.StartTime >= timeRange.EndTime)
        {
            return Result.Error(DoctorScheduleMessageCodes.INVALID_TIME_RANGE);
        }
        if (timeRange.StartTime.Date != timeRange.EndTime.Date)
        {
            return Result.Error(
                DoctorScheduleMessageCodes.INVALID_TIME_RANGE_IT_SHOULD_BE_IN_THE_SAME_DAY
            );
        }

        var doctorClinic = await _context
            .Set<DoctorClinic>()
            .FirstOrDefaultAsync(x => x.DoctorId == doctorId && x.ClinicId == clinicId);

        if (doctorClinic == null)
        {
            return Result.Error(DoctorScheduleMessageCodes.NOT_FOUND);
        }

        var entity = new DoctorWorkingTimes
        {
            DoctorClinicId = doctorClinic.Id,
            StartTime = timeRange.StartTime,
            EndTime = timeRange.EndTime,
        };

        await _context.Set<DoctorWorkingTimes>().AddAsync(entity);
        await _context.SaveChangesAsync();

        return Result.Ok(DoctorScheduleMessageCodes.CREATED);
    }

    public async Task<Result> RemoveWorkingHoursAsync(int workingHoursId)
    {
        var entity = await _context.Set<DoctorWorkingTimes>().FindAsync(workingHoursId);

        if (entity == null)
        {
            return Result.Error(DoctorScheduleMessageCodes.TIME_SLOT_NOT_FOUND);
        }

        _context.Set<DoctorWorkingTimes>().Remove(entity);
        await _context.SaveChangesAsync();

        return Result.Ok(DoctorScheduleMessageCodes.DELETED);
    }

    public async Task<Result> UpdateWorkingHoursAsync(int workingHoursId, TimeRangeDto timeRange)
    {
        if (timeRange.StartTime >= timeRange.EndTime)
        {
            return Result.Error(DoctorScheduleMessageCodes.INVALID_TIME_RANGE);
        }
        if (timeRange.StartTime.Date != timeRange.EndTime.Date)
        {
            return Result.Error(
                DoctorScheduleMessageCodes.INVALID_TIME_RANGE_IT_SHOULD_BE_IN_THE_SAME_DAY
            );
        }

        var entity = await _context.Set<DoctorWorkingTimes>().FindAsync(workingHoursId);

        if (entity == null)
        {
            return Result.Error(DoctorScheduleMessageCodes.TIME_SLOT_NOT_FOUND);
        }

        entity.StartTime = timeRange.StartTime;
        entity.EndTime = timeRange.EndTime;

        _context.Set<DoctorWorkingTimes>().Update(entity);

        await _context.SaveChangesAsync();

        return Result.Ok(DoctorScheduleMessageCodes.UPDATED);
    }

    public async Task<Result<PaginatedResult<DoctorWorkingTimesDto>>> GetWorkingHoursAsync(
        int? doctorId,
        int? clinicId,
        string language,
        string? searchKey,
        int page = 1,
        int pageSize = 10
    )
    {
        var query = _context.Set<DoctorWorkingTimes>().AsQueryable();

        if (doctorId.HasValue)
        {
            query = query.Where(x => x.DoctorClinic.DoctorId == doctorId.Value);
        }
        if (clinicId.HasValue)
        {
            query = query.Where(x => x.DoctorClinic.ClinicId == clinicId.Value);
        }
        if (!string.IsNullOrWhiteSpace(searchKey))
        {
            query = query.Where(x =>
                x.DoctorClinic.Doctor.User.FirstName.Contains(searchKey)
                || x.DoctorClinic.Doctor.User.LastName.Contains(searchKey)
            );
        }
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(x => x.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new DoctorWorkingTimesDto
            {
                Id = x.Id,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                DoctorId = x.DoctorClinic.DoctorId,
                ClinicId = x.DoctorClinic.ClinicId,
                ClinicName =
                    language == "ar" ? x.DoctorClinic.Clinic.NameAr : x.DoctorClinic.Clinic.NameEn,
                DoctorName =
                    language == "ar"
                        ? x.DoctorClinic.Doctor.DisplayNameAr
                        : x.DoctorClinic.Doctor.DisplayNameEn,
            })
            .ToListAsync();
        var result = PaginatedResult<DoctorWorkingTimesDto>.Create(
            items,
            totalCount,
            page,
            pageSize
        );

        return Result<PaginatedResult<DoctorWorkingTimesDto>>.Ok(
            DoctorScheduleMessageCodes.SUCCESS,
            result
        );
    }
}

public static class DoctorScheduleMessageCodes
{
    public const string NOT_FOUND = "DOCTOR_CLINIC_RELATIONSHIP_NOT_FOUND";
    public const string TIME_SLOT_NOT_FOUND = "TIME_SLOT_NOT_FOUND";
    public const string INVALID_TIME_RANGE = "INVALID_TIME_RANGE_START_MUST_BE_BEFORE_END";
    public const string INVALID_TIME_RANGE_IT_SHOULD_BE_IN_THE_SAME_DAY =
        "INVALID_TIME_RANGE_IT_SHOULD_BE_IN_THE_SAME_DAY";
    public const string SUCCESS = "SCHEDULE_RETRIEVED";
    public const string UPDATED = "SCHEDULE_UPDATED";
    public const string CREATED = "SCHEDULE_SLOT_CREATED";
    public const string DELETED = "SCHEDULE_SLOT_DELETED";
}
