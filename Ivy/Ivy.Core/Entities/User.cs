namespace Ivy.Core.Entities;

public class User : BaseEntity<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public UserType UserType { get; set; } = UserType.Patient;
    public bool IsPhoneVerified { get; set; }
    public DateTime DateOfBirth { get; set; }
}

public enum Gender
{
    Male = 1,
    Female = 2,
    NotSpecified = 3,
}

public enum UserType
{
    Clinic = 1,
    Doctor = 2,
    Patient = 3,
}
