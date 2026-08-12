using JobHunter.API.Models;

namespace JobHunter.API.Services;

public interface IJwtService
{
    string GenerateToken(TaiKhoan taiKhoan, string displayName);

    // UC03/UC06 - ky/xac minh token xac thuc email & dat lai mat khau bang
    // HMAC (dung lai Jwt:Key) thay vi luu them cot DB - xem
    // docs/superpowers/specs/2026-08-12-smtp-email-design.md
    string KyTokenMucDich(int maToken, string loaiToken, DateTime thoiHanHetHan);
    bool XacMinhTokenMucDich(string chuKy, int maToken, string loaiToken, DateTime thoiHanHetHan);
}
