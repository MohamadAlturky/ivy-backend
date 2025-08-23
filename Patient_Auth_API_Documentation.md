# Patient Authentication API Documentation

## Base URL

`/api/patient-auth`

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

## 1. Register Patient

**POST** `/register`

### Request Body

```json
{
  "firstName": "John",
  "middleName": "Michael",
  "lastName": "Doe",
  "password": "password123",
  "phoneNumber": "+1234567890",
  "gender": 0,
  "dateOfBirth": "1990-01-15T00:00:00Z"
}
```

### Response

```json
{
  "success": true,
  "messageCode": "REGISTRATION_SUCCESS",
  "data": {
    "message": "Patient registered successfully. Please verify your phone number with the OTP sent.",
    "otp": "123456"
  }
}
```

---

## 2. Verify OTP

**POST** `/verify-otp`

### Request Body

```json
{
  "phoneNumber": "+1234567890",
  "otp": "123456"
}
```

### Response

```json
{
  "success": true,
  "messageCode": "VERIFICATION_SUCCESS",
  "data": {
    "patient": {
      "userId": 1,
      "user": {
        "id": 1,
        "firstName": "John",
        "middleName": "Michael",
        "lastName": "Doe",
        "userName": "John Michael Doe",
        "phoneNumber": "+1234567890",
        "gender": 0,
        "isPhoneVerified": true,
        "dateOfBirth": "1990-01-15T00:00:00Z",
        "isActive": true,
        "createdAt": "2023-12-01T10:00:00Z",
        "updatedAt": "2023-12-01T10:05:00Z"
      }
    },
    "message": "Phone number verified successfully. Account is now active.",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

---

## 3. Login

**POST** `/login`

### Request Body

```json
{
  "phoneNumber": "+1234567890",
  "password": "password123"
}
```

### Response

```json
{
  "success": true,
  "messageCode": "LOGIN_SUCCESS",
  "data": {
    "patient": {
      "userId": 1,
      "user": {
        "id": 1,
        "firstName": "John",
        "middleName": "Michael",
        "lastName": "Doe",
        "userName": "John Michael Doe",
        "phoneNumber": "+1234567890",
        "gender": 0,
        "isPhoneVerified": true,
        "dateOfBirth": "1990-01-15T00:00:00Z",
        "isActive": true,
        "createdAt": "2023-12-01T10:00:00Z",
        "updatedAt": "2023-12-01T10:05:00Z"
      }
    },
    "message": "Login successful.",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

---

## 4. Forgot Password

**POST** `/forgot-password`

### Request Body

```json
{
  "phoneNumber": "+1234567890"
}
```

### Response

```json
{
  "success": true,
  "messageCode": "FORGOT_PASSWORD_SUCCESS",
  "data": {
    "message": "Password reset OTP has been sent to your phone number.",
    "otp": "654321"
  }
}
```

---

## 5. Reset Password

**POST** `/reset-password`

### Request Body

```json
{
  "phoneNumber": "+1234567890",
  "otp": "654321",
  "newPassword": "newpassword123"
}
```

### Response

```json
{
  "success": true,
  "messageCode": "PASSWORD_RESET_SUCCESS",
  "data": "Password has been reset successfully. You can now login with your new password."
}
```

---

## 6. Get My Profile

**GET** `/my-profile`

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
    "userId": 1,
    "user": {
      "id": 1,
      "firstName": "John",
      "middleName": "Michael",
      "lastName": "Doe",
      "userName": "John Michael Doe",
      "phoneNumber": "+1234567890",
      "gender": 0,
      "isPhoneVerified": true,
      "dateOfBirth": "1990-01-15T00:00:00Z",
      "isActive": true,
      "createdAt": "2023-12-01T10:00:00Z",
      "updatedAt": "2023-12-01T10:05:00Z"
    }
  }
}
```

---

## Field Validations

### RegisterPatientDto

- **firstName**: Required, 1-50 characters
- **middleName**: Optional, max 50 characters
- **lastName**: Required, 1-50 characters
- **password**: Required, 6-255 characters
- **phoneNumber**: Required, valid phone format, max 20 characters
- **gender**: Required, enum value (0=Male, 1=Female)
- **dateOfBirth**: Required, valid date

### Common Validations

- **phoneNumber**: Valid phone format, max 20 characters
- **otp**: 4-10 characters
- **password**: 6-255 characters

---

## Error Responses

### Validation Errors (400 Bad Request)

```json
{
  "success": false,
  "messageCode": "VALIDATION_ERROR",
  "message": "Validation failed",
}
```

### Authentication Errors (401 Unauthorized)

```json
{
  "success": false,
  "messageCode": "INVALID_CREDENTIALS",
  "message": "Invalid phone number or password"
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

> notes:
Gender:

- Male = 1,
- Female = 2,
- NotSpecified = 3,
