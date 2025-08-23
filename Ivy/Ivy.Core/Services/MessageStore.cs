namespace IvyBackend.Services;

public class MessageStore : IMessageStore
{
    private readonly Dictionary<string, Dictionary<string, string>> _messages;

    public MessageStore()
    {
        _messages = InitializeMessages();
    }

    public string GetMessage(string messageCode, string language = "en")
    {
        if (_messages.TryGetValue(messageCode, out var translations))
        {
            if (translations.TryGetValue(language, out var message))
            {
                return message;
            }

            // Fallback to English if requested language not found
            if (translations.TryGetValue("en", out var englishMessage))
            {
                return englishMessage;
            }
        }

        // Return the message code if no translation found
        return messageCode;
    }

    private static Dictionary<string, Dictionary<string, string>> InitializeMessages()
    {
        return new Dictionary<string, Dictionary<string, string>>
        {
            // Success Messages
            ["SUCCESS"] = new()
            {
                ["en"] = "Operation completed successfully",
                ["ar"] = "تمت العملية بنجاح",
            },
            ["GOVERNORATE_RETRIEVED"] = new()
            {
                ["en"] = "Governorate retrieved successfully",
                ["ar"] = "تم استرجاع المحافظة بنجاح",
            },
            ["GOVERNORATES_RETRIEVED"] = new()
            {
                ["en"] = "Governorates retrieved successfully",
                ["ar"] = "تم استرجاع المحافظات بنجاح",
            },
            ["GOVERNORATE_CREATED"] = new()
            {
                ["en"] = "Governorate created successfully",
                ["ar"] = "تم إنشاء المحافظة بنجاح",
            },
            ["GOVERNORATE_UPDATED"] = new()
            {
                ["en"] = "Governorate updated successfully",
                ["ar"] = "تم تحديث المحافظة بنجاح",
            },
            ["GOVERNORATE_DELETED"] = new()
            {
                ["en"] = "Governorate deleted successfully",
                ["ar"] = "تم حذف المحافظة بنجاح",
            },
            ["CITY_RETRIEVED"] = new()
            {
                ["en"] = "City retrieved successfully",
                ["ar"] = "تم استرجاع المدينة بنجاح",
            },
            ["CITIES_RETRIEVED"] = new()
            {
                ["en"] = "Cities retrieved successfully",
                ["ar"] = "تم استرجاع المدن بنجاح",
            },
            ["CITY_CREATED"] = new()
            {
                ["en"] = "City created successfully",
                ["ar"] = "تم إنشاء المدينة بنجاح",
            },
            ["CITY_UPDATED"] = new()
            {
                ["en"] = "City updated successfully",
                ["ar"] = "تم تحديث المدينة بنجاح",
            },
            ["CITY_DELETED"] = new()
            {
                ["en"] = "City deleted successfully",
                ["ar"] = "تم حذف المدينة بنجاح",
            },
            ["CHECK_COMPLETED"] = new()
            {
                ["en"] = "Check completed successfully",
                ["ar"] = "تم إنجاز الفحص بنجاح",
            },
            ["COUNT_RETRIEVED"] = new()
            {
                ["en"] = "Count retrieved successfully",
                ["ar"] = "تم استرجاع العدد بنجاح",
            },
            ["MEDICAL_SPECIALITY_RETRIEVED_SUCCESS"] = new()
            {
                ["en"] = "Medical speciality retrieved successfully",
                ["ar"] = "تم استرجاع التخصص الطبي بنجاح",
            },
            ["MEDICAL_SPECIALITIES_RETRIEVED_SUCCESS"] = new()
            {
                ["en"] = "Medical specialities retrieved successfully",
                ["ar"] = "تم استرجاع التخصصات الطبية بنجاح",
            },
            ["MEDICAL_SPECIALITY_CREATED_SUCCESS"] = new()
            {
                ["en"] = "Medical speciality created successfully",
                ["ar"] = "تم إنشاء التخصص الطبي بنجاح",
            },
            ["MEDICAL_SPECIALITY_UPDATED_SUCCESS"] = new()
            {
                ["en"] = "Medical speciality updated successfully",
                ["ar"] = "تم تحديث التخصص الطبي بنجاح",
            },
            ["MEDICAL_SPECIALITY_DELETED_SUCCESS"] = new()
            {
                ["en"] = "Medical speciality deleted successfully",
                ["ar"] = "تم حذف التخصص الطبي بنجاح",
            },
            ["MEDICAL_SPECIALITY_EXISTS_CHECK_SUCCESS"] = new()
            {
                ["en"] = "Medical speciality existence check completed successfully",
                ["ar"] = "تم إنجاز فحص وجود التخصص الطبي بنجاح",
            },

            // Patient Authentication Success Messages
            ["PATIENT_REGISTERED_SUCCESS"] = new()
            {
                ["en"] = "Patient registered successfully",
                ["ar"] = "تم تسجيل المريض بنجاح",
            },
            ["OTP_VERIFIED_SUCCESS"] = new()
            {
                ["en"] = "OTP verified successfully",
                ["ar"] = "تم التحقق من رمز التأكيد بنجاح",
            },
            ["LOGIN_SUCCESS"] = new()
            {
                ["en"] = "Login successful",
                ["ar"] = "تم تسجيل الدخول بنجاح",
            },
            ["PHONE_CHECK_SUCCESS"] = new()
            {
                ["en"] = "Phone number check completed successfully",
                ["ar"] = "تم فحص رقم الهاتف بنجاح",
            },
            ["USERNAME_CHECK_SUCCESS"] = new()
            {
                ["en"] = "Username check completed successfully",
                ["ar"] = "تم فحص اسم المستخدم بنجاح",
            },

            // Error Messages
            ["GOVERNORATE_NOT_FOUND"] = new()
            {
                ["en"] = "Governorate not found",
                ["ar"] = "المحافظة غير موجودة",
            },
            ["GOVERNORATE_NAME_ALREADY_EXISTS"] = new()
            {
                ["en"] = "A governorate with this name already exists",
                ["ar"] = "محافظة بهذا الاسم موجودة بالفعل",
            },
            ["GOVERNORATE_HAS_ACTIVE_CITIES"] = new()
            {
                ["en"] = "Cannot delete governorate that has active cities",
                ["ar"] = "لا يمكن حذف المحافظة التي تحتوي على مدن نشطة",
            },
            ["GOVERNORATE_CREATION_FAILED"] = new()
            {
                ["en"] = "Failed to create the governorate",
                ["ar"] = "فشل في إنشاء المحافظة",
            },
            ["GOVERNORATE_UPDATE_FAILED"] = new()
            {
                ["en"] = "Failed to update the governorate",
                ["ar"] = "فشل في تحديث المحافظة",
            },
            ["GOVERNORATE_DELETION_FAILED"] = new()
            {
                ["en"] = "Failed to delete the governorate",
                ["ar"] = "فشل في حذف المحافظة",
            },
            ["CITY_NOT_FOUND"] = new() { ["en"] = "City not found", ["ar"] = "المدينة غير موجودة" },
            ["CITY_NAME_ALREADY_EXISTS"] = new()
            {
                ["en"] = "A city with this name already exists in the governorate",
                ["ar"] = "مدينة بهذا الاسم موجودة بالفعل في المحافظة",
            },
            ["CITY_CREATION_FAILED"] = new()
            {
                ["en"] = "Failed to create the city",
                ["ar"] = "فشل في إنشاء المدينة",
            },
            ["CITY_UPDATE_FAILED"] = new()
            {
                ["en"] = "Failed to update the city",
                ["ar"] = "فشل في تحديث المدينة",
            },
            ["CITY_DELETION_FAILED"] = new()
            {
                ["en"] = "Failed to delete the city",
                ["ar"] = "فشل في حذف المدينة",
            },
            ["MEDICAL_SPECIALITY_NOT_FOUND"] = new()
            {
                ["en"] = "Medical speciality not found",
                ["ar"] = "التخصص الطبي غير موجود",
            },
            ["MEDICAL_SPECIALITY_NAME_ALREADY_EXISTS"] = new()
            {
                ["en"] = "A medical speciality with this name already exists",
                ["ar"] = "تخصص طبي بهذا الاسم موجود بالفعل",
            },
            ["MEDICAL_SPECIALITY_CREATION_FAILED"] = new()
            {
                ["en"] = "Failed to create the medical speciality",
                ["ar"] = "فشل في إنشاء التخصص الطبي",
            },
            ["MEDICAL_SPECIALITY_UPDATE_FAILED"] = new()
            {
                ["en"] = "Failed to update the medical speciality",
                ["ar"] = "فشل في تحديث التخصص الطبي",
            },
            ["MEDICAL_SPECIALITY_DELETION_FAILED"] = new()
            {
                ["en"] = "Failed to delete the medical speciality",
                ["ar"] = "فشل في حذف التخصص الطبي",
            },
            ["MEDICAL_SPECIALITY_RETRIEVAL_FAILED"] = new()
            {
                ["en"] = "Failed to retrieve the medical speciality",
                ["ar"] = "فشل في استرجاع التخصص الطبي",
            },
            ["MEDICAL_SPECIALITIES_RETRIEVAL_FAILED"] = new()
            {
                ["en"] = "Failed to retrieve medical specialities",
                ["ar"] = "فشل في استرجاع التخصصات الطبية",
            },
            ["MEDICAL_SPECIALITY_EXISTS_CHECK_FAILED"] = new()
            {
                ["en"] = "Failed to check medical speciality existence",
                ["ar"] = "فشل في فحص وجود التخصص الطبي",
            },
            ["VALIDATION_ERROR"] = new()
            {
                ["en"] = "Validation failed",
                ["ar"] = "فشل في التحقق من البيانات",
            },
            ["INTERNAL_ERROR"] = new()
            {
                ["en"] = "An internal error occurred",
                ["ar"] = "حدث خطأ داخلي",
            },
            ["OPERATION_FAILED"] = new() { ["en"] = "Operation failed", ["ar"] = "فشلت العملية" },
            ["INVALID_PHONE_NUMBER"] = new()
            {
                ["en"] = "Invalid phone number",
                ["ar"] = "رقم الهاتف غير صحيح",
            },
            // Patient Authentication Error Messages
            ["INVALID_REGISTRATION_DATA"] = new()
            {
                ["en"] = "Invalid registration data provided",
                ["ar"] = "بيانات التسجيل المقدمة غير صحيحة",
            },
            ["PHONE_NUMBER_ALREADY_EXISTS"] = new()
            {
                ["en"] = "A user with this phone number already exists",
                ["ar"] = "مستخدم بهذا الرقم موجود بالفعل",
            },
            ["USERNAME_ALREADY_EXISTS"] = new()
            {
                ["en"] = "A user with this username already exists",
                ["ar"] = "مستخدم بهذا الاسم موجود بالفعل",
            },
            ["PATIENT_REGISTRATION_FAILED"] = new()
            {
                ["en"] = "Failed to register patient",
                ["ar"] = "فشل في تسجيل المريض",
            },
            ["INVALID_OTP_DATA"] = new()
            {
                ["en"] = "Invalid OTP data provided",
                ["ar"] = "بيانات رمز التأكيد غير صحيحة",
            },
            ["INVALID_OTP"] = new()
            {
                ["en"] = "Invalid or expired OTP",
                ["ar"] = "رمز التأكيد غير صحيح أو منتهي الصلاحية",
            },
            ["USER_NOT_FOUND"] = new() { ["en"] = "User not found", ["ar"] = "المستخدم غير موجود" },
            ["PATIENT_NOT_FOUND"] = new()
            {
                ["en"] = "Patient not found",
                ["ar"] = "المريض غير موجود",
            },
            ["OTP_VERIFICATION_FAILED"] = new()
            {
                ["en"] = "Failed to verify OTP",
                ["ar"] = "فشل في التحقق من رمز التأكيد",
            },
            ["INVALID_LOGIN_DATA"] = new()
            {
                ["en"] = "Invalid login data provided",
                ["ar"] = "بيانات تسجيل الدخول غير صحيحة",
            },
            ["INVALID_CREDENTIALS"] = new()
            {
                ["en"] = "Invalid phone number or password",
                ["ar"] = "رقم الهاتف أو كلمة المرور غير صحيحة",
            },
            ["PHONE_NOT_VERIFIED"] = new()
            {
                ["en"] = "Phone number is not verified. Please verify your phone number first.",
                ["ar"] = "رقم الهاتف غير مؤكد. يرجى تأكيد رقم الهاتف أولاً",
            },
            ["ACCOUNT_INACTIVE"] = new()
            {
                ["en"] = "Account is inactive. Please contact support.",
                ["ar"] = "الحساب غير نشط. يرجى التواصل مع الدعم الفني",
            },
            ["LOGIN_FAILED"] = new() { ["en"] = "Failed to login", ["ar"] = "فشل في تسجيل الدخول" },
            ["INVALID_PHONE_NUMBER"] = new()
            {
                ["en"] = "Invalid phone number",
                ["ar"] = "رقم الهاتف غير صحيح",
            },
            ["PHONE_CHECK_FAILED"] = new()
            {
                ["en"] = "Failed to check phone number",
                ["ar"] = "فشل في فحص رقم الهاتف",
            },
            ["INVALID_USERNAME"] = new()
            {
                ["en"] = "Invalid username",
                ["ar"] = "اسم المستخدم غير صحيح",
            },
            ["USERNAME_CHECK_FAILED"] = new()
            {
                ["en"] = "Failed to check username",
                ["ar"] = "فشل في فحص اسم المستخدم",
            },

            // Admin Authentication Messages
            ["ADMIN_LOGIN_SUCCESS"] = new()
            {
                ["en"] = "Admin login successful",
                ["ar"] = "تم تسجيل دخول المسؤول بنجاح",
            },
            ["ADMIN_LOGIN_FAILED"] = new()
            {
                ["en"] = "Failed to login admin",
                ["ar"] = "فشل في تسجيل دخول المسؤول",
            },
            ["ADMIN_PROFILE_RETRIEVED_SUCCESS"] = new()
            {
                ["en"] = "Admin profile retrieved successfully",
                ["ar"] = "تم استرجاع ملف المسؤول الشخصي بنجاح",
            },
            ["ADMIN_PROFILE_RETRIEVAL_FAILED"] = new()
            {
                ["en"] = "Failed to retrieve admin profile",
                ["ar"] = "فشل في استرجاع ملف المسؤول الشخصي",
            },
            ["ADMIN_PROFILE_UPDATED_SUCCESS"] = new()
            {
                ["en"] = "Admin profile updated successfully",
                ["ar"] = "تم تحديث ملف المسؤول الشخصي بنجاح",
            },
            ["ADMIN_PROFILE_UPDATE_FAILED"] = new()
            {
                ["en"] = "Failed to update admin profile",
                ["ar"] = "فشل في تحديث ملف المسؤول الشخصي",
            },
            ["ADMIN_PASSWORD_CHANGED_SUCCESS"] = new()
            {
                ["en"] = "Admin password changed successfully",
                ["ar"] = "تم تغيير كلمة مرور المسؤول بنجاح",
            },
            ["ADMIN_PASSWORD_CHANGE_FAILED"] = new()
            {
                ["en"] = "Failed to change admin password",
                ["ar"] = "فشل في تغيير كلمة مرور المسؤول",
            },
            ["ADMIN_NOT_FOUND"] = new()
            {
                ["en"] = "Admin not found",
                ["ar"] = "المسؤول غير موجود",
            },
            ["ADMIN_EMAIL_ALREADY_EXISTS"] = new()
            {
                ["en"] = "An admin with this email already exists",
                ["ar"] = "مسؤول بهذا البريد الإلكتروني موجود بالفعل",
            },
            ["ADMIN_EMAIL_CHECK_SUCCESS"] = new()
            {
                ["en"] = "Admin email check completed successfully",
                ["ar"] = "تم إنجاز فحص البريد الإلكتروني للمسؤول بنجاح",
            },
            ["ADMIN_EMAIL_CHECK_FAILED"] = new()
            {
                ["en"] = "Failed to check admin email",
                ["ar"] = "فشل في فحص البريد الإلكتروني للمسؤول",
            },
            ["INVALID_ADMIN_ID"] = new()
            {
                ["en"] = "Invalid admin ID",
                ["ar"] = "معرف المسؤول غير صحيح",
            },
            ["INVALID_EMAIL"] = new()
            {
                ["en"] = "Invalid email address",
                ["ar"] = "عنوان البريد الإلكتروني غير صحيح",
            },
            ["INVALID_UPDATE_DATA"] = new()
            {
                ["en"] = "Invalid update data provided",
                ["ar"] = "بيانات التحديث المقدمة غير صحيحة",
            },
            ["INVALID_PASSWORD_CHANGE_DATA"] = new()
            {
                ["en"] = "Invalid password change data provided",
                ["ar"] = "بيانات تغيير كلمة المرور المقدمة غير صحيحة",
            },
            ["CURRENT_PASSWORD_REQUIRED"] = new()
            {
                ["en"] = "Current password is required",
                ["ar"] = "كلمة المرور الحالية مطلوبة",
            },
            ["INVALID_CURRENT_PASSWORD"] = new()
            {
                ["en"] = "Current password is incorrect",
                ["ar"] = "كلمة المرور الحالية غير صحيحة",
            },
            ["PASSWORD_CONFIRMATION_MISMATCH"] = new()
            {
                ["en"] = "Password confirmation does not match",
                ["ar"] = "تأكيد كلمة المرور غير متطابق",
            },
            ["INVALID_TOKEN"] = new()
            {
                ["en"] = "Invalid or expired token",
                ["ar"] = "رمز غير صحيح أو منتهي الصلاحية",
            },
        };
    }
}
