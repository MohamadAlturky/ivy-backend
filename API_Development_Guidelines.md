# API Development Guidelines for Ivy Backend

## Table of Contents

1. [Overview](#overview)
2. [Architecture Pattern](#architecture-pattern)
3. [Project Structure](#project-structure)
4. [Step-by-Step Guide](#step-by-step-guide)
5. [Entity Layer](#entity-layer)
6. [Service Layer](#service-layer)
7. [Controller Layer](#controller-layer)
8. [DTO Layer](#dto-layer)
9. [Error Handling](#error-handling)
10. [Naming Conventions](#naming-conventions)
11. [Best Practices](#best-practices)
12. [Code Templates](#code-templates)

## Overview

This document provides comprehensive guidelines for creating new APIs in the Ivy Backend project, following the established conventions and patterns demonstrated in the `GovernorateController`.

## Architecture Pattern

The Ivy Backend follows a **Clean Architecture** pattern with clear separation of concerns:

- **API Layer** (`Ivy.Api`): Controllers, DTOs, Response handling
- **Core Layer** (`Ivy.Core`): Entities, Services, Business logic, Data context
- **Cross-cutting**: Result pattern, Error handling, Logging

### Key Patterns Used

- **Repository Pattern**: Implicit through Entity Framework DbContext
- **Service Pattern**: Business logic encapsulation
- **Result Pattern**: Consistent success/failure handling
- **DTO Pattern**: Data transfer and validation
- **Dependency Injection**: IoC container for loose coupling

## Project Structure

```
Ivy/
├── Ivy.Api/                    # Web API Layer
│   ├── Controllers/           # API Controllers
│   ├── DTOs/                  # Data Transfer Objects
│   ├── Services/              # API-specific services
│   └── Program.cs             # Application entry point
└── Ivy.Core/                  # Core Business Layer
    ├── Entities/              # Domain entities
    ├── Services/              # Business logic services
    ├── DataContext/           # Database context
    ├── Configurations/        # Entity configurations
    └── Utils/                 # Utility classes
```

## Step-by-Step Guide

### 1. Define Your Entity (Core Layer)

First, create your domain entity in `Ivy.Core/Entities/`:

```csharp
namespace Ivy.Core.Entities;

public class YourEntity : BaseEntity<int>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    // Add your specific properties
    
    // Navigation properties
    public ICollection<RelatedEntity>? RelatedEntities { get; set; }
}
```

### 2. Configure Entity (Core Layer)

Create entity configuration in `Ivy.Core/Configurations/`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ivy.Core.Configurations;

public class YourEntityConfiguration : IEntityTypeConfiguration<YourEntity>
{
    public void Configure(EntityTypeBuilder<YourEntity> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(e => e.Description)
            .HasMaxLength(500);
        
        // Configure relationships
        builder.HasMany(e => e.RelatedEntities)
            .WithOne(r => r.YourEntity)
            .HasForeignKey(r => r.YourEntityId);
            
        // Add indexes if needed
        builder.HasIndex(e => e.Name);
    }
}
```

### 3. Update DbContext

Add your entity to `IvyContext.cs`:

```csharp
public DbSet<YourEntity> YourEntities { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // ... existing configurations
    modelBuilder.ApplyConfiguration(new YourEntityConfiguration());
}
```

### 4. Create Service Interface (Core Layer)

Define service interface in `Ivy.Core/Services/`:

```csharp
using Ivy.Core.Entities;
using IvyBackend;

namespace Ivy.Core.Services;

public interface IYourEntityService
{
    Task<Result<PaginatedResult<YourEntity>>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? searchTerm = null,
        bool? isActive = null,
        bool includeRelated = false
    );

    Task<Result<YourEntity>> GetByIdAsync(int id, bool includeRelated = false);
    Task<Result<YourEntity>> CreateAsync(YourEntity entity);
    Task<Result<YourEntity>> UpdateAsync(int id, YourEntity entity);
    Task<Result> DeleteAsync(int id);
    Task<Result<bool>> ExistsAsync(int id);
}
```

### 5. Implement Service (Core Layer)

Create service implementation:

```csharp
using IvyBackend;
using Ivy.Core.DataContext;
using Ivy.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ivy.Core.Services;

public class YourEntityService : IYourEntityService
{
    private readonly IvyContext _context;

    public YourEntityService(IvyContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedResult<YourEntity>>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        string? searchTerm = null,
        bool? isActive = null,
        bool includeRelated = false)
    {
        try
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = _context.YourEntities.AsQueryable();

            if (includeRelated)
            {
                query = query.Include(e => e.RelatedEntities);
            }

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(e => e.Name.Contains(searchTerm) || 
                                         e.Description.Contains(searchTerm));
            }

            if (isActive.HasValue)
            {
                query = query.Where(e => e.IsActive == isActive.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination and ordering
            var entities = await query
                .OrderBy(e => e.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var paginatedResult = PaginatedResult<YourEntity>.Create(entities, totalCount, page, pageSize);
            return Result<PaginatedResult<YourEntity>>.Ok("ENTITIES_RETRIEVED_SUCCESS", paginatedResult);
        }
        catch (Exception ex)
        {
            var emptyResult = new PaginatedResult<YourEntity>();
            return Result<PaginatedResult<YourEntity>>.Error("ENTITIES_RETRIEVAL_FAILED", emptyResult);
        }
    }

    public async Task<Result<YourEntity>> GetByIdAsync(int id, bool includeRelated = false)
    {
        try
        {
            var query = _context.YourEntities.AsQueryable();

            if (includeRelated)
            {
                query = query.Include(e => e.RelatedEntities);
            }

            var entity = await query.FirstOrDefaultAsync(e => e.Id == id);

            if (entity == null)
            {
                return Result<YourEntity>.Error("ENTITY_NOT_FOUND", null!);
            }

            return Result<YourEntity>.Ok("ENTITY_RETRIEVED_SUCCESS", entity);
        }
        catch (Exception ex)
        {
            return Result<YourEntity>.Error("ENTITY_RETRIEVAL_FAILED", null!);
        }
    }

    public async Task<Result<YourEntity>> CreateAsync(YourEntity entity)
    {
        try
        {
            // Validation logic
            var duplicateExists = await _context.YourEntities
                .AnyAsync(e => e.Name == entity.Name);

            if (duplicateExists)
            {
                return Result<YourEntity>.Error("ENTITY_NAME_ALREADY_EXISTS", null!);
            }

            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            _context.YourEntities.Add(entity);
            await _context.SaveChangesAsync();

            return Result<YourEntity>.Ok("ENTITY_CREATED_SUCCESS", entity);
        }
        catch (Exception ex)
        {
            return Result<YourEntity>.Error("ENTITY_CREATION_FAILED", null!);
        }
    }

    public async Task<Result<YourEntity>> UpdateAsync(int id, YourEntity entity)
    {
        try
        {
            var existingEntity = await _context.YourEntities.FirstOrDefaultAsync(e => e.Id == id);
            if (existingEntity == null)
            {
                return Result<YourEntity>.Error("ENTITY_NOT_FOUND", null!);
            }

            // Check for duplicates
            var duplicateExists = await _context.YourEntities
                .AnyAsync(e => e.Id != id && e.Name == entity.Name);

            if (duplicateExists)
            {
                return Result<YourEntity>.Error("ENTITY_NAME_ALREADY_EXISTS", null!);
            }

            // Update properties
            existingEntity.Name = entity.Name;
            existingEntity.Description = entity.Description;
            existingEntity.IsActive = entity.IsActive;
            existingEntity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result<YourEntity>.Ok("ENTITY_UPDATED_SUCCESS", existingEntity);
        }
        catch (Exception ex)
        {
            return Result<YourEntity>.Error("ENTITY_UPDATE_FAILED", null!);
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            var entity = await _context.YourEntities
                .Include(e => e.RelatedEntities)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (entity == null)
            {
                return Result.Error("ENTITY_NOT_FOUND");
            }

            // Check for dependencies
            var hasActiveRelated = entity.RelatedEntities?.Any(r => !r.IsDeleted) == true;
            if (hasActiveRelated)
            {
                return Result.Error("ENTITY_HAS_ACTIVE_DEPENDENCIES");
            }

            // Soft delete
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result.Ok("ENTITY_DELETED_SUCCESS");
        }
        catch (Exception ex)
        {
            return Result.Error("ENTITY_DELETION_FAILED");
        }
    }

    public async Task<Result<bool>> ExistsAsync(int id)
    {
        try
        {
            var exists = await _context.YourEntities.AnyAsync(e => e.Id == id);
            return Result<bool>.Ok("ENTITY_EXISTS_CHECK_SUCCESS", exists);
        }
        catch (Exception ex)
        {
            return Result<bool>.Error("ENTITY_EXISTS_CHECK_FAILED", false);
        }
    }
}
```

### 6. Create DTOs (API Layer)

Create DTOs in `Ivy.Api/DTOs/`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace Ivy.Api.DTOs;

public class YourEntityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<RelatedEntityDto>? RelatedEntities { get; set; }
}

public class CreateYourEntityDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

public class UpdateYourEntityDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

public class YourEntityQueryDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
    public int PageSize { get; set; } = 10;

    [StringLength(100)]
    public string? SearchTerm { get; set; }

    public bool? IsActive { get; set; }

    public bool IncludeRelated { get; set; } = false;
}
```

### 7. Create Controller (API Layer)

Create controller in `Ivy.Api/Controllers/`:

```csharp
using Ivy.Api.Services;

namespace Ivy.Api.Controllers;

[Route("api/your-entities")]
public class YourEntityController : BaseController
{
    private readonly IYourEntityService _yourEntityService;

    public YourEntityController(
        IYourEntityService yourEntityService,
        IApiResponseRepresenter responseRepresenter,
        ILogger<YourEntityController> logger
    )
        : base(responseRepresenter, logger)
    {
        _yourEntityService = yourEntityService;
    }

    /// <summary>
    /// Get all entities with pagination and filtering
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<YourEntityDto>>>> GetAllEntities(
        [FromQuery] YourEntityQueryDto query)
    {
        try
        {
            var result = await _yourEntityService.GetAllAsync(
                query.Page,
                query.PageSize,
                query.SearchTerm,
                query.IsActive,
                query.IncludeRelated
            );

            if (result.Success)
            {
                var entityDtos = result.Data.Data.Select(e =>
                    MapToDto(e, query.IncludeRelated)
                );
                var paginatedDto = new PaginatedResult<YourEntityDto>
                {
                    Data = entityDtos,
                    TotalCount = result.Data.TotalCount,
                    Page = result.Data.Page,
                    PageSize = result.Data.PageSize,
                    TotalPages = result.Data.TotalPages,
                    HasNextPage = result.Data.HasNextPage,
                    HasPreviousPage = result.Data.HasPreviousPage,
                };

                var mappedResult = Result<PaginatedResult<YourEntityDto>>.Ok(
                    result.MessageCode,
                    paginatedDto
                );
                return HandleResult(mappedResult);
            }

            var failedResult = Result<PaginatedResult<YourEntityDto>>.Error(
                result.MessageCode,
                default!
            );
            return HandleResult(failedResult);
        }
        catch (Exception ex)
        {
            return HandleInternalError<PaginatedResult<YourEntityDto>>(
                ex,
                "retrieving entities"
            );
        }
    }

    /// <summary>
    /// Get a specific entity by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<YourEntityDto>>> GetEntity(
        int id,
        [FromQuery] bool includeRelated = false
    )
    {
        try
        {
            var result = await _yourEntityService.GetByIdAsync(id, includeRelated);

            if (result.Success)
            {
                var entityDto = MapToDto(result.Data, includeRelated);
                var mappedResult = Result<YourEntityDto>.Ok(result.MessageCode, entityDto);
                return HandleResult(mappedResult);
            }

            var failedResult = Result<YourEntityDto>.Error(result.MessageCode, default!);
            return HandleResult(failedResult);
        }
        catch (Exception ex)
        {
            return HandleInternalError<YourEntityDto>(ex, $"retrieving entity with ID {id}");
        }
    }

    /// <summary>
    /// Create a new entity
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<YourEntityDto>>> CreateEntity(
        [FromBody] CreateYourEntityDto createEntityDto
    )
    {
        try
        {
            if (!IsModelValid())
            {
                return HandleValidationError<YourEntityDto>();
            }

            var entity = new YourEntity
            {
                Name = createEntityDto.Name,
                Description = createEntityDto.Description,
                IsActive = createEntityDto.IsActive,
            };

            var result = await _yourEntityService.CreateAsync(entity);

            if (result.Success)
            {
                var entityDto = MapToDto(result.Data, false);
                var mappedResult = Result<YourEntityDto>.Ok(result.MessageCode, entityDto);
                return HandleResult(mappedResult);
            }

            var failedResult = Result<YourEntityDto>.Error(result.MessageCode, default!);
            return HandleResult(failedResult);
        }
        catch (Exception ex)
        {
            return HandleInternalError<YourEntityDto>(ex, "creating entity");
        }
    }

    /// <summary>
    /// Update an existing entity
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<YourEntityDto>>> UpdateEntity(
        int id,
        [FromBody] UpdateYourEntityDto updateEntityDto
    )
    {
        try
        {
            if (!IsModelValid())
            {
                return HandleValidationError<YourEntityDto>();
            }

            var entity = new YourEntity
            {
                Name = updateEntityDto.Name,
                Description = updateEntityDto.Description,
                IsActive = updateEntityDto.IsActive,
            };

            var result = await _yourEntityService.UpdateAsync(id, entity);

            if (result.Success)
            {
                var entityDto = MapToDto(result.Data, false);
                var mappedResult = Result<YourEntityDto>.Ok(result.MessageCode, entityDto);
                return HandleResult(mappedResult);
            }

            var failedResult = Result<YourEntityDto>.Error(result.MessageCode, default!);
            return HandleResult(failedResult);
        }
        catch (Exception ex)
        {
            return HandleInternalError<YourEntityDto>(ex, $"updating entity with ID {id}");
        }
    }

    /// <summary>
    /// Delete an entity (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteEntity(int id)
    {
        try
        {
            var result = await _yourEntityService.DeleteAsync(id);
            return HandleResult(result);
        }
        catch (Exception ex)
        {
            return HandleInternalError(ex, $"deleting entity with ID {id}");
        }
    }

    /// <summary>
    /// Check if an entity exists by ID
    /// </summary>
    [HttpGet("{id}/exists")]
    public async Task<ActionResult<ApiResponse<bool>>> CheckEntityExists(int id)
    {
        try
        {
            var result = await _yourEntityService.ExistsAsync(id);
            return HandleResult(result);
        }
        catch (Exception ex)
        {
            return HandleInternalError<bool>(ex, $"checking if entity exists with ID {id}");
        }
    }

    private static YourEntityDto MapToDto(YourEntity entity, bool includeRelated)
    {
        var dto = new YourEntityDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
        };

        if (includeRelated && entity.RelatedEntities != null)
        {
            dto.RelatedEntities = entity.RelatedEntities
                .Select(r => new RelatedEntityDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    // Map other properties
                })
                .ToList();
        }

        return dto;
    }
}
```

### 8. Register Services (Program.cs)

Add service registration to `Program.cs`:

```csharp
// Add this to the service configuration section
builder.Services.AddScoped<IYourEntityService, YourEntityService>();
```

## Entity Layer

### BaseEntity Inheritance

All entities should inherit from `BaseEntity<T>` which provides:

- `Id` property of type T
- `CreatedAt`, `UpdatedAt` timestamps
- `DeletedAt` for soft deletes
- `IsDeleted` flag for soft deletes
- `IsActive` flag for active/inactive states

### Entity Guidelines

- Use meaningful, singular names (e.g., `Product`, not `Products`)
- Include required properties with appropriate data types
- Define navigation properties for relationships
- Use proper naming conventions (PascalCase)

## Service Layer

### Service Interface Guidelines

- Interface name: `I{EntityName}Service`
- Include standard CRUD operations
- Add pagination support for list operations
- Include existence checks
- Support soft deletes

### Service Implementation Guidelines

- Implement comprehensive error handling with try-catch blocks
- Use the Result pattern for all return types
- Validate input parameters
- Apply business logic and validations
- Use Entity Framework operations efficiently
- Log errors appropriately

## Controller Layer

### Controller Guidelines

- Inherit from `BaseController`
- Use plural, kebab-case for route naming
- Include XML documentation for all endpoints
- Follow RESTful conventions
- Implement proper error handling using base controller methods

### Standard Endpoints

1. `GET /api/{entities}` - Get all with pagination
2. `GET /api/{entities}/{id}` - Get by ID
3. `POST /api/{entities}` - Create new
4. `PUT /api/{entities}/{id}` - Update existing
5. `DELETE /api/{entities}/{id}` - Soft delete
6. `GET /api/{entities}/{id}/exists` - Check existence

## DTO Layer

### DTO Types Required

1. **Main DTO**: For returning data (`{Entity}Dto`)
2. **Create DTO**: For creation operations (`Create{Entity}Dto`)
3. **Update DTO**: For update operations (`Update{Entity}Dto`)
4. **Query DTO**: For filtering and pagination (`{Entity}QueryDto`)

### DTO Guidelines

- Use appropriate validation attributes
- Include only necessary properties
- Provide default values where appropriate
- Use meaningful property names

## Error Handling

### Result Pattern

- Use `Result` for operations without return data
- Use `Result<T>` for operations returning data
- Always include meaningful message codes
- Handle both success and failure scenarios

### Message Codes

Follow this convention:

- Success: `{ENTITY}_{OPERATION}_SUCCESS`
- Error: `{ENTITY}_{OPERATION}_FAILED`
- Not Found: `{ENTITY}_NOT_FOUND`
- Duplicate: `{ENTITY}_NAME_ALREADY_EXISTS`

### Adding Message Codes to MessageStore

When creating new APIs, you must add corresponding message codes to the `MessageStore.cs` file located in `Ivy.Core/Services/MessageStore.cs`. The MessageStore provides internationalization support with English and Arabic translations.

#### Steps to Add Message Codes

1. **Open MessageStore.cs** and locate the `InitializeMessages()` method
2. **Add your new message codes** to the dictionary following the existing pattern
3. **Provide translations** for both English (`"en"`) and Arabic (`"ar"`)
4. **Group related messages** together (e.g., all Product messages in one section)

#### Example: Adding Product Entity Messages

```csharp
// Add these to the InitializeMessages() method in MessageStore.cs

// Success Messages for Product
["PRODUCT_RETRIEVED_SUCCESS"] = new()
{
    ["en"] = "Product retrieved successfully",
    ["ar"] = "تم استرجاع المنتج بنجاح"
},
["PRODUCTS_RETRIEVED_SUCCESS"] = new()
{
    ["en"] = "Products retrieved successfully", 
    ["ar"] = "تم استرجاع المنتجات بنجاح"
},
["PRODUCT_CREATED_SUCCESS"] = new()
{
    ["en"] = "Product created successfully",
    ["ar"] = "تم إنشاء المنتج بنجاح"
},
["PRODUCT_UPDATED_SUCCESS"] = new()
{
    ["en"] = "Product updated successfully",
    ["ar"] = "تم تحديث المنتج بنجاح"
},
["PRODUCT_DELETED_SUCCESS"] = new()
{
    ["en"] = "Product deleted successfully",
    ["ar"] = "تم حذف المنتج بنجاح"
},
["PRODUCT_EXISTS_CHECK_SUCCESS"] = new()
{
    ["en"] = "Product existence check completed successfully",
    ["ar"] = "تم إنجاز فحص وجود المنتج بنجاح"
},

// Error Messages for Product
["PRODUCT_NOT_FOUND"] = new()
{
    ["en"] = "Product not found",
    ["ar"] = "المنتج غير موجود"
},
["PRODUCT_NAME_ALREADY_EXISTS"] = new()
{
    ["en"] = "A product with this name already exists",
    ["ar"] = "منتج بهذا الاسم موجود بالفعل"
},
["PRODUCT_CREATION_FAILED"] = new()
{
    ["en"] = "Failed to create the product",
    ["ar"] = "فشل في إنشاء المنتج"
},
["PRODUCT_UPDATE_FAILED"] = new()
{
    ["en"] = "Failed to update the product",
    ["ar"] = "فشل في تحديث المنتج"
},
["PRODUCT_DELETION_FAILED"] = new()
{
    ["en"] = "Failed to delete the product",
    ["ar"] = "فشل في حذف المنتج"
},
["PRODUCT_RETRIEVAL_FAILED"] = new()
{
    ["en"] = "Failed to retrieve the product",
    ["ar"] = "فشل في استرجاع المنتج"
},
["PRODUCTS_RETRIEVAL_FAILED"] = new()
{
    ["en"] = "Failed to retrieve products",
    ["ar"] = "فشل في استرجاع المنتجات"
},
["PRODUCT_EXISTS_CHECK_FAILED"] = new()
{
    ["en"] = "Failed to check product existence",
    ["ar"] = "فشل في فحص وجود المنتج"
},
["PRODUCT_HAS_ACTIVE_DEPENDENCIES"] = new()
{
    ["en"] = "Cannot delete product that has active dependencies",
    ["ar"] = "لا يمكن حذف المنتج الذي له تبعيات نشطة"
},
```

#### Message Code Naming Conventions

**Success Messages:**

- `{ENTITY}_RETRIEVED_SUCCESS` - For single item retrieval
- `{ENTITIES}_RETRIEVED_SUCCESS` - For list/paginated retrieval  
- `{ENTITY}_CREATED_SUCCESS` - For creation operations
- `{ENTITY}_UPDATED_SUCCESS` - For update operations
- `{ENTITY}_DELETED_SUCCESS` - For deletion operations
- `{ENTITY}_EXISTS_CHECK_SUCCESS` - For existence checks

**Error Messages:**

- `{ENTITY}_NOT_FOUND` - When entity doesn't exist
- `{ENTITY}_NAME_ALREADY_EXISTS` - For duplicate name validation
- `{ENTITY}_CREATION_FAILED` - When creation fails
- `{ENTITY}_UPDATE_FAILED` - When update fails
- `{ENTITY}_DELETION_FAILED` - When deletion fails
- `{ENTITY}_RETRIEVAL_FAILED` - When retrieval fails
- `{ENTITIES}_RETRIEVAL_FAILED` - When list retrieval fails
- `{ENTITY}_EXISTS_CHECK_FAILED` - When existence check fails
- `{ENTITY}_HAS_ACTIVE_DEPENDENCIES` - When deletion blocked by dependencies

#### Translation Guidelines

1. **English (`"en"`)**: Use clear, professional language
2. **Arabic (`"ar"`)**: Provide accurate Arabic translations
3. **Consistency**: Keep message tone and format consistent
4. **Clarity**: Messages should be clear and actionable

#### Best Practices for Message Codes

1. **Add all required messages** when creating a new entity
2. **Use UPPERCASE** for message code keys
3. **Follow naming conventions** strictly
4. **Group related messages** together in the dictionary
5. **Test translations** to ensure they make sense
6. **Keep messages concise** but descriptive

## Naming Conventions

### Files and Classes

- **Entities**: Singular, PascalCase (`Product`, `Category`)
- **Controllers**: `{Entity}Controller` (`ProductController`)
- **Services**: `I{Entity}Service`, `{Entity}Service`
- **DTOs**: `{Entity}Dto`, `Create{Entity}Dto`, etc.

### API Routes

- Use plural, kebab-case: `/api/products`, `/api/product-categories`
- Be consistent with naming across all endpoints

### Database

- **Tables**: Plural entity names (`Products`, `Categories`)
- **Columns**: PascalCase matching entity properties
- **Foreign Keys**: `{Entity}Id` (`CategoryId`)

## Best Practices

### 1. Validation

- Use Data Annotations for basic validation
- Implement business logic validation in services
- Always validate model state in controllers

### 2. Error Handling

- Use try-catch blocks in all service methods
- Return appropriate Result objects
- Log errors with meaningful context

### 3. Performance

- Use Include() for related data when needed
- Implement pagination for list operations
- Limit maximum page sizes (100 is recommended)

### 4. Security

- Validate all inputs
- Use proper authorization attributes
- Sanitize search terms and filters

### 5. Documentation

- Include XML documentation for all public methods
- Use meaningful parameter names
- Document expected behaviors and edge cases

## Code Templates

### Quick Entity Template

```csharp
namespace Ivy.Core.Entities;

public class EntityName : BaseEntity<int>
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ICollection<RelatedEntity>? RelatedEntities { get; set; }
}
```

### Quick Controller Template

```csharp
[Route("api/entity-names")]
public class EntityNameController : BaseController
{
    private readonly IEntityNameService _service;

    public EntityNameController(
        IEntityNameService service,
        IApiResponseRepresenter responseRepresenter,
        ILogger<EntityNameController> logger
    ) : base(responseRepresenter, logger)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<EntityNameDto>>>> GetAll(
        [FromQuery] EntityNameQueryDto query)
    {
        // Implementation follows the standard pattern
    }
    
    // Add other standard endpoints...
}
```

---

## Conclusion

Following these guidelines ensures consistency, maintainability, and scalability across the Ivy Backend project. Always refer to the `GovernorateController` and related files as reference implementations when in doubt.

For any questions or clarifications, review the existing implementation patterns in the codebase or consult with the development team.
