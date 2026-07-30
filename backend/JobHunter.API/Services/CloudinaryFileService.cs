using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using JobHunter.API.Exceptions;
using Microsoft.AspNetCore.Http;

namespace JobHunter.API.Services;

public class CloudinaryFileService : ICloudinaryFileService
{
    private readonly Cloudinary? _cloudinary;

    public CloudinaryFileService(IConfiguration config)
    {
        var cloudName = config["Cloudinary:CloudName"];
        var apiKey = config["Cloudinary:ApiKey"];
        var apiSecret = config["Cloudinary:ApiSecret"];
        if (!string.IsNullOrEmpty(cloudName) && !string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(apiSecret))
        {
            _cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
        }
    }

    public async Task<string> UploadRawAsync(IFormFile file, string publicIdPrefix)
    {
        if (_cloudinary is null)
            throw new BusinessRuleException(500, "Chưa cấu hình Cloudinary (Cloudinary:CloudName/ApiKey/ApiSecret) — không thể tải file lên.");

        await using var stream = file.OpenReadStream();
        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            PublicId = $"{publicIdPrefix}/{Guid.NewGuid()}_{file.FileName}",
        };
        var result = await _cloudinary.UploadAsync(uploadParams);
        if (result.Error is not null)
            throw new BusinessRuleException(500, $"Lỗi Cloudinary: {result.Error.Message}");
        if (result.SecureUrl is null)
            throw new BusinessRuleException(500, $"Cloudinary không trả về URL (status: {result.StatusCode}).");
        return result.SecureUrl.ToString();
    }
}
