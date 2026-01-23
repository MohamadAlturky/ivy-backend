# Agent Init Booking API Request Documentation

## Overview

The `InitBooking` endpoint is used by agents to initialize a new massage booking in the system. This endpoint creates a booking record with the specified service, therapist preferences, focus areas, and customer preferences.

## Endpoint Details

- **Method**: POST
- **Authentication**: Requires valid agent API key
- **Request Body**: `AgentInitBookingRequest`

## Request Structure

### AgentInitBookingRequest Properties

#### Required Fields

- **`UserId`** (`int`): The unique identifier of the customer/user who is making the booking
- **`ServiceId`** (`int`): The ID of the massage service being booked
- **`DurationId`** (`int`): The ID representing the duration/time slot for the service

#### Optional Fields

- **`TherapistId`** (`int?`): The ID of a specific therapist (nullable - if not specified, system may auto-assign)

- **`FocusAreas`** (`List<FocusAreas>?`): A list of body areas to focus on during the massage. Possible values include:
  - **Front body areas**: `Head`, `Neck`, `Shoulders`, `Chest`, `Abdomen`, `Arms`, `Legs`, `Feet`
  - **Back body areas**: `UpperBack`, `LowerBack`, `Spine`, `Hips`, `Buttocks`, `Thighs`, `Calves`

- **`AddonIds`** (`List<int>?`): A list of additional service IDs to be added to the main service (defaults to empty list)

- **`CustomerBookingPreferences`** (`CustomerBookingPreferences?`): Customer's massage preferences including:
  - **`Pressure`** (`PressureType?`): Massage pressure level
    - `Light` (1)
    - `Medium` (2) 
    - `High` (3)
  - **`Oil`** (`OilType?`): Type of oil to use
    - `Lavender` (1)
    - `Olive` (2)
    - `Ginger` (3)
  - **`Temperature`** (`OilTemperature?`): Oil temperature preference
    - `Hot` (1)
    - `Normal` (2)
  - **`BringEquipment`** (`bool?`): Whether the therapist should bring equipment

## Example Request

```json
{
  "UserId": 123,
  "ServiceId": 45,
  "TherapistId": 67,
  "FocusAreas": ["Shoulders", "UpperBack", "Neck"],
  "DurationId": 2,
  "AddonIds": [10, 15],
  "CustomerBookingPreferences": {
    "Pressure": 2,
    "Oil": 1,
    "Temperature": 1,
    "BringEquipment": true
  }
}
```

## Authentication & Authorization

- The endpoint requires a valid agent API key for authentication
- Returns `401 Unauthorized` if the API key is invalid

## Response

- **Success**: Returns a `BookingDto` with the created booking details
- **Error**: Returns appropriate error response with validation messages

## Purpose

This endpoint initializes a new booking in the system, creating a booking record with the specified service, therapist preferences, focus areas, and customer preferences. The booking will be in a pending state and require additional steps (like specifying date, address, members, etc.) to complete the full booking process.

## Related Endpoints

After initializing a booking, the following steps are typically required:
- Specify booking members
- Specify booking address  
- Specify booking date
- Confirm booking

## Notes

- The `IsReorder` flag is automatically set to `false` for agent-initiated bookings
- The system language is automatically determined from the agent context
- All validation is handled by the `InitBookingCommandValidator`
- The booking process uses database transactions to ensure data consistency
