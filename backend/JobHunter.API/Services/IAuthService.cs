using JobHunter.API.DTOs;

namespace JobHunter.API.Services;

public interface IAuthService
{
    Task<DangKyResponse> DangKyUngVienAsync(DangKyUngVienRequest request);
    Task<DangKyResponse> DangKyNhaTuyenDungAsync(DangKyNhaTuyenDungRequest request);
    Task<LoginResponse> DangNhapAsync(LoginRequest request);
    Task VerifyEmailAsync(string token);
    Task ResendVerificationAsync(string email);
    Task ForgotPasswordAsync(string email);
    Task ResetPasswordAsync(string token, string matKhauMoi, string xacNhanMatKhauMoi);
}
