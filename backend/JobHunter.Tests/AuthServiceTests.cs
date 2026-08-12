using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Models;
using JobHunter.API.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace JobHunter.Tests;

// Fake nhung van kiem that (khong chi tra ve hang so co dinh) de test con
// y nghia - "ky" = noi 3 tham so, "verify" = so khop lai dung chuoi do.
// Khong can Jwt:Key that, khac voi JwtServiceTests.cs (test bang JwtService
// that, dung HMAC that).
file class FakeJwtService : IJwtService
{
    public string GenerateToken(TaiKhoan taiKhoan, string displayName) => "fake-token";

    public string KyTokenMucDich(int maToken, string loaiToken, DateTime thoiHanHetHan)
        => $"{maToken}|{loaiToken}|{thoiHanHetHan:O}";

    public bool XacMinhTokenMucDich(string chuKy, int maToken, string loaiToken, DateTime thoiHanHetHan)
        => chuKy == KyTokenMucDich(maToken, loaiToken, thoiHanHetHan);
}

file class FakeEmailService : IEmailService
{
    public List<(string ToEmail, string TokenValue)> XacThucDaGui { get; } = new();
    public List<(string ToEmail, string TokenValue)> DatLaiMatKhauDaGui { get; } = new();

    public Task GuiXacThucEmailAsync(string toEmail, string tokenValue)
    {
        XacThucDaGui.Add((toEmail, tokenValue));
        return Task.CompletedTask;
    }

    public Task GuiDatLaiMatKhauAsync(string toEmail, string tokenValue)
    {
        DatLaiMatKhauDaGui.Add((toEmail, tokenValue));
        return Task.CompletedTask;
    }
}

public class AuthServiceTests
{
    private static AuthService NewService(out JobHunter.API.Data.JobHunterDbContext db)
        => NewService(out db, out _);

    // "IJwtService" (khong phai FakeJwtService file-local) o kieu tra ve -
    // C# khong cho type file-local xuat hien trong signature cua thanh vien
    // thuoc class KHONG file-local (AuthServiceTests o day).
    private static AuthService NewService(out JobHunter.API.Data.JobHunterDbContext db, out IJwtService jwt)
    {
        db = TestHelpers.NewInMemoryDb();
        jwt = new FakeJwtService();
        return new AuthService(db, new ThamSoService(db), jwt, new FakeEmailService());
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

    [Fact]
    [Trait("Category", "UC03")]
    public async Task DangNhap_ChuaXacThucEmail_ThatBai()
    {
        var service = NewService(out _);
        await service.DangKyUngVienAsync(new DangKyUngVienRequest { HoTen = "A", Email = "f@test.local", MatKhau = "Test1234", XacNhanMatKhau = "Test1234" });

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.DangNhapAsync(new LoginRequest { Email = "f@test.local", MatKhau = "Test1234" }));
        Assert.Equal(403, ex.StatusCode); // MS10
    }

    [Fact]
    [Trait("Category", "UC03")]
    public async Task DangNhap_SaiMatKhauVaChuaXacThuc_UuTienBaoSaiMatKhau()
    {
        var service = NewService(out _);
        await service.DangKyUngVienAsync(new DangKyUngVienRequest { HoTen = "A", Email = "g@test.local", MatKhau = "Test1234", XacNhanMatKhau = "Test1234" });

        // Chua xac thuc VA sai mat khau -> phai bao sai mat khau (MS01,401),
        // KHONG duoc lo MS10 (403) truoc khi biet mat khau dung hay sai
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.DangNhapAsync(new LoginRequest { Email = "g@test.local", MatKhau = "saimatkhau" }));
        Assert.Equal(401, ex.StatusCode);
    }

    [Fact]
    [Trait("Category", "BR18")]
    public async Task DangNhap_TaiKhoanBiAdminKhoaVinhVien_ThatBai()
    {
        var service = NewService(out var db);
        await service.DangKyUngVienAsync(new DangKyUngVienRequest { HoTen = "A", Email = "h@test.local", MatKhau = "Test1234", XacNhanMatKhau = "Test1234" });
        var taiKhoan = await db.TaiKhoans.FirstAsync(x => x.Email == "h@test.local");
        taiKhoan.DaXacThuc = true;
        taiKhoan.TrangThai = "BiKhoa";
        taiKhoan.LyDoKhoa = "Vi pham dieu khoan";
        await db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.DangNhapAsync(new LoginRequest { Email = "h@test.local", MatKhau = "Test1234" }));
        Assert.Equal(403, ex.StatusCode); // MS11
    }

    [Fact]
    [Trait("Category", "BR08")]
    public async Task XacThucEmail_TokenHetHan_ThatBai()
    {
        var service = NewService(out var db, out var jwt);
        await service.DangKyUngVienAsync(new DangKyUngVienRequest { HoTen = "A", Email = "h@test.local", MatKhau = "Test1234", XacNhanMatKhau = "Test1234" });

        var taiKhoan = db.TaiKhoans.Single(x => x.Email == "h@test.local");
        var token = db.TokenXacThucs.Single(x => x.MaTK == taiKhoan.MaTK && x.LoaiToken == "XacThucEmail");
        token.ThoiHanHetHan = DateTime.UtcNow.AddMinutes(-1); // gia lap qua han
        await db.SaveChangesAsync();

        var tokenValue = $"{token.MaToken}.{jwt.KyTokenMucDich(token.MaToken, token.LoaiToken, token.ThoiHanHetHan)}";
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.VerifyEmailAsync(tokenValue));
        Assert.Equal(400, ex.StatusCode); // MS19
    }

    [Fact]
    [Trait("Category", "BR08")]
    public async Task DatLaiMatKhau_TokenQua15Phut_ThatBai()
    {
        var service = NewService(out var db, out var jwt);
        await service.DangKyUngVienAsync(new DangKyUngVienRequest { HoTen = "A", Email = "i@test.local", MatKhau = "Test1234", XacNhanMatKhau = "Test1234" });
        await service.ForgotPasswordAsync("i@test.local");

        var taiKhoan = db.TaiKhoans.Single(x => x.Email == "i@test.local");
        var token = db.TokenXacThucs.Single(x => x.MaTK == taiKhoan.MaTK && x.LoaiToken == "DatLaiMatKhau");
        token.ThoiHanHetHan = DateTime.UtcNow.AddMinutes(-1); // qua 15 phut (BR08)
        await db.SaveChangesAsync();

        var tokenValue = $"{token.MaToken}.{jwt.KyTokenMucDich(token.MaToken, token.LoaiToken, token.ThoiHanHetHan)}";
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.ResetPasswordAsync(tokenValue, "NewPass123", "NewPass123"));
        Assert.Equal(400, ex.StatusCode); // MS23
    }

    [Fact]
    [Trait("Category", "Security")]
    public async Task XacThucEmail_ChuKyBiSuaSai_ThatBai()
    {
        // Chung minh lo hong "doan MaToken" da duoc va: du MaToken dung va
        // token chua het han, sua sai 1 ky tu trong chu ky thi van bi tu
        // choi - khong con the chi doan so nguyen la xac thuc duoc tai
        // khoan nguoi khac nhu truoc.
        var service = NewService(out var db, out var jwt);
        await service.DangKyUngVienAsync(new DangKyUngVienRequest { HoTen = "A", Email = "j@test.local", MatKhau = "Test1234", XacNhanMatKhau = "Test1234" });

        var taiKhoan = db.TaiKhoans.Single(x => x.Email == "j@test.local");
        var token = db.TokenXacThucs.Single(x => x.MaTK == taiKhoan.MaTK && x.LoaiToken == "XacThucEmail");
        var chuKyDung = jwt.KyTokenMucDich(token.MaToken, token.LoaiToken, token.ThoiHanHetHan);
        var chuKySai = chuKyDung + "x"; // gia mao / sua sai

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.VerifyEmailAsync($"{token.MaToken}.{chuKySai}"));
        Assert.Equal(400, ex.StatusCode);

        var taiKhoanSau = db.TaiKhoans.Single(x => x.Email == "j@test.local");
        Assert.False(taiKhoanSau.DaXacThuc); // van chua xac thuc duoc
    }

    [Fact]
    [Trait("Category", "Security")]
    public async Task DangKy_GuiEmailXacThucDungNguoiNhanVaKemToken()
    {
        var db = TestHelpers.NewInMemoryDb();
        var email = new FakeEmailService();
        var service = new AuthService(db, new ThamSoService(db), new FakeJwtService(), email);

        await service.DangKyUngVienAsync(new DangKyUngVienRequest { HoTen = "A", Email = "k@test.local", MatKhau = "Test1234", XacNhanMatKhau = "Test1234" });

        var goiGui = Assert.Single(email.XacThucDaGui);
        Assert.Equal("k@test.local", goiGui.ToEmail);
        Assert.Contains('.', goiGui.TokenValue); // dung dinh dang "{MaToken}.{chuKy}"
    }
}
