# Admin Authentication API Documentation

## Base URL

`/api/admin`

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

---

## 1. Admin Login

**POST** `/login`

### Request Body

```json
{
  "email": "admin@ivy.com",
  "password": "Admin123!"
}
```

### Response

```json
{
  "success": true,
  "messageCode": "LOGIN_SUCCESS",
  "data": {
    "admin": {
      "id": 1,
      "email": "admin@example.com",
      "isActive": true,
      "createdAt": "2023-12-01T10:00:00Z",
      "updatedAt": "2023-12-01T10:00:00Z"
    },
    "message": "Admin login successful.",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

---

## 2. Get Admin Profile

**GET** `/profile`

### Headers

```
Authorization: Bearer {jwt_token}
```

### Response

```json
{
  "success": true,
  "messageCode": "PROFILE_RETRIEVED",
  "data": {
    "id": 1,
    "email": "admin@example.com",
    "isActive": true,
    "createdAt": "2023-12-01T10:00:00Z",
    "updatedAt": "2023-12-01T10:00:00Z"
  }
}
```

---

## 3. Change Password

**PUT** `/change-password`

### Headers

```
Authorization: Bearer {jwt_token}
```

### Request Body

```json
{
  "currentPassword": "oldpassword123",
  "newPassword": "newpassword123",
  "confirmPassword": "newpassword123"
}
```

### Response

```json
{
  "success": true,
  "messageCode": "PASSWORD_CHANGED",
  "data": "Password changed successfully."
}
```

---

## Field Validations

### AdminLoginDto

- **email**: Required, valid email address, max 255 characters
- **password**: Required, 6-255 characters

### ChangeAdminPasswordDto

- **currentPassword**: Required, 6-255 characters
- **newPassword**: Required, 6-255 characters
- **confirmPassword**: Required, must match newPassword

---

## Error Responses

### Validation Errors (400 Bad Request)

```json
{
  "success": false,
  "messageCode": "VALIDATION_ERROR",
  "message": "Validation failed"
}
```

### Authentication Errors (401 Unauthorized)

```json
{
  "success": false,
  "messageCode": "INVALID_CREDENTIALS",
  "message": "Invalid email or password"
}
```

### Invalid Token (401 Unauthorized)

```json
{
  "success": false,
  "messageCode": "INVALID_TOKEN",
  "message": "Invalid or expired token"
}
```

### Password Confirmation Mismatch (400 Bad Request)

```json
{
  "success": false,
  "messageCode": "PASSWORD_CONFIRMATION_MISMATCH",
  "message": "Password confirmation does not match"
}
```

### Current Password Incorrect (400 Bad Request)

```json
{
  "success": false,
  "messageCode": "INVALID_CURRENT_PASSWORD",
  "message": "Current password is incorrect"
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

## Authentication

### JWT Token

- **Header**: `Authorization: Bearer {token}`
- **Role**: `admin`
- **Claims**: Contains admin ID and email
- **Usage**: Required for all protected endpoints (`/profile`, `/change-password`)

### Token Generation

- Generated upon successful login
- Contains admin ID, email, and role
- Used to authenticate subsequent requests to protected endpoints

---

## Security Notes

1. **Password Requirements**: Minimum 6 characters, maximum 255 characters
2. **Email Validation**: Must be a valid email format
3. **JWT Authorization**: Required for profile access and password changes
4. **Password Change**: Requires current password verification
5. **Token Expiration**: Tokens have expiration (configured in JWT settings)

---

## Usage Examples

### Login Flow

1. Admin enters credentials on login page
2. POST to `/api/admin/login` with email and password
3. Store returned JWT token securely
4. Use token in Authorization header for subsequent requests

### Profile Access

1. Send GET request to `/api/admin/profile`
2. Include `Authorization: Bearer {token}` header
3. Receive admin profile data

### Password Change

1. Admin provides current password and new password
2. PUT to `/api/admin/change-password` with password data
3. Include `Authorization: Bearer {token}` header
4. Receive confirmation of password change