using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using JobHunter.API.Models;
using Microsoft.IdentityModel.Tokens;

namespace JobHunter.API.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(TaiKhoan taiKhoan, string displayName)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, taiKhoan.MaTK.ToString()),
            new(ClaimTypes.Role, taiKhoan.VaiTro),
            new(ClaimTypes.Email, taiKhoan.Email),
        };
        if (taiKhoan.VaiTro == "NhaTuyenDung")
            claims.Add(new Claim("tencongty", displayName));
        else
            claims.Add(new Claim("hoten", displayName));

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(120),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // UC03/UC06 - token xac thuc email/dat lai mat khau tu ky bang HMAC,
    // KHONG luu them cot DB (schema Lab 3 da nop, khong doi). Chu ky bao
    // dam "MaToken" (INT tu tang, doan duoc) khong the bi gia mao neu
    // khong biet Jwt:Key - xem chi tiet trong
    // docs/superpowers/specs/2026-08-12-smtp-email-design.md
    public string KyTokenMucDich(int maToken, string loaiToken, DateTime thoiHanHetHan)
    {
        // SpecifyKind(Utc) truoc khi format ":O" - BAT BUOC, vi format "O"
        // phu thuoc DateTimeKind (them hau to "Z" neu Kind=Utc, khong them
        // gi neu Kind=Unspecified). Luc ky (object moi tao, DateTime.UtcNow)
        // Kind=Utc, nhung luc verify (doc lai tu SQL Server qua EF Core)
        // Kind luon la Unspecified - neu khong ep lai, 2 chuoi khac nhau ->
        // chu ky khong bao gio khop du dung Jwt:Key. Bug that da gap
        // 2026-08-12 (moi link xac thuc email/dat lai mat khau deu bao loi).
        var utc = DateTime.SpecifyKind(thoiHanHetHan, DateTimeKind.Utc);
        var data = $"{maToken}|{loaiToken}|{utc:O}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    public bool XacMinhTokenMucDich(string chuKy, int maToken, string loaiToken, DateTime thoiHanHetHan)
    {
        var chuKyDung = KyTokenMucDich(maToken, loaiToken, thoiHanHetHan);
        // FixedTimeEquals doi 2 mang cung do dai - kiem truoc de tranh
        // exception (do dai khac nhau tu no da la "khong khop" roi).
        var a = Encoding.UTF8.GetBytes(chuKy);
        var b = Encoding.UTF8.GetBytes(chuKyDung);
        if (a.Length != b.Length) return false;
        return CryptographicOperations.FixedTimeEquals(a, b);
    }
}
