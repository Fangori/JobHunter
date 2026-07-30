using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Models;
using JobHunter.API.Services;
using Xunit;

namespace JobHunter.Tests;

file class FakeJwtService : IJwtService
{
    public string GenerateToken(TaiKhoan taiKhoan, string displayName) => "fake-token";
}

public class AuthServiceTests
{
    private static AuthService NewService(out JobHunter.API.Data.JobHunterDbContext db)
    {
        db = TestHelpers.NewInMemoryDb();
        return new AuthService(db, new ThamSoService(db), new FakeJwtService());
    }

    [Fact]
    [Trait("Category", "QD01")]
    public async Task DangKy_EmailTrung_ThatBai()
    {
        var service = NewService(out _);
        var req = new DangKyUngVienRequest { HoTen = "A", Email = "a@test.local", MatKhau = "Test1234", XacNhanMatKhau = "Test1234" };
        await service.DangKyUngVienAsync(req);

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.DangKyUngVienAsync(req));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    [Trait("Category", "QD01")]
    public async Task DangKy_MatKhauNganHonTS1_ThatBai()
    {
        var service = NewService(out _);
        var req = new DangKyUngVienRequest { HoTen = "A", Email = "b@test.local", MatKhau = "abc123", XacNhanMatKhau = "abc123" }; // 6 ky tu, co so

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.DangKyUngVienAsync(req));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    [Trait("Category", "QD01")]
    public async Task DangKy_MatKhauKhongCoChuSo_ThatBai()
    {
        var service = NewService(out _);
        var req = new DangKyUngVienRequest { HoTen = "A", Email = "c@test.local", MatKhau = "abcdefgh", XacNhanMatKhau = "abcdefgh" }; // du dai, khong so

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.DangKyUngVienAsync(req));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    [Trait("Category", "QD03")]
    public async Task DangNhap_SaiDuSoLanToiDa_BiKhoaTam()
    {
        var service = NewService(out var db);
        await service.DangKyUngVienAsync(new DangKyUngVienRequest { HoTen = "A", Email = "d@test.local", MatKhau = "Test1234", XacNhanMatKhau = "Test1234" });

        BusinessRuleException? last = null;
        for (var i = 0; i < 5; i++)
        {
            last = await Assert.ThrowsAsync<BusinessRuleException>(() =>
                service.DangNhapAsync(new LoginRequest { Email = "d@test.local", MatKhau = "saimatkhau" }));
        }

        Assert.Equal(403, last!.StatusCode); // lan thu 5 phai la MS02, khong con la MS01
        var taiKhoan = db.TaiKhoans.Single(x => x.Email == "d@test.local");
        Assert.NotNull(taiKhoan.KhoaTamThoiDenLuc);
        Assert.True(taiKhoan.KhoaTamThoiDenLuc > DateTime.UtcNow);
    }

    [Fact]
    [Trait("Category", "QD03")]
    public async Task DangNhap_DungMatKhauNhungDangKhoaTam_VanThatBai()
    {
        var service = NewService(out _);
        await service.DangKyUngVienAsync(new DangKyUngVienRequest { HoTen = "A", Email = "e@test.local", MatKhau = "Test1234", XacNhanMatKhau = "Test1234" });

        for (var i = 0; i < 5; i++)
        {
            await Assert.ThrowsAsync<BusinessRuleException>(() =>
                service.DangNhapAsync(new LoginRequest { Email = "e@test.local", MatKhau = "saimatkhau" }));
        }

        // Dung mat khau nhung van dang trong 15 phut khoa tam
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.DangNhapAsync(new LoginRequest { Email = "e@test.local", MatKhau = "Test1234" }));
        Assert.Equal(403, ex.StatusCode);
    }
}
