using JobHunter.API.DTOs;
using Microsoft.AspNetCore.Http;

namespace JobHunter.API.Services;

public interface ICvService
{
    Task<CvSummaryDto> TaoCvTrucTuyenAsync(int maTkUv, TaoCvTrucTuyenRequest request);
    Task<CvSummaryDto> UploadCvAsync(int maTkUv, string tenCv, IFormFile file);
    Task<List<CvSummaryDto>> LayDanhSachCuaToiAsync(int maTkUv);
}
