using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
}
