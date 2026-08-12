using JobHunter.API.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace JobHunter.Tests;

// Test JwtService THAT (khong qua Fake) cho 2 ham ky/xac minh token
// UC03/UC06 - chung minh co che HMAC thuc su hoat dong dung, khong chi
// gia dinh trong thiet ke. Xem docs/superpowers/specs/2026-08-12-smtp-email-design.md
public class JwtServiceTests
{
    private static JwtService NewService()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "test-secret-key-du-dai-cho-hmac-sha256-32ky-tu-tro-len",
                ["Jwt:Issuer"] = "JobHunterAPI",
                ["Jwt:Audience"] = "JobHunterClient",
            })
            .Build();
        return new JwtService(config);
    }

    [Fact]
    [Trait("Category", "Security")]
    public void KyRoiVerify_CungDuLieu_TraVeDung()
    {
        var jwt = NewService();
        var hanDung = DateTime.UtcNow.AddMinutes(15);

        var chuKy = jwt.KyTokenMucDich(42, "XacThucEmail", hanDung);

        Assert.True(jwt.XacMinhTokenMucDich(chuKy, 42, "XacThucEmail", hanDung));
    }

    [Fact]
    [Trait("Category", "Security")]
    public void ChuKyBiSuaMotKyTu_TraVeSai()
    {
        var jwt = NewService();
        var hanDung = DateTime.UtcNow.AddMinutes(15);
        var chuKy = jwt.KyTokenMucDich(42, "XacThucEmail", hanDung);

        var chuKySai = chuKy[..^1] + (chuKy[^1] == 'a' ? 'b' : 'a'); // doi ky tu cuoi

        Assert.False(jwt.XacMinhTokenMucDich(chuKySai, 42, "XacThucEmail", hanDung));
    }

    [Fact]
    [Trait("Category", "Security")]
    public void ChuKyDungChoDuLieuKhac_TraVeSai()
    {
        // Day chinh la kich ban tan cong: biet 1 token hop le (vd cua chinh
        // minh, MaToken=42) roi thu doi sang MaToken=43 (doan tuan tu, chiem
        // tai khoan nguoi khac) - phai bi tu choi vi chu ky khong con khop.
        var jwt = NewService();
        var hanDung = DateTime.UtcNow.AddMinutes(15);
        var chuKyCuaToken42 = jwt.KyTokenMucDich(42, "XacThucEmail", hanDung);

        Assert.False(jwt.XacMinhTokenMucDich(chuKyCuaToken42, 43, "XacThucEmail", hanDung));
    }

    [Fact]
    [Trait("Category", "Security")]
    public void ChuKyDoDaiKhacNhau_TraVeSaiKhongCrash()
    {
        var jwt = NewService();
        var hanDung = DateTime.UtcNow.AddMinutes(15);

        Assert.False(jwt.XacMinhTokenMucDich("chuoi-qua-ngan", 42, "XacThucEmail", hanDung));
    }
}
