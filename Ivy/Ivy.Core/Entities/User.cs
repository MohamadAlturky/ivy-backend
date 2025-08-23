namespace Ivy.Core.Entities;

public class User : BaseEntity<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public bool IsPhoneVerified { get; set; }
    public DateTime DateOfBirth { get; set; }
}

public enum Gender
{
    Male = 1,
    Female = 2,
    NotSpecified = 3,
}
