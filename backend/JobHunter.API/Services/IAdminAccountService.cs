using JobHunter.API.DTOs;

namespace JobHunter.API.Services;

public interface IAdminAccountService
{
    Task<List<AdminAccountDto>> LayDanhSachAsync(string? vaiTro);
    Task<string> KhoaTaiKhoanAsync(int maTk, string lyDo); // BR18, MS47/MS48/MS55
    Task MoKhoaTaiKhoanAsync(int maTk);
}
