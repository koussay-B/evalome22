using api.services.CloudinaryService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    /// <summary>
    /// General-purpose image / file upload endpoint.
    /// Returns a Cloudinary secure URL for the uploaded file.
    /// </summary>
    [Authorize]
    public class UploadController(ICloudService cloudService) : BaseApiController
    {
        private readonly ICloudService _cloudService = cloudService;

        // POST /api/upload/image
        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file provided.");

            if (!file.ContentType.StartsWith("image/"))
                return BadRequest("Only image files are accepted.");

            if (file.Length > 5 * 1024 * 1024) // 5 MB hard cap
                return BadRequest("File size must be under 5 MB.");

            await using var stream = file.OpenReadStream();
            var url = await _cloudService.UploadFileAsync(stream, file.FileName, "candidatapp/uploads");

            return Ok(new { url });
        }
    }
}
