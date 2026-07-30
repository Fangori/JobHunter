using JobHunter.API.DTOs;
using Microsoft.AspNetCore.Http;

namespace JobHunter.API.Services;

public interface ICvService
{
    Task<CvSummaryDto> TaoCvTrucTuyenAsync(int maTkUv, TaoCvTrucTuyenRequest request);
    Task<CvSummaryDto> UploadCvAsync(int maTkUv, string tenCv, IFormFile file);
    Task<List<CvSummaryDto>> LayDanhSachCuaToiAsync(int maTkUv, string trangThai = "HoatDong");
    Task<CvDetailDto> LayChiTietAsync(int maTkUv, int maCv);
    Task<CvSummaryDto> SuaCvTrucTuyenAsync(int maTkUv, int maCv, TaoCvTrucTuyenRequest request); // MS37
    Task<string> XoaCvAsync(int maTkUv, int maCv); // BR13, MS38/MS39
    Task PhucHoiCvAsync(int maTkUv, int maCv); // MS40
}
