# Doctor Authentication API Documentation

## Base URL

`/api/doctor-auth`

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

## 1. Doctor Registration

**POST** `/register`

### Request Body

```json
{
  "firstName": "Ahmed",
  "middleName": "Ali",
  "lastName": "Hassan",
  "password": "SecurePass123",
  "phoneNumber": "0912345678",
  "gender": 1,
  "dateOfBirth": "1990-05-15"
}
```

### Response

```json
{
  "success": true,
  "messageCode": "REGISTRATION_SUCCESS",
  "data": {
    "message": "Doctor registered successfully. Please verify your phone number with the OTP sent.",
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
  "phoneNumber": "0912345678",
  "otp": "123456"
}
```

### Response

```json
{
  "success": true,
  "messageCode": "OTP_VERIFIED",
  "data": {
    "doctor": {
      "userId": 1,
      "user": {
        "id": 1,
        "firstName": "Ahmed",
        "middleName": "Ali",
        "lastName": "Hassan",
        "userName": "Ahmed Ali Hassan",
        "phoneNumber": "0912345678",
        "gender": 1,
        "isPhoneVerified": true,
        "dateOfBirth": "1990-05-15T00:00:00Z",
        "isActive": true,
        "createdAt": "2023-12-01T10:00:00Z",
        "updatedAt": "2023-12-01T10:00:00Z"
      },
      "profileImageUrl": "",
      "isProfileCompleted": false
    },
    "message": "Phone number verified successfully. Account is now active.",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

---

## 3. Doctor Login

**POST** `/login`

### Request Body

```json
{
  "phoneNumber": "0912345678",
  "password": "SecurePass123"
}
```

### Response

```json
{
  "success": true,
  "messageCode": "LOGIN_SUCCESS",
  "data": {
    "doctor": {
      "userId": 1,
      "user": {
        "id": 1,
        "firstName": "Ahmed",
        "middleName": "Ali",
        "lastName": "Hassan",
        "userName": "Ahmed Ali Hassan",
        "phoneNumber": "0912345678",
        "gender": 1,
        "isPhoneVerified": true,
        "dateOfBirth": "1990-05-15T00:00:00Z",
        "isActive": true,
        "createdAt": "2023-12-01T10:00:00Z",
        "updatedAt": "2023-12-01T10:00:00Z"
      },
      "profileImageUrl": "",
      "isProfileCompleted": false
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
  "phoneNumber": "0912345678"
}
```

### Response

```json
{
  "success": true,
  "messageCode": "FORGOT_PASSWORD_OTP_SENT",
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
  "phoneNumber": "0912345678",
  "otp": "654321",
  "newPassword": "NewSecurePass123"
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
      "firstName": "Ahmed",
      "middleName": "Ali",
      "lastName": "Hassan",
      "userName": "Ahmed Ali Hassan",
      "phoneNumber": "0912345678",
      "gender": 1,
      "isPhoneVerified": true,
      "dateOfBirth": "1990-05-15T00:00:00Z",
      "isActive": true,
      "createdAt": "2023-12-01T10:00:00Z",
      "updatedAt": "2023-12-01T10:00:00Z"
    },
    "profileImageUrl": "",
    "isProfileCompleted": false
  }
}
```

---

## Field Validations

### RegisterDoctorDto

- **firstName**: Required, 1-50 characters
- **middleName**: Optional, max 50 characters
- **lastName**: Required, 1-50 characters
- **password**: Required, 8-100 characters
- **phoneNumber**: Required, exactly 10 digits, must start with "09"
- **gender**: Required, valid enum value (1=Male, 2=Female, 3=NotSpecified)
- **dateOfBirth**: Required, valid date

### LoginDoctorDto

- **phoneNumber**: Required, exactly 10 digits, must start with "09"
- **password**: Required, 1-100 characters

### VerifyDoctorOtpDto

- **phoneNumber**: Required, exactly 10 digits, must start with "09"
- **otp**: Required, exactly 6 digits

### ForgotDoctorPasswordDto

- **phoneNumber**: Required, exactly 10 digits, must start with "09"

### ResetDoctorPasswordDto

- **phoneNumber**: Required, exactly 10 digits, must start with "09"
- **otp**: Required, exactly 6 digits
- **newPassword**: Required, 8-100 characters

---

## Gender Enum Values

- **1**: Male
- **2**: Female
- **3**: NotSpecified

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

### Phone Number Already Exists (409 Conflict)

```json
{
  "success": false,
  "messageCode": "PHONE_NUMBER_ALREADY_EXISTS",
  "message": "Phone number already registered"
}
```

### Invalid Credentials (401 Unauthorized)

```json
{
  "success": false,
  "messageCode": "INVALID_CREDENTIALS",
  "message": "Invalid phone number or password"
}
```

### Invalid OTP (400 Bad Request)

```json
{
  "success": false,
  "messageCode": "INVALID_OTP",
  "message": "Invalid or expired OTP"
}
```

### Account Not Found (404 Not Found)

```json
{
  "success": false,
  "messageCode": "DOCTOR_NOT_FOUND",
  "message": "Doctor account not found"
}
```

### Account Not Verified (400 Bad Request)

```json
{
  "success": false,
  "messageCode": "ACCOUNT_NOT_VERIFIED",
  "message": "Please verify your phone number first"
}
```

### Account Inactive (400 Bad Request)

```json
{
  "success": false,
  "messageCode": "ACCOUNT_INACTIVE",
  "message": "Account is inactive"
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
- **Role**: `doctor`
- **Claims**: Contains doctor user ID and username
- **Usage**: Required for protected endpoints (`/my-profile`)

### Token Generation

- Generated upon successful OTP verification (registration)
- Generated upon successful login
- Contains user ID, username, and role
- Used to authenticate subsequent requests to protected endpoints

---

## Security Notes

1. **Password Requirements**: Minimum 8 characters, maximum 100 characters
2. **Phone Number Format**: Must start with "09" and be exactly 10 digits
3. **OTP Security**: 6-digit numeric code, expires after a certain time
4. **JWT Authorization**: Required for profile access
5. **Phone Verification**: Account must be verified before login
6. **Token Expiration**: Tokens have expiration (configured in JWT settings)

---

## Usage Examples

### Registration Flow

1. Doctor provides registration details
2. POST to `/api/doctor-auth/register` with doctor data
3. OTP is sent to provided phone number
4. Doctor enters OTP for verification
5. POST to `/api/doctor-auth/verify-otp` with phone number and OTP
6. Account is activated and JWT token is returned

### Login Flow

1. Doctor enters phone number and password
2. POST to `/api/doctor-auth/login` with credentials
3. Store returned JWT token securely
4. Use token in Authorization header for subsequent requests

### Forgot Password Flow

1. Doctor enters phone number
2. POST to `/api/doctor-auth/forgot-password` with phone number
3. OTP is sent to phone number
4. Doctor enters OTP and new password
5. POST to `/api/doctor-auth/reset-password` with phone number, OTP, and new password
6. Password is reset successfully

### Profile Access

1. Send GET request to `/api/doctor-auth/my-profile`
2. Include `Authorization: Bearer {token}` header
3. Receive doctor profile data

---

## Development Notes

1. **OTP in Response**: Currently OTP is returned in API response for development purposes. In production, OTP should only be sent via SMS.
2. **Username Generation**: Username is automatically generated as "FirstName MiddleName LastName".
3. **Profile Completion**: `isProfileCompleted` field indicates if doctor has completed their profile setup.
4. **Image Upload**: Profile image URL is currently empty - image upload functionality can be added separately.
