using Ivy.Core.DataContext;
using Ivy.Core.Entities;
using IvyBackend;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Core.Services;

public class ClinicService : IClinicService
{
    private readonly IvyContext _context;

    public ClinicService(IvyContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedResult<Clinic>>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? nameAr = null,
        string? nameEn = null,
        string? descriptionAr = null,
        string? descriptionEn = null,
        int? locationId = null,
        bool? isActive = null
    )
    {
        try
        {
            // Validate pagination parameters
            if (page < 1)
                page = 1;
            if (pageSize < 1)
                pageSize = 10;
            if (pageSize > 100)
                pageSize = 100; // Limit maximum page size

            var query = _context
                .Clinics.Include(c => c.Location)
                .Include(c => c.ClinicImages)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(nameAr))
            {
                query = query.Where(c => c.NameAr.Contains(nameAr));
            }

            if (!string.IsNullOrWhiteSpace(nameEn))
            {
                query = query.Where(c => c.NameEn.Contains(nameEn));
            }

            if (!string.IsNullOrWhiteSpace(descriptionAr))
            {
                query = query.Where(c => c.DescriptionAr.Contains(descriptionAr));
            }

            if (!string.IsNullOrWhiteSpace(descriptionEn))
            {
                query = query.Where(c => c.DescriptionEn.Contains(descriptionEn));
            }

            if (locationId.HasValue)
            {
                query = query.Where(c => c.LocationId == locationId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination and ordering
            var clinics = await query
                .OrderBy(c => c.NameEn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var paginatedResult = PaginatedResult<Clinic>.Create(
                clinics,
                totalCount,
                page,
                pageSize
            );
            return Result<PaginatedResult<Clinic>>.Ok("CLINICS_RETRIEVED_SUCCESS", paginatedResult);
        }
        catch (Exception)
        {
            var emptyResult = new PaginatedResult<Clinic>();
            return Result<PaginatedResult<Clinic>>.Error("CLINICS_RETRIEVAL_FAILED", emptyResult);
        }
    }

    public async Task<Result<Clinic>> GetByIdAsync(int id)
    {
        try
        {
            var clinic = await _context
                .Clinics.Include(c => c.Location)
                .Include(c => c.ClinicImages)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (clinic == null)
            {
                return Result<Clinic>.Error("CLINIC_NOT_FOUND", null!);
            }

            return Result<Clinic>.Ok("CLINIC_RETRIEVED_SUCCESS", clinic);
        }
        catch (Exception)
        {
            return Result<Clinic>.Error("CLINIC_RETRIEVAL_FAILED", null!);
        }
    }

    public async Task<Result<Clinic>> CreateAsync(Clinic clinic)
    {
        try
        {
            // Check if location exists
            var locationExists = await _context.Locations.AnyAsync(l => l.Id == clinic.LocationId);
            if (!locationExists)
            {
                return Result<Clinic>.Error("LOCATION_NOT_FOUND", null!);
            }

            // Check for duplicate name within the same location
            var duplicateNameExists = await _context.Clinics.AnyAsync(c =>
                (c.NameAr == clinic.NameAr || c.NameEn == clinic.NameEn)
                && c.LocationId == clinic.LocationId
            );

            if (duplicateNameExists)
            {
                return Result<Clinic>.Error("CLINIC_NAME_ALREADY_EXISTS", null!);
            }

            // Check for duplicate email
            var duplicateEmailExists = await _context.Clinics.AnyAsync(c =>
                c.ContactEmail == clinic.ContactEmail
            );

            if (duplicateEmailExists)
            {
                return Result<Clinic>.Error("CLINIC_EMAIL_ALREADY_EXISTS", null!);
            }

            clinic.CreatedAt = DateTime.UtcNow;
            clinic.UpdatedAt = DateTime.UtcNow;

            _context.Clinics.Add(clinic);
            await _context.SaveChangesAsync();

            // Load the created clinic with its location and images
            var createdClinic = await _context
                .Clinics.Include(c => c.Location)
                .Include(c => c.ClinicImages)
                .FirstAsync(c => c.Id == clinic.Id);

            return Result<Clinic>.Ok("CLINIC_CREATED_SUCCESS", createdClinic);
        }
        catch (Exception)
        {
            return Result<Clinic>.Error("CLINIC_CREATION_FAILED", null!);
        }
    }

    public async Task<Result<Clinic>> UpdateAsync(int id, Clinic clinic)
    {
        try
        {
            var existingClinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == id);
            if (existingClinic == null)
            {
                return Result<Clinic>.Error("CLINIC_NOT_FOUND", null!);
            }

            // Check if location exists
            var locationExists = await _context.Locations.AnyAsync(l => l.Id == clinic.LocationId);
            if (!locationExists)
            {
                return Result<Clinic>.Error("LOCATION_NOT_FOUND", null!);
            }

            // Check for duplicate name within the same location (excluding current clinic)
            var duplicateNameExists = await _context.Clinics.AnyAsync(c =>
                c.Id != id
                && (c.NameAr == clinic.NameAr || c.NameEn == clinic.NameEn)
                && c.LocationId == clinic.LocationId
            );

            if (duplicateNameExists)
            {
                return Result<Clinic>.Error("CLINIC_NAME_ALREADY_EXISTS", null!);
            }

            // Check for duplicate email (excluding current clinic)
            var duplicateEmailExists = await _context.Clinics.AnyAsync(c =>
                c.Id != id && c.ContactEmail == clinic.ContactEmail
            );

            if (duplicateEmailExists)
            {
                return Result<Clinic>.Error("CLINIC_EMAIL_ALREADY_EXISTS", null!);
            }

            existingClinic.NameAr = clinic.NameAr;
            existingClinic.NameEn = clinic.NameEn;
            existingClinic.DescriptionAr = clinic.DescriptionAr;
            existingClinic.DescriptionEn = clinic.DescriptionEn;
            existingClinic.ContactPhoneNumber = clinic.ContactPhoneNumber;
            existingClinic.ContactEmail = clinic.ContactEmail;
            existingClinic.LocationId = clinic.LocationId;
            existingClinic.IsActive = clinic.IsActive;
            existingClinic.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Load the updated clinic with its location and images
            var updatedClinic = await _context
                .Clinics.Include(c => c.Location)
                .Include(c => c.ClinicImages)
                .FirstAsync(c => c.Id == id);

            return Result<Clinic>.Ok("CLINIC_UPDATED_SUCCESS", updatedClinic);
        }
        catch (Exception)
        {
            return Result<Clinic>.Error("CLINIC_UPDATE_FAILED", null!);
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            var clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == id);
            if (clinic == null)
            {
                return Result.Error("CLINIC_NOT_FOUND");
            }

            // Soft delete
            clinic.IsDeleted = true;
            clinic.DeletedAt = DateTime.UtcNow;
            clinic.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result.Ok("CLINIC_DELETED_SUCCESS");
        }
        catch (Exception)
        {
            return Result.Error("CLINIC_DELETION_FAILED");
        }
    }

    public async Task<Result<IEnumerable<Clinic>>> GetByLocationIdAsync(int locationId)
    {
        try
        {
            var clinics = await _context
                .Clinics.Include(c => c.Location)
                .Include(c => c.ClinicImages)
                .Where(c => c.LocationId == locationId)
                .OrderBy(c => c.NameEn)
                .ToListAsync();

            return Result<IEnumerable<Clinic>>.Ok("CLINICS_RETRIEVED_SUCCESS", clinics);
        }
        catch (Exception)
        {
            return Result<IEnumerable<Clinic>>.Error(
                "CLINICS_RETRIEVAL_FAILED",
                Enumerable.Empty<Clinic>()
            );
        }
    }

    public async Task<Result<bool>> ExistsAsync(int id)
    {
        try
        {
            var exists = await _context.Clinics.AnyAsync(c => c.Id == id);
            return Result<bool>.Ok("CLINIC_EXISTS_CHECK_SUCCESS", exists);
        }
        catch (Exception)
        {
            return Result<bool>.Error("CLINIC_EXISTS_CHECK_FAILED", false);
        }
    }

    public async Task<Result<bool>> ExistsAsync(
        string nameAr,
        string nameEn,
        int locationId,
        int? excludeId = null
    )
    {
        try
        {
            var query = _context.Clinics.AsQueryable();

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            var exists = await query.AnyAsync(c =>
                (c.NameAr == nameAr || c.NameEn == nameEn) && c.LocationId == locationId
            );

            return Result<bool>.Ok("CLINIC_EXISTS_CHECK_SUCCESS", exists);
        }
        catch (Exception)
        {
            return Result<bool>.Error("CLINIC_EXISTS_CHECK_FAILED", false);
        }
    }

    public async Task<Result<bool>> EmailExistsAsync(string email, int? excludeId = null)
    {
        try
        {
            var query = _context.Clinics.AsQueryable();

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            var exists = await query.AnyAsync(c => c.ContactEmail == email);

            return Result<bool>.Ok("CLINIC_EMAIL_EXISTS_CHECK_SUCCESS", exists);
        }
        catch (Exception)
        {
            return Result<bool>.Error("CLINIC_EMAIL_EXISTS_CHECK_FAILED", false);
        }
    }

    public async Task<Result> DeactivateAsync(int id)
    {
        try
        {
            var clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == id);
            if (clinic == null)
            {
                return Result.Error("CLINIC_NOT_FOUND");
            }

            clinic.IsActive = false;
            clinic.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result.Ok("CLINIC_DEACTIVATED_SUCCESS");
        }
        catch (Exception)
        {
            return Result.Error("CLINIC_DEACTIVATION_FAILED");
        }
    }

    public async Task<Result> ActivateAsync(int id)
    {
        try
        {
            var clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == id);
            if (clinic == null)
            {
                return Result.Error("CLINIC_NOT_FOUND");
            }

            clinic.IsActive = true;
            clinic.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result.Ok("CLINIC_ACTIVATED_SUCCESS");
        }
        catch (Exception)
        {
            return Result.Error("CLINIC_ACTIVATION_FAILED");
        }
    }

    public async Task<Result<DoctorClinic>> AddDoctorToClinicAsync(int clinicId, int doctorId)
    {
        try
        {
            // Check if clinic exists and is active
            var clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == clinicId && c.IsActive);
            if (clinic == null)
            {
                return Result<DoctorClinic>.Error("CLINIC_NOT_FOUND_OR_INACTIVE", null!);
            }

            // Check if doctor exists
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == doctorId);
            if (doctor == null)
            {
                return Result<DoctorClinic>.Error("DOCTOR_NOT_FOUND", null!);
            }

            // Check if the relationship already exists and is active
            var existingRelation = await _context.DoctorClinics
                .FirstOrDefaultAsync(dc => dc.ClinicId == clinicId && dc.DoctorId == doctorId);

            if (existingRelation != null && !existingRelation.IsDeleted)
            {
                return Result<DoctorClinic>.Error("DOCTOR_ALREADY_ASSIGNED_TO_CLINIC", null!);
            }

            // If relationship exists but was soft deleted, reactivate it
            if (existingRelation != null && existingRelation.IsDeleted)
            {
                existingRelation.IsDeleted = false;
                existingRelation.DeletedAt = null;
                existingRelation.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Load the reactivated relationship with its related entities
                var reactivatedRelation = await _context.DoctorClinics
                    .Include(dc => dc.Doctor)
                    .ThenInclude(d => d.User)
                    .Include(dc => dc.Clinic)
                    .FirstAsync(dc => dc.Id == existingRelation.Id);

                return Result<DoctorClinic>.Ok("DOCTOR_CLINIC_RELATIONSHIP_REACTIVATED", reactivatedRelation);
            }

            // Create new relationship
            var doctorClinic = new DoctorClinic
            {
                ClinicId = clinicId,
                DoctorId = doctorId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.DoctorClinics.Add(doctorClinic);
            await _context.SaveChangesAsync();

            // Load the created relationship with its related entities
            var createdRelation = await _context.DoctorClinics
                .Include(dc => dc.Doctor)
                .ThenInclude(d => d.User)
                .Include(dc => dc.Clinic)
                .FirstAsync(dc => dc.Id == doctorClinic.Id);

            return Result<DoctorClinic>.Ok("DOCTOR_ADDED_TO_CLINIC_SUCCESS", createdRelation);
        }
        catch (Exception)
        {
            return Result<DoctorClinic>.Error("DOCTOR_CLINIC_ASSIGNMENT_FAILED", null!);
        }
    }

    public async Task<Result> RemoveDoctorFromClinicAsync(int clinicId, int doctorId)
    {
        try
        {
            // Find the active relationship
            var doctorClinic = await _context.DoctorClinics
                .FirstOrDefaultAsync(dc => dc.ClinicId == clinicId && dc.DoctorId == doctorId && !dc.IsDeleted);

            if (doctorClinic == null)
            {
                return Result.Error("DOCTOR_CLINIC_RELATIONSHIP_NOT_FOUND");
            }

            // Soft delete the relationship
            doctorClinic.IsDeleted = true;
            doctorClinic.DeletedAt = DateTime.UtcNow;
            doctorClinic.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result.Ok("DOCTOR_REMOVED_FROM_CLINIC_SUCCESS");
        }
        catch (Exception)
        {
            return Result.Error("DOCTOR_CLINIC_REMOVAL_FAILED");
        }
    }

    public async Task<Result<IEnumerable<DoctorClinic>>> GetClinicDoctorsAsync(int clinicId)
    {
        try
        {
            // Check if clinic exists
            var clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == clinicId);
            if (clinic == null)
            {
                return Result<IEnumerable<DoctorClinic>>.Error("CLINIC_NOT_FOUND", Enumerable.Empty<DoctorClinic>());
            }

            var doctorClinics = await _context.DoctorClinics
                .Include(dc => dc.Doctor)
                .ThenInclude(d => d.User)
                .Include(dc => dc.Clinic)
                .Where(dc => dc.ClinicId == clinicId && !dc.IsDeleted)
                .OrderBy(dc => dc.Doctor.User.FirstName)
                .ToListAsync();

            return Result<IEnumerable<DoctorClinic>>.Ok("CLINIC_DOCTORS_RETRIEVED_SUCCESS", doctorClinics);
        }
        catch (Exception)
        {
            return Result<IEnumerable<DoctorClinic>>.Error("CLINIC_DOCTORS_RETRIEVAL_FAILED", Enumerable.Empty<DoctorClinic>());
        }
    }
}
