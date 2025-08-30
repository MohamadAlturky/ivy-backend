namespace Ivy.Core.Entities;

public class Doctor
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string ProfileImageUrl { get; set; } = null!;
    public bool IsProfileCompleted { get; set; }
}
