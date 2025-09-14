namespace Ivy.Api.Controllers;

public class BookingFlowController : BaseController
{
    public BookingFlowController(
        IApiResponseRepresenter responseRepresenter,
        ILogger<BookingFlowController> logger
    )
        : base(responseRepresenter, logger)
    {
    }
    [HttpGet("doctors")]
    public async Task<ActionResult<ApiResponse<DocktorsListingResultDto>>> GetDoctorsListing()
    {
        await Task.Delay(1000);
        var result = Result<DocktorsListingResultDto>.Ok("DOCTORS_LISTING_FETCHED", new DocktorsListingResultDto()
        {
            Count = 10,
            Doctors = [
                new DoctorListingDto() { Id = 1, Name = "John Doe", About = "About John Doe", ProfileImageUrl = "https://via.placeholder.com/150", Rating = 4.5M, Gender = Gender.Male   },
                new DoctorListingDto() { Id = 2, Name = "Jsmith", About = "About Jsmith", ProfileImageUrl = "https://via.placeholder.com/150", Rating = 4.5M, Gender = Gender.Male   },
            ]
        });
        return HandleResult(result);
    }
    [HttpGet("doctor/{id}")]
    public async Task<ActionResult<ApiResponse<DocktorDetailsDto>>> GetDoctorDetails(int id)
    {
        await Task.Delay(1000);
        var result = Result<DocktorDetailsDto>.Ok("DOCTOR_DETAILS_FETCHED",
            new DocktorDetailsDto()
            {
                Id = id,
                Name = "John Doe",
                About = "About John Doe",
                ProfileImageUrl = "https://via.placeholder.com/150", Rating = 4.5M, Specializations = ["Cardiology", "Pediatrics"], Hospitals = [new HospitalsDtos() { Id = 1, Address = "123 Main St", Lat = 37.774929m, Lng = 47.419416m, ImageUrl = "https://via.placeholder.com/150" }] });
        return HandleResult(result);
    }

}

public class DoctorListingDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string About { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public string ProfileImageUrl { get; set; } = string.Empty;
    public decimal Rating { get; set; }
}

public class DocktorsListingResultDto
{
    public int Count { get; set; }
    public List<DoctorListingDto> Doctors { get; set; } = [];
}

public class DocktorDetailsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string About { get; set; } = string.Empty;
    public string ProfileImageUrl { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public List<string> Specializations { get; set; } = [];
    public List<HospitalsDtos> Hospitals { get; set; } = [];
}

public class HospitalsDtos
{
    public int Id { get; set; }
    public string Address { get; set; } = string.Empty;
    public decimal Lat { get; set; }
    public decimal Lng { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}