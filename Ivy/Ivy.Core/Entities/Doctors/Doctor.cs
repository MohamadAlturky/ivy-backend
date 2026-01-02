namespace Ivy.Core.Entities;

public class Doctor
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string ProfileImageUrl { get; set; } = string.Empty;
    public string DisplayNameAr { get; set; } = string.Empty;
    public string DisplayNameEn { get; set; } = string.Empty;
    public double Rating { get; set; } = 3;
    public ICollection<DoctorDynamicProfileHistory> DoctorDynamicProfileHistories { get; set; } =
        [];
}
