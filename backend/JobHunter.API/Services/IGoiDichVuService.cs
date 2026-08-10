using JobHunter.API.DTOs;

namespace JobHunter.API.Services;

public interface IGoiDichVuService
{
    Task<List<GoiDichVuDto>> LayDanhSachAdminAsync();
    Task<GoiDichVuDto> ThemGoiAsync(GoiDichVuUpsertRequest request);
    Task<GoiDichVuDto> SuaGoiAsync(int maGoi, GoiDichVuUpsertRequest request);
    Task<string> XoaGoiAsync(int maGoi);
    Task<DanhSachGoiResponse> LayDanhSachChoNtdAsync(int maTkNtd);
    Task<MuaGoiResponse> MuaGoiAsync(int maTkNtd, int maGoi, MuaGoiRequest request);
    Task<int> LayGioiHanHieuLucAsync(int maTkNtd);
}
