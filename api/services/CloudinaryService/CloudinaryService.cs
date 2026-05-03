using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace api.services.CloudinaryService
{
    public class CloudinaryService : ICloudService
    {
        private readonly Cloudinary _cloudinary;
        public CloudinaryService(IOptions<CloudinarySettings> cloudinarySettings)
        {
            var settings = cloudinarySettings.Value ?? throw new ArgumentNullException(nameof(cloudinarySettings));

            var account = new Account(
                settings.CloudName,
                settings.ApiKey,
                settings.ApiSecret);

            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true; // Ensure HTTPS is used
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string? fileName = null, string folder = "uploads")
        {
            if (fileStream == null || fileStream.Length == 0)
            {
                throw new ArgumentException("File stream cannot be null or empty.", nameof(fileStream));
            }

            try
            {

                string sanitizedFileName = SanitizeFileName(fileName ?? "file");
                string fileExtension = Path.GetExtension(fileName)?.ToLower(CultureInfo.InvariantCulture) ?? "";
                string uniqueFileName = $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}_{sanitizedFileName}{fileExtension}";

                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(uniqueFileName, fileStream),
                    Folder = folder,
                    PublicId = uniqueFileName,
                    Overwrite = true // Overwrite if the file already exists
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    throw new Exception($"Cloudinary upload failed: {uploadResult.Error.Message}");
                }

                return uploadResult.SecureUrl.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to upload file to Cloudinary: {ex.Message}", ex);
            }
        }
        private string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return "file";

            string nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string sanitized = Regex.Replace(nameWithoutExtension, "[^a-zA-Z0-9_-]", "_");
            return sanitized.ToLower(CultureInfo.InvariantCulture);
        }
    }
}