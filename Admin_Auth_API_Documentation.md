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
  "email": "admin@example.com",
  "password": "adminPassword123"
}
```

### Response

```json
{
  "success": true,
  "messageCode": "ADMIN_LOGIN_SUCCESS",
  "data": {
    "admin": {
      "id": 1,
      "email": "admin@example.com",
      "isActive": true,
      "createdAt": "2023-12-01T10:00:00Z",
      "updatedAt": "2023-12-01T10:05:00Z"
    },
    "message": "Admin login successful.",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

### Error Responses

```json
{
  "success": false,
  "messageCode": "INVALID_CREDENTIALS",
  "message": "Invalid email or password"
}
```

```json
{
  "success": false,
  "messageCode": "ACCOUNT_INACTIVE",
  "message": "Account is inactive. Please contact support."
}
```

---

## 2. Get Admin Profile

**GET** `/profile`

**Authorization Required**: Bearer Token with `admin` role

### Headers

```
Authorization: Bearer <jwt_token>
```

### Response

```json
{
  "success": true,
  "messageCode": "ADMIN_PROFILE_RETRIEVED_SUCCESS",
  "data": {
    "id": 1,
    "email": "admin@example.com",
    "isActive": true,
    "createdAt": "2023-12-01T10:00:00Z",
    "updatedAt": "2023-12-01T10:05:00Z"
  }
}
```

### Error Responses

```json
{
  "success": false,
  "messageCode": "INVALID_TOKEN",
  "message": "Invalid or expired token"
}
```

```json
{
  "success": false,
  "messageCode": "ADMIN_NOT_FOUND",
  "message": "Admin not found"
}
```

---

## 3. Update Admin Profile

**PUT** `/profile`

**Authorization Required**: Bearer Token with `admin` role

### Headers

```
Authorization: Bearer <jwt_token>
```

### Request Body (Email Only)

```json
{
  "email": "newemail@example.com"
}
```

### Request Body (Email and Password)

```json
{
  "email": "newemail@example.com",
  "currentPassword": "currentPassword123",
  "newPassword": "newPassword123",
  "confirmPassword": "newPassword123"
}
```

### Response

```json
{
  "success": true,
  "messageCode": "ADMIN_PROFILE_UPDATED_SUCCESS",
  "data": {
    "id": 1,
    "email": "newemail@example.com",
    "isActive": true,
    "createdAt": "2023-12-01T10:00:00Z",
    "updatedAt": "2023-12-01T10:15:00Z"
  }
}
```

### Error Responses

```json
{
  "success": false,
  "messageCode": "ADMIN_EMAIL_ALREADY_EXISTS",
  "message": "An admin with this email already exists"
}
```

```json
{
  "success": false,
  "messageCode": "INVALID_CURRENT_PASSWORD",
  "message": "Current password is incorrect"
}
```

```json
{
  "success": false,
  "messageCode": "PASSWORD_CONFIRMATION_MISMATCH",
  "message": "Password confirmation does not match"
}
```

---

## 4. Change Admin Password

**PUT** `/change-password`

**Authorization Required**: Bearer Token with `admin` role

### Headers

```
Authorization: Bearer <jwt_token>
```

### Request Body

```json
{
  "currentPassword": "currentPassword123",
  "newPassword": "newPassword123",
  "confirmPassword": "newPassword123"
}
```

### Response

```json
{
  "success": true,
  "messageCode": "ADMIN_PASSWORD_CHANGED_SUCCESS",
  "data": "Password changed successfully."
}
```

### Error Responses

```json
{
  "success": false,
  "messageCode": "INVALID_CURRENT_PASSWORD",
  "message": "Current password is incorrect"
}
```

```json
{
  "success": false,
  "messageCode": "PASSWORD_CONFIRMATION_MISMATCH",
  "message": "Password confirmation does not match"
}
```

---

## 5. Check Email Exists

**GET** `/email-exists?email=admin@example.com`

### Query Parameters

- `email` (required): The email address to check

### Response

```json
{
  "success": true,
  "messageCode": "ADMIN_EMAIL_CHECK_SUCCESS",
  "data": true
}
```

### Error Responses

```json
{
  "success": false,
  "messageCode": "INVALID_EMAIL",
  "message": "Invalid email address"
}
```

---

## Authentication Flow

1. **Admin Login**: Use `POST /api/admin/login` with email and password
2. **Store Token**: Store the JWT token from the response
3. **Access Protected Routes**: Include the token in the Authorization header for subsequent requests
4. **Profile Management**: Use the profile endpoints to view and update admin information

---

## Error Codes

### Authentication Errors
- `INVALID_CREDENTIALS` - Invalid email or password
- `ACCOUNT_INACTIVE` - Admin account is inactive
- `INVALID_TOKEN` - JWT token is invalid or expired
- `ADMIN_LOGIN_FAILED` - General login failure

### Profile Management Errors
- `ADMIN_NOT_FOUND` - Admin not found in database
- `ADMIN_EMAIL_ALREADY_EXISTS` - Email is already in use by another admin
- `INVALID_CURRENT_PASSWORD` - Current password verification failed
- `PASSWORD_CONFIRMATION_MISMATCH` - New password and confirmation don't match
- `CURRENT_PASSWORD_REQUIRED` - Current password is required for password changes
- `INVALID_EMAIL` - Invalid email format
- `INVALID_UPDATE_DATA` - Invalid data provided for update
- `ADMIN_PROFILE_UPDATE_FAILED` - General profile update failure
- `ADMIN_PASSWORD_CHANGE_FAILED` - General password change failure

---

## Security Notes

1. **JWT Tokens**: All protected endpoints require a valid JWT token with `admin` role
2. **Password Requirements**: Passwords must be at least 6 characters long
3. **Email Validation**: Email addresses must be in valid email format
4. **Password Verification**: Current password verification is required for password changes
5. **Account Status**: Only active admin accounts can authenticate

---

## Example Usage with curl

### Login
```bash
curl -X POST http://localhost:5000/api/admin/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "adminPassword123"
  }'
```

### Get Profile
```bash
curl -X GET http://localhost:5000/api/admin/profile \
  -H "Authorization: Bearer <jwt_token>"
```

### Update Profile
```bash
curl -X PUT http://localhost:5000/api/admin/profile \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <jwt_token>" \
  -d '{
    "email": "newemail@example.com"
  }'
```

### Change Password
```bash
curl -X PUT http://localhost:5000/api/admin/change-password \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <jwt_token>" \
  -d '{
    "currentPassword": "currentPassword123",
    "newPassword": "newPassword123",
    "confirmPassword": "newPassword123"
  }'
```
