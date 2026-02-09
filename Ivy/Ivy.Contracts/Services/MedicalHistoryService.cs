using Ivy.Contracts.Models;
using Ivy.Core.DataContext;
using Ivy.Core.Entities;
using IvyBackend;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Contracts.Services;

public interface IMedicalHistoryService
{
    Task<Result<PaginatedResult<MedicalHistoryDto>>> GetAllAsync(MedicalHistoryQueryDto query);
    Task<Result<MedicalHistoryDto>> GetByIdAsync(int id);
    Task<Result<List<MedicalHistoryDto>>> GetByPatientIdAsync(
        int patientId,
        MedicalHistoryType? type
    );
    Task<Result<MedicalHistoryDto>> CreateAsync(CreateMedicalHistoryDto dto);
    Task<Result<MedicalHistoryDto>> UpdateAsync(int id, UpdateMedicalHistoryDto dto);
    Task<Result> DeleteAsync(int id);
    Task<Result> ToggleStatusAsync(int id);
}

public class MedicalHistoryService : IMedicalHistoryService
{
    private readonly IvyContext _context;

    public MedicalHistoryService(IvyContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedResult<MedicalHistoryDto>>> GetAllAsync(
        MedicalHistoryQueryDto query
    )
    {
        try
        {
            // Validate pagination parameters
            if (query.Page < 1)
                query.Page = 1;
            if (query.PageSize < 1)
                query.PageSize = 10;
            if (query.PageSize > 100)
                query.PageSize = 100;

            var dbQuery = _context
                .Set<MedicalHistory>()
                .Include(mh => mh.MedicalHistoryItems)
                .Include(mh => mh.CreatedByUser)
                .AsQueryable();

            // Apply filters
            if (query.PatientId.HasValue)
            {
                dbQuery = dbQuery.Where(mh => mh.PatientId == query.PatientId.Value);
            }

            if (query.CreatedByUserId.HasValue)
            {
                dbQuery = dbQuery.Where(mh => mh.CreatedByUserId == query.CreatedByUserId.Value);
            }

            if (query.Type.HasValue)
            {
                dbQuery = dbQuery.Where(mh => mh.Type == query.Type.Value);
            }

            if (query.IsActive.HasValue)
            {
                dbQuery = dbQuery.Where(mh => mh.IsActive == query.IsActive.Value);
            }

            if (query.FromDate.HasValue)
            {
                dbQuery = dbQuery.Where(mh => mh.CreatedAt >= query.FromDate.Value);
            }

            if (query.ToDate.HasValue)
            {
                dbQuery = dbQuery.Where(mh => mh.CreatedAt <= query.ToDate.Value);
            }

            // Get total count
            var totalCount = await dbQuery.CountAsync();

            // Apply pagination
            var items = await dbQuery
                .OrderByDescending(mh => mh.CreatedAt)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(mh => new MedicalHistoryDto
                {
                    Id = mh.Id,
                    PatientId = mh.PatientId,
                    CreatedByUserId = mh.CreatedByUserId,
                    CreatedByUserName = $"{mh.CreatedByUser.FirstName} {mh.CreatedByUser.LastName}",
                    Type = mh.Type,
                    CreatedAt = mh.CreatedAt,
                    UpdatedAt = mh.UpdatedAt,
                    IsActive = mh.IsActive,
                    Items = mh
                        .MedicalHistoryItems.Select(item => new MedicalHistoryItemDto
                        {
                            Id = item.Id,
                            MedicalHistoryId = item.MedicalHistoryId,
                            MediaUrl = item.MediaUrl,
                            MediaType = item.MediaType,
                            CreatedAt = item.CreatedAt,
                        })
                        .ToList(),
                })
                .ToListAsync();

            var result = new PaginatedResult<MedicalHistoryDto>
            {
                Data = items,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / query.PageSize),
                HasNextPage = query.Page < (int)Math.Ceiling((double)totalCount / query.PageSize),
                HasPreviousPage = query.Page > 1,
            };

            return Result<PaginatedResult<MedicalHistoryDto>>.Ok(
                MedicalHistoryServiceMessageCodes.SUCCESS,
                result
            );
        }
        catch (Exception)
        {
            return Result<PaginatedResult<MedicalHistoryDto>>.Error(
                MedicalHistoryServiceMessageCodes.ERROR,
                null!
            );
        }
    }

    public async Task<Result<MedicalHistoryDto>> GetByIdAsync(int id)
    {
        try
        {
            var medicalHistory = await _context
                .Set<MedicalHistory>()
                .Include(mh => mh.MedicalHistoryItems)
                .Include(mh => mh.CreatedByUser)
                .FirstOrDefaultAsync(mh => mh.Id == id);

            if (medicalHistory == null)
            {
                return Result<MedicalHistoryDto>.Error(
                    MedicalHistoryServiceMessageCodes.NOT_FOUND,
                    null!
                );
            }

            var dto = new MedicalHistoryDto
            {
                Id = medicalHistory.Id,
                PatientId = medicalHistory.PatientId,
                CreatedByUserId = medicalHistory.CreatedByUserId,
                CreatedByUserName =
                    $"{medicalHistory.CreatedByUser.FirstName} {medicalHistory.CreatedByUser.LastName}",
                Type = medicalHistory.Type,
                CreatedAt = medicalHistory.CreatedAt,
                UpdatedAt = medicalHistory.UpdatedAt,
                IsActive = medicalHistory.IsActive,
                Items = medicalHistory
                    .MedicalHistoryItems.Select(item => new MedicalHistoryItemDto
                    {
                        Id = item.Id,
                        MedicalHistoryId = item.MedicalHistoryId,
                        MediaUrl = item.MediaUrl,
                        MediaType = item.MediaType,
                        CreatedAt = item.CreatedAt,
                    })
                    .ToList(),
            };

            return Result<MedicalHistoryDto>.Ok(MedicalHistoryServiceMessageCodes.SUCCESS, dto);
        }
        catch (Exception)
        {
            return Result<MedicalHistoryDto>.Error(MedicalHistoryServiceMessageCodes.ERROR, null!);
        }
    }

    public async Task<Result<List<MedicalHistoryDto>>> GetByPatientIdAsync(
        int patientId,
        MedicalHistoryType? type
    )
    {
        try
        {
            var medicalHistories = await _context
                .Set<MedicalHistory>()
                .Include(mh => mh.MedicalHistoryItems)
                .Include(mh => mh.CreatedByUser)
                .Where(mh => mh.PatientId == patientId)
                .Where(mh => type == null || mh.Type == type)
                .OrderByDescending(mh => mh.CreatedAt)
                .Select(mh => new MedicalHistoryDto
                {
                    Id = mh.Id,
                    PatientId = mh.PatientId,
                    CreatedByUserId = mh.CreatedByUserId,
                    CreatedByUserName = $"{mh.CreatedByUser.FirstName} {mh.CreatedByUser.LastName}",
                    Type = mh.Type,
                    CreatedAt = mh.CreatedAt,
                    UpdatedAt = mh.UpdatedAt,
                    IsActive = mh.IsActive,
                    Items = mh
                        .MedicalHistoryItems.Select(item => new MedicalHistoryItemDto
                        {
                            Id = item.Id,
                            MedicalHistoryId = item.MedicalHistoryId,
                            MediaUrl = item.MediaUrl,
                            MediaType = item.MediaType,
                            CreatedAt = item.CreatedAt,
                        })
                        .ToList(),
                })
                .ToListAsync();

            return Result<List<MedicalHistoryDto>>.Ok(
                MedicalHistoryServiceMessageCodes.SUCCESS,
                medicalHistories
            );
        }
        catch (Exception)
        {
            return Result<List<MedicalHistoryDto>>.Error(
                MedicalHistoryServiceMessageCodes.ERROR,
                null!
            );
        }
    }

    public async Task<Result<MedicalHistoryDto>> CreateAsync(CreateMedicalHistoryDto dto)
    {
        try
        {
            // Validate patient exists
            var patientExists = await _context
                .Set<Patient>()
                .AnyAsync(p => p.UserId == dto.PatientId);

            if (!patientExists)
            {
                return Result<MedicalHistoryDto>.Error(
                    MedicalHistoryServiceMessageCodes.PATIENT_NOT_FOUND,
                    null!
                );
            }

            // Validate creator user exists
            var userExists = await _context.Set<User>().AnyAsync(u => u.Id == dto.CreatedByUserId);

            if (!userExists)
            {
                return Result<MedicalHistoryDto>.Error(
                    MedicalHistoryServiceMessageCodes.USER_NOT_FOUND,
                    null!
                );
            }

            var medicalHistory = new MedicalHistory
            {
                PatientId = dto.PatientId,
                CreatedByUserId = dto.CreatedByUserId,
                Type = dto.Type,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false,
                MedicalHistoryItems = dto
                    .Items.Select(item => new MedicalHistoryItem
                    {
                        MediaUrl = item.MediaUrl,
                        MediaType = item.MediaType,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true,
                        IsDeleted = false,
                    })
                    .ToList(),
            };

            _context.Set<MedicalHistory>().Add(medicalHistory);
            await _context.SaveChangesAsync();

            // Reload with includes
            var createdHistory = await _context
                .Set<MedicalHistory>()
                .Include(mh => mh.MedicalHistoryItems)
                .Include(mh => mh.CreatedByUser)
                .FirstAsync(mh => mh.Id == medicalHistory.Id);

            var resultDto = new MedicalHistoryDto
            {
                Id = createdHistory.Id,
                PatientId = createdHistory.PatientId,
                CreatedByUserId = createdHistory.CreatedByUserId,
                CreatedByUserName =
                    $"{createdHistory.CreatedByUser.FirstName} {createdHistory.CreatedByUser.LastName}",
                Type = createdHistory.Type,
                CreatedAt = createdHistory.CreatedAt,
                UpdatedAt = createdHistory.UpdatedAt,
                IsActive = createdHistory.IsActive,
                Items = createdHistory
                    .MedicalHistoryItems.Select(item => new MedicalHistoryItemDto
                    {
                        Id = item.Id,
                        MedicalHistoryId = item.MedicalHistoryId,
                        MediaUrl = item.MediaUrl,
                        MediaType = item.MediaType,
                        CreatedAt = item.CreatedAt,
                    })
                    .ToList(),
            };

            return Result<MedicalHistoryDto>.Ok(
                MedicalHistoryServiceMessageCodes.CREATED,
                resultDto
            );
        }
        catch (Exception)
        {
            return Result<MedicalHistoryDto>.Error(MedicalHistoryServiceMessageCodes.ERROR, null!);
        }
    }

    public async Task<Result<MedicalHistoryDto>> UpdateAsync(int id, UpdateMedicalHistoryDto dto)
    {
        try
        {
            var medicalHistory = await _context
                .Set<MedicalHistory>()
                .Include(mh => mh.MedicalHistoryItems)
                .Include(mh => mh.CreatedByUser)
                .FirstOrDefaultAsync(mh => mh.Id == id);

            if (medicalHistory == null)
            {
                return Result<MedicalHistoryDto>.Error(
                    MedicalHistoryServiceMessageCodes.NOT_FOUND,
                    null!
                );
            }

            // Update basic properties
            medicalHistory.Type = dto.Type;
            medicalHistory.IsActive = dto.IsActive;
            medicalHistory.UpdatedAt = DateTime.UtcNow;

            // Handle items update
            // Remove items that are not in the update list
            var existingItemIds = dto
                .Items.Where(i => i.Id.HasValue)
                .Select(i => i.Id!.Value)
                .ToList();

            var itemsToRemove = medicalHistory
                .MedicalHistoryItems.Where(item => !existingItemIds.Contains(item.Id))
                .ToList();

            foreach (var item in itemsToRemove)
            {
                item.IsDeleted = true;
                item.DeletedAt = DateTime.UtcNow;
            }

            // Update existing items and add new ones
            foreach (var itemDto in dto.Items)
            {
                if (itemDto.Id.HasValue)
                {
                    // Update existing item
                    var existingItem = medicalHistory.MedicalHistoryItems.FirstOrDefault(i =>
                        i.Id == itemDto.Id.Value
                    );

                    if (existingItem != null)
                    {
                        existingItem.MediaUrl = itemDto.MediaUrl;
                        existingItem.MediaType = itemDto.MediaType;
                        existingItem.UpdatedAt = DateTime.UtcNow;
                    }
                }
                else
                {
                    // Add new item
                    var newItem = new MedicalHistoryItem
                    {
                        MedicalHistoryId = medicalHistory.Id,
                        MediaUrl = itemDto.MediaUrl,
                        MediaType = itemDto.MediaType,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true,
                        IsDeleted = false,
                    };
                    medicalHistory.MedicalHistoryItems.Add(newItem);
                }
            }

            await _context.SaveChangesAsync();

            // Reload to get updated data
            var updatedHistory = await _context
                .Set<MedicalHistory>()
                .Include(mh => mh.MedicalHistoryItems.Where(i => !i.IsDeleted))
                .Include(mh => mh.CreatedByUser)
                .FirstAsync(mh => mh.Id == id);

            var resultDto = new MedicalHistoryDto
            {
                Id = updatedHistory.Id,
                PatientId = updatedHistory.PatientId,
                CreatedByUserId = updatedHistory.CreatedByUserId,
                CreatedByUserName =
                    $"{updatedHistory.CreatedByUser.FirstName} {updatedHistory.CreatedByUser.LastName}",
                Type = updatedHistory.Type,
                CreatedAt = updatedHistory.CreatedAt,
                UpdatedAt = updatedHistory.UpdatedAt,
                IsActive = updatedHistory.IsActive,
                Items = updatedHistory
                    .MedicalHistoryItems.Select(item => new MedicalHistoryItemDto
                    {
                        Id = item.Id,
                        MedicalHistoryId = item.MedicalHistoryId,
                        MediaUrl = item.MediaUrl,
                        MediaType = item.MediaType,
                        CreatedAt = item.CreatedAt,
                    })
                    .ToList(),
            };

            return Result<MedicalHistoryDto>.Ok(
                MedicalHistoryServiceMessageCodes.UPDATED,
                resultDto
            );
        }
        catch (Exception)
        {
            return Result<MedicalHistoryDto>.Error(MedicalHistoryServiceMessageCodes.ERROR, null!);
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            var medicalHistory = await _context
                .Set<MedicalHistory>()
                .Include(mh => mh.MedicalHistoryItems)
                .FirstOrDefaultAsync(mh => mh.Id == id);

            if (medicalHistory == null)
            {
                return Result.Error(MedicalHistoryServiceMessageCodes.NOT_FOUND);
            }

            // Soft delete
            medicalHistory.IsDeleted = true;
            medicalHistory.DeletedAt = DateTime.UtcNow;
            medicalHistory.UpdatedAt = DateTime.UtcNow;

            // Soft delete all items
            foreach (var item in medicalHistory.MedicalHistoryItems)
            {
                item.IsDeleted = true;
                item.DeletedAt = DateTime.UtcNow;
                item.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Result.Ok(MedicalHistoryServiceMessageCodes.DELETED);
        }
        catch (Exception)
        {
            return Result.Error(MedicalHistoryServiceMessageCodes.ERROR);
        }
    }

    public async Task<Result> ToggleStatusAsync(int id)
    {
        try
        {
            var medicalHistory = await _context
                .Set<MedicalHistory>()
                .FirstOrDefaultAsync(mh => mh.Id == id);

            if (medicalHistory == null)
            {
                return Result.Error(MedicalHistoryServiceMessageCodes.NOT_FOUND);
            }

            medicalHistory.IsActive = !medicalHistory.IsActive;
            medicalHistory.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result.Ok(MedicalHistoryServiceMessageCodes.STATUS_UPDATED);
        }
        catch (Exception)
        {
            return Result.Error(MedicalHistoryServiceMessageCodes.ERROR);
        }
    }
}

public static class MedicalHistoryServiceMessageCodes
{
    public const string NOT_FOUND = "MEDICAL_HISTORY_NOT_FOUND";
    public const string PATIENT_NOT_FOUND = "MEDICAL_HISTORY_PATIENT_NOT_FOUND";
    public const string USER_NOT_FOUND = "MEDICAL_HISTORY_USER_NOT_FOUND";
    public const string CREATED = "MEDICAL_HISTORY_CREATED";
    public const string UPDATED = "MEDICAL_HISTORY_UPDATED";
    public const string DELETED = "MEDICAL_HISTORY_DELETED";
    public const string SUCCESS = "MEDICAL_HISTORY_SUCCESS";
    public const string STATUS_UPDATED = "MEDICAL_HISTORY_STATUS_UPDATED";
    public const string ERROR = "MEDICAL_HISTORY_ERROR";
}
