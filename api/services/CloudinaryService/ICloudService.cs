using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.services.CloudinaryService
{
    public interface ICloudService
    {
        /// <summary>
        /// Uploads a file to Cloudinary and returns the secure URL.
        /// </summary>
        /// <param name="fileStream">The stream of the file to upload.</param>
        /// <param name="fileName">The original name of the file (optional, for public ID).</param>
        /// <param name="folder">The folder in Cloudinary to store the file (optional).</param>
        /// <returns>The secure URL of the uploaded file.</returns>
        /// <exception cref="Exception">Thrown if the upload fails.</exception>
        Task<string> UploadFileAsync(Stream fileStream, string? fileName = null, string folder = "uploads");
    }
}