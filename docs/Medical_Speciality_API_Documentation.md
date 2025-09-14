# Medical Speciality API Documentation

## Base URL

`/api/medical-specialities`

## Response Format

All endpoints return data in the following format:

```json
{
  "success": true,
  "messageCode": "SUCCESS_CODE",
  "message": "Human readable message",
  "data": {}
}
```

## Headers

### Accept-Language
For localized responses, include the `Accept-Language` header:
- `ar` for Arabic names and descriptions
- `en` for English names and descriptions (default)

```
Accept-Language: ar
```

---

## 1. Get All Medical Specialities

**GET** `/`

Retrieve all medical specialities with pagination, filtering, and search capabilities.

### Query Parameters

```
page=1                    # Page number (minimum: 1)
pageSize=10              # Items per page (1-100)
name=cardiology          # Filter by name
searchTerm=heart         # Search in name and description
isActive=true            # Filter by active status
```

### Response

```json
{
  "success": true,
  "messageCode": "SPECIALITIES_RETRIEVED",
  "message": "Medical specialities retrieved successfully",
  "data": {
    "data": [
      {
        "id": 1,
        "name": "Cardiology",
        "description": "Heart and cardiovascular system specialists",
        "isActive": true,
        "createdAt": "2023-12-01T10:00:00Z",
        "updatedAt": "2023-12-01T10:00:00Z"
      },
      {
        "id": 2,
        "name": "Neurology",
        "description": "Nervous system specialists",
        "isActive": true,
        "createdAt": "2023-12-01T11:00:00Z",
        "updatedAt": "2023-12-01T11:00:00Z"
      }
    ],
    "totalCount": 25,
    "page": 1,
    "pageSize": 10,
    "totalPages": 3,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

---

## 2. Get Medical Speciality by ID

**GET** `/{id}`

Retrieve a specific medical speciality by its ID.

### Path Parameters

- `id` (integer, required): The ID of the medical speciality

### Response

```json
{
  "success": true,
  "messageCode": "SPECIALITY_RETRIEVED",
  "message": "Medical speciality retrieved successfully",
  "data": {
    "id": 1,
    "name": "Cardiology",
    "description": "Heart and cardiovascular system specialists",
    "isActive": true,
    "createdAt": "2023-12-01T10:00:00Z",
    "updatedAt": "2023-12-01T10:00:00Z"
  }
}
```

---

## 3. Create Medical Speciality

**POST** `/`

Create a new medical speciality with bilingual support.

### Request Body

```json
{
  "nameAr": "طب القلب",
  "nameEn": "Cardiology",
  "descriptionAr": "أطباء متخصصون في القلب والجهاز القلبي الوعائي",
  "descriptionEn": "Heart and cardiovascular system specialists",
  "isActive": true
}
```

### Response

```json
{
  "success": true,
  "messageCode": "SPECIALITY_CREATED",
  "message": "Medical speciality created successfully",
  "data": {
    "id": 3,
    "name": "Cardiology",
    "description": "Heart and cardiovascular system specialists",
    "isActive": true,
    "createdAt": "2023-12-01T12:00:00Z",
    "updatedAt": "2023-12-01T12:00:00Z"
  }
}
```

---

## 4. Update Medical Speciality

**PUT** `/{id}`

Update an existing medical speciality.

### Path Parameters

- `id` (integer, required): The ID of the medical speciality to update

### Request Body

```json
{
  "nameAr": "طب القلب المحدث",
  "nameEn": "Updated Cardiology",
  "descriptionAr": "وصف محدث لأطباء القلب",
  "descriptionEn": "Updated description for heart specialists",
  "isActive": false
}
```

### Response

```json
{
  "success": true,
  "messageCode": "SPECIALITY_UPDATED",
  "message": "Medical speciality updated successfully",
  "data": {
    "id": 1,
    "name": "Updated Cardiology",
    "description": "Updated description for heart specialists",
    "isActive": false,
    "createdAt": "2023-12-01T10:00:00Z",
    "updatedAt": "2023-12-01T15:30:00Z"
  }
}
```

---

## 5. Delete Medical Speciality

**DELETE** `/{id}`

Soft delete a medical speciality (marks as inactive rather than permanently deleting).

### Path Parameters

- `id` (integer, required): The ID of the medical speciality to delete

### Response

```json
{
  "success": true,
  "messageCode": "SPECIALITY_DELETED",
  "message": "Medical speciality deleted successfully"
}
```

---

## 6. Check Medical Speciality Existence

**GET** `/{id}/exists`

Check if a medical speciality exists by its ID.

### Path Parameters

- `id` (integer, required): The ID of the medical speciality to check

### Response

```json
{
  "success": true,
  "messageCode": "EXISTENCE_CHECKED",
  "message": "Speciality existence checked successfully",
  "data": true
}
```

---

## Field Validations

### CreateMedicalSpecialityDto / UpdateMedicalSpecialityDto

- **nameAr**: Required, 1-100 characters
- **nameEn**: Required, 1-100 characters
- **descriptionAr**: Optional, maximum 500 characters
- **descriptionEn**: Optional, maximum 500 characters
- **isActive**: Boolean, defaults to true

### MedicalSpecialityQueryDto

- **page**: Integer, minimum 1, defaults to 1
- **pageSize**: Integer, 1-100, defaults to 10
- **name**: Optional, maximum 100 characters
- **searchTerm**: Optional, maximum 100 characters
- **isActive**: Optional boolean filter

---

## Error Responses

### Validation Errors (400 Bad Request)

```json
{
  "success": false,
  "messageCode": "VALIDATION_ERROR",
  "message": "One or more validation errors occurred",
  "data": {
    "NameAr": ["The NameAr field is required."],
    "NameEn": ["The NameEn field must be between 1 and 100 characters."]
  }
}
```

### Not Found (404 Not Found)

```json
{
  "success": false,
  "messageCode": "SPECIALITY_NOT_FOUND",
  "message": "Medical speciality with the specified ID was not found"
}
```

### Conflict (409 Conflict)

```json
{
  "success": false,
  "messageCode": "SPECIALITY_ALREADY_EXISTS",
  "message": "A medical speciality with this name already exists"
}
```

### Delete Conflict (409 Conflict)

```json
{
  "success": false,
  "messageCode": "CANNOT_DELETE_SPECIALITY",
  "message": "Cannot delete medical speciality as it has associated records"
}
```

### Server Errors (500 Internal Server Error)

```json
{
  "success": false,
  "messageCode": "INTERNAL_ERROR",
  "message": "An error occurred while processing your request"
}
```

---

## Localization

The API supports bilingual content (Arabic and English):

### Storage
- All specialities are stored with both Arabic (`NameAr`, `DescriptionAr`) and English (`NameEn`, `DescriptionEn`) versions

### Retrieval
- The API returns the appropriate language based on the `Accept-Language` header
- Default language is English (`en`)
- Arabic responses use `Accept-Language: ar`

### Example with Arabic Response

**Request:**
```
GET /api/medical-specialities/1
Accept-Language: ar
```

**Response:**
```json
{
  "success": true,
  "messageCode": "SPECIALITY_RETRIEVED",
  "message": "تم استرداد التخصص الطبي بنجاح",
  "data": {
    "id": 1,
    "name": "طب القلب",
    "description": "أطباء متخصصون في القلب والجهاز القلبي الوعائي",
    "isActive": true,
    "createdAt": "2023-12-01T10:00:00Z",
    "updatedAt": "2023-12-01T10:00:00Z"
  }
}
```

---

## Pagination

All list endpoints return paginated results with the following structure:

```json
{
  "data": [], // Array of items
  "totalCount": 25, // Total number of items
  "page": 1, // Current page number
  "pageSize": 10, // Items per page
  "totalPages": 3, // Total number of pages
  "hasNextPage": true, // Whether there's a next page
  "hasPreviousPage": false // Whether there's a previous page
}
```

### Pagination Parameters
- **page**: Page number (minimum: 1, default: 1)
- **pageSize**: Items per page (range: 1-100, default: 10)

---

## Filtering and Search

The GET `/api/medical-specialities` endpoint supports:

### Filtering
- **name**: Exact match filter on speciality name
- **isActive**: Filter by active status (true/false)

### Search
- **searchTerm**: Searches in both name and description fields (case-insensitive)

### Combined Usage
```
GET /api/medical-specialities?searchTerm=heart&isActive=true&page=1&pageSize=5
```

---

## Usage Examples

### Basic CRUD Operations

1. **List all specialities:**
   ```
   GET /api/medical-specialities
   ```

2. **Search for heart-related specialities:**
   ```
   GET /api/medical-specialities?searchTerm=heart
   ```

3. **Get a specific speciality:**
   ```
   GET /api/medical-specialities/1
   ```

4. **Create a new speciality:**
   ```
   POST /api/medical-specialities
   Content-Type: application/json
   
   {
     "nameAr": "طب الأطفال",
     "nameEn": "Pediatrics",
     "descriptionAr": "طب الأطفال وحديثي الولادة",
     "descriptionEn": "Children and newborn medicine",
     "isActive": true
   }
   ```

5. **Update a speciality:**
   ```
   PUT /api/medical-specialities/1
   Content-Type: application/json
   
   {
     "nameAr": "طب القلب المتقدم",
     "nameEn": "Advanced Cardiology",
     "descriptionAr": "تخصص متقدم في طب القلب",
     "descriptionEn": "Advanced specialization in heart medicine",
     "isActive": true
   }
   ```

6. **Delete a speciality:**
   ```
   DELETE /api/medical-specialities/1
   ```

7. **Check if speciality exists:**
   ```
   GET /api/medical-specialities/1/exists
   ```

---

## Security Notes

1. **Input Validation**: All inputs are validated for length, format, and required fields
2. **SQL Injection Protection**: Uses Entity Framework parameterized queries
3. **XSS Prevention**: All output is properly encoded
4. **Soft Delete**: Delete operations are soft deletes to maintain data integrity
5. **Relationship Constraints**: Prevents deletion of specialities with associated records

---

## Response Codes

- **200 OK**: Successful GET, PUT operations
- **201 Created**: Successful POST operations  
- **400 Bad Request**: Validation errors, malformed requests
- **404 Not Found**: Resource not found
- **409 Conflict**: Business logic conflicts (duplicate names, cannot delete)
- **500 Internal Server Error**: Server errors
