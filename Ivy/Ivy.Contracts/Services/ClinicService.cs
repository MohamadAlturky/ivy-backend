using Ivy.Contracts.Models;
using Ivy.Core.DataContext;
using Ivy.Core.Entities;
using IvyBackend;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Contracts.Services;

public interface IClinicService
{
    Task<Result<PaginatedResult<ClinicDto>>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? name = null,
        bool? isActive = null
    );
    Task<Result<PaginatedResult<ClinicLocalizedDto>>> GetAllLocalizedAsync(
        string language = "en",
        int page = 1,
        int pageSize = 10,
        string? name = null,
        bool? isActive = null
    );
    Task<Result<List<ClinicDropDownDto>>> DropDownAsync(
        string language = "en",
        string? name = null
    );
    Task<Result<ClinicDto>> CreateAsync(CreateClinicDto dto);
    Task<Result<ClinicDto>> UpdateAsync(int id, UpdateClinicDto dto);
    Task<Result> DeleteAsync(int id);
    Task<Result> ToggleStatusAsync(int id);
}

public class ClinicService : IClinicService
{
    private readonly IvyContext _context;

    public ClinicService(IvyContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedResult<ClinicDto>>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? name = null,
        bool? isActive = null
    )
    {
        var query = _context.Set<Clinic>().AsQueryable();

        // Filtering
        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(x => x.NameAr.Contains(name) || x.NameEn.Contains(name));
        }

        // Pagination
        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ClinicDto
            {
                Id = x.Id,
                NameAr = x.NameAr,
                NameEn = x.NameEn,
                DescriptionAr = x.DescriptionAr,
                DescriptionEn = x.DescriptionEn,
                Location = new LocationDto
                {
                    Id = x.LocationId,
                    NameAr = x.Location.NameAr,
                    NameEn = x.Location.NameEn,
                    Latitude = x.Location.Latitude,
                    Longitude = x.Location.Longitude,
                    City = new CityDto
                    {
                        Id = x.Location.CityId,
                        NameAr = x.Location.City.NameAr,
                        NameEn = x.Location.City.NameEn,
                        Governorate = new GovernorateDto
                        {
                            Id = x.Location.City.GovernorateId,
                            NameAr = x.Location.City.Governorate.NameAr,
                            NameEn = x.Location.City.Governorate.NameEn,
                            IsActive = x.Location.City.Governorate.IsActive,
                        },
                        IsActive = x.Location.City.IsActive,
                    },
                },
                IsActive = x.IsActive,
            })
            .ToListAsync();

        var result = PaginatedResult<ClinicDto>.Create(items, totalCount, page, pageSize);

        return Result<PaginatedResult<ClinicDto>>.Ok(ClinicServiceMessageCodes.SUCCESS, result);
    }

    public async Task<Result<ClinicDto>> CreateAsync(CreateClinicDto dto)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(dto.NameAr) || string.IsNullOrWhiteSpace(dto.NameEn))
        {
            return Result<ClinicDto>.Error(ClinicServiceMessageCodes.INVALID_DATA, null!);
        }
        var entity = new Clinic
        {
            NameAr = dto.NameAr,
            NameEn = dto.NameEn,
            DescriptionAr = dto.DescriptionAr,
            DescriptionEn = dto.DescriptionEn,
            Location = new Location
            {
                NameAr = dto.Location.NameAr,
                NameEn = dto.Location.NameEn,
                Latitude = dto.Location.Latitude,
                Longitude = dto.Location.Longitude,
                CityId = dto.Location.CityId,
            },
            IsActive = dto.IsActive,
        };

        await _context.Set<Clinic>().AddAsync(entity);
        await _context.SaveChangesAsync();

        // Load the created clinic with its location and city
        var createdClinic = await _context
            .Set<Clinic>()
            .Include(x => x.Location)
            .ThenInclude(x => x.City)
            .ThenInclude(x => x.Governorate)
            .FirstOrDefaultAsync(x => x.Id == entity.Id);

        if (createdClinic == null)
        {
            return Result<ClinicDto>.Error(ClinicServiceMessageCodes.NOT_FOUND, null!);
        }

        // Map back to DTO
        var resultDto = new ClinicDto
        {
            Id = createdClinic.Id,
            NameAr = createdClinic.NameAr,
            NameEn = createdClinic.NameEn,
            DescriptionAr = createdClinic.DescriptionAr,
            DescriptionEn = createdClinic.DescriptionEn,
            Location = new LocationDto
            {
                Id = createdClinic.LocationId,
                NameAr = createdClinic.Location.NameAr,
                NameEn = createdClinic.Location.NameEn,
                Latitude = createdClinic.Location.Latitude,
                Longitude = createdClinic.Location.Longitude,
                City = new CityDto
                {
                    Id = createdClinic.Location.CityId,
                    NameAr = createdClinic.Location.City.NameAr,
                    NameEn = createdClinic.Location.City.NameEn,
                    Governorate = new GovernorateDto
                    {
                        Id = createdClinic.Location.City.GovernorateId,
                        NameAr = createdClinic.Location.City.Governorate.NameAr,
                        NameEn = createdClinic.Location.City.Governorate.NameEn,
                        IsActive = createdClinic.Location.City.Governorate.IsActive,
                    },
                    IsActive = createdClinic.Location.City.IsActive,
                },
            },
            IsActive = entity.IsActive,
        };

        return Result<ClinicDto>.Ok(ClinicServiceMessageCodes.CREATED, resultDto);
    }

    public async Task<Result<ClinicDto>> UpdateAsync(int id, UpdateClinicDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NameAr) || string.IsNullOrWhiteSpace(dto.NameEn))
        {
            return Result<ClinicDto>.Error(ClinicServiceMessageCodes.INVALID_DATA, null!);
        }

        var entity = await _context.Set<Clinic>().FindAsync(id);

        if (entity == null)
        {
            return Result<ClinicDto>.Error(ClinicServiceMessageCodes.NOT_FOUND, null!);
        }

        // Update fields
        entity.NameAr = dto.NameAr;
        entity.NameEn = dto.NameEn;
        entity.DescriptionAr = dto.DescriptionAr;
        entity.DescriptionEn = dto.DescriptionEn;
        if (dto.Location != null)
        {
            entity.Location = new Location
            {
                NameAr = dto.Location.NameAr,
                NameEn = dto.Location.NameEn,
                Latitude = dto.Location.Latitude,
                Longitude = dto.Location.Longitude,
                CityId = dto.Location.CityId,
            };
        }
        entity.IsActive = dto.IsActive;

        _context.Set<Clinic>().Update(entity);
        await _context.SaveChangesAsync();

        // Load the created clinic with its location and city
        var updatedClinic = await _context
            .Set<Clinic>()
            .Include(x => x.Location)
            .ThenInclude(x => x.City)
            .ThenInclude(x => x.Governorate)
            .FirstOrDefaultAsync(x => x.Id == entity.Id);

        if (updatedClinic == null)
        {
            return Result<ClinicDto>.Error(ClinicServiceMessageCodes.NOT_FOUND, null!);
        }

        // Map back to DTO
        var resultDto = new ClinicDto
        {
            Id = updatedClinic.Id,
            NameAr = updatedClinic.NameAr,
            NameEn = updatedClinic.NameEn,
            DescriptionAr = updatedClinic.DescriptionAr,
            DescriptionEn = updatedClinic.DescriptionEn,
            Location = new LocationDto
            {
                Id = updatedClinic.LocationId,
                NameAr = updatedClinic.Location.NameAr,
                NameEn = updatedClinic.Location.NameEn,
                Latitude = updatedClinic.Location.Latitude,
                Longitude = updatedClinic.Location.Longitude,
                City = new CityDto
                {
                    Id = updatedClinic.Location.CityId,
                    NameAr = updatedClinic.Location.City.NameAr,
                    NameEn = updatedClinic.Location.City.NameEn,
                    Governorate = new GovernorateDto
                    {
                        Id = updatedClinic.Location.City.GovernorateId,
                        NameAr = updatedClinic.Location.City.Governorate.NameAr,
                        NameEn = updatedClinic.Location.City.Governorate.NameEn,
                        IsActive = updatedClinic.Location.City.Governorate.IsActive,
                    },
                    IsActive = updatedClinic.Location.City.IsActive,
                },
            },
            IsActive = entity.IsActive,
        };

        return Result<ClinicDto>.Ok(ClinicServiceMessageCodes.UPDATED, resultDto);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var entity = await _context.Set<Clinic>().FindAsync(id);

        if (entity == null)
        {
            return Result.Error(ClinicServiceMessageCodes.NOT_FOUND);
        }

        // Hard delete
        _context.Set<Clinic>().Remove(entity);
        await _context.SaveChangesAsync();

        return Result.Ok(ClinicServiceMessageCodes.DELETED);
    }

    public async Task<Result<PaginatedResult<ClinicLocalizedDto>>> GetAllLocalizedAsync(
        string language = "en",
        int page = 1,
        int pageSize = 10,
        string? name = null,
        bool? isActive = null
    )
    {
        var query = _context.Set<Clinic>().AsQueryable();

        // Filtering
        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(x => x.NameAr.Contains(name) || x.NameEn.Contains(name));
        }

        // Pagination
        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ClinicLocalizedDto
            {
                Id = x.Id,
                Name = language == "ar" ? x.NameAr : x.NameEn,
                Description = language == "ar" ? x.DescriptionAr : x.DescriptionEn,
                Location = new LocationLocalizedDto
                {
                    Id = x.LocationId,
                    Name = language == "ar" ? x.Location.NameAr : x.Location.NameEn,
                    Latitude = x.Location.Latitude,
                    Longitude = x.Location.Longitude,
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
                IsActive = x.IsActive,
            })
            .ToListAsync();

        var result = PaginatedResult<ClinicLocalizedDto>.Create(items, totalCount, page, pageSize);

        return Result<PaginatedResult<ClinicLocalizedDto>>.Ok(
            ClinicServiceMessageCodes.SUCCESS,
            result
        );
    }

    public async Task<Result<List<ClinicDropDownDto>>> DropDownAsync(
        string language = "en",
        string? name = null
    )
    {
        var query = _context.Set<Clinic>().AsQueryable();

        // Filtering
        query = query.Where(x => x.IsActive == true);

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(x => x.NameAr.Contains(name) || x.NameEn.Contains(name));
        }

        var items = await query
            .OrderBy(x => x.Id)
            .Select(x => new ClinicDropDownDto
            {
                Id = x.Id,
                Name = language == "ar" ? x.NameAr : x.NameEn,
            })
            .ToListAsync();

        return Result<List<ClinicDropDownDto>>.Ok(ClinicServiceMessageCodes.SUCCESS, items);
    }

    public async Task<Result> ToggleStatusAsync(int id)
    {
        var entity = await _context.Set<Clinic>().FindAsync(id);

        if (entity == null)
        {
            return Result.Error(ClinicServiceMessageCodes.NOT_FOUND);
        }

        entity.IsActive = !entity.IsActive;
        await _context.SaveChangesAsync();

        return Result.Ok(ClinicServiceMessageCodes.STATUS_UPDATED);
    }
}

public static class ClinicServiceMessageCodes
{
    public const string INVALID_DATA = "CLINIC_INVALID_DATA";
    public const string DUPLICATE_NAME = "CLINIC_DUPLICATE_NAME";
    public const string NOT_FOUND = "CLINIC_NOT_FOUND";
    public const string LOCATION_NOT_FOUND = "CLINIC_LOCATION_NOT_FOUND";
    public const string CREATED = "CLINIC_CREATED";
    public const string UPDATED = "CLINIC_UPDATED";
    public const string DELETED = "CLINIC_DELETED";
    public const string SUCCESS = "CLINIC_SUCCESS";
    public const string STATUS_UPDATED = "CLINIC_STATUS_UPDATED";
    public const string CITY_NOT_FOUND = "CLINIC_CITY_NOT_FOUND";
}
