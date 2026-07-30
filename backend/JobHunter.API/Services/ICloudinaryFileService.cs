using Microsoft.AspNetCore.Http;

namespace JobHunter.API.Services;

public interface ICloudinaryFileService
{
    Task<string> UploadRawAsync(IFormFile file, string publicIdPrefix);
    Task<string> UploadImageAsync(IFormFile file, string publicIdPrefix);
}
