using JobHunter.API.Models;

namespace JobHunter.API.Services;

public interface IJwtService
{
    string GenerateToken(TaiKhoan taiKhoan, string displayName);
}
