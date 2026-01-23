using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles; // For MIME type resolution

namespace Ivy.Api.Controllers;

[Route("api/files")]
[ApiController]
public class FilesController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;

    // Inject IWebHostEnvironment to access the wwwroot path
    public FilesController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    // POST: api/files/upload
    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        // 1. Create a safe file name (prevents path traversal)
        var fileName = Path.GetFileName(file.FileName);
        // 2. Define the path: wwwroot/uploads/
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");

        // 3. Ensure the directory exists
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var filePath = Path.Combine(uploadsFolder, fileName);

        // 4. Save the file to the server
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Return the URL to access the file
        var fileUrl = $"{Request.Scheme}://{Request.Host}/api/files/uploads/{fileName}";

        return Ok(new { Message = "File uploaded successfully", Url = fileUrl });
    }

    // GET: api/files/download/{fileName}
    [HttpGet("uploads/{fileName}")]
    public IActionResult GetFile(string fileName)
    {
        // 1. Construct the full path
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
        var filePath = Path.Combine(uploadsFolder, fileName);
        Console.WriteLine(filePath);

        // 2. Check if file exists
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("File not found.");
        }

        // 3. Determine the content type (MIME type)
        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(filePath, out var contentType))
        {
            contentType = "application/octet-stream"; // Default binary type
        }

        // 4. Return the file
        // returning PhysicalFile reads directly from disk
        return PhysicalFile(filePath, contentType, fileName);
    }
}