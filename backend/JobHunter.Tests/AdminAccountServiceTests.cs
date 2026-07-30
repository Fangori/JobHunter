using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Models;
using JobHunter.API.Services;
using Xunit;

namespace JobHunter.Tests;

public class AdminAccountServiceTests
{
    [Fact]
    [Trait("Category", "BR18")]
    public async Task KhoaTaiKhoan_KhongCoLyDo_ThatBai()
    {
        var db = TestHelpers.NewInMemoryDb();
        var taiKhoan = new TaiKhoan { Email = "ntd@t.local", MatKhau = "x", VaiTro = "NhaTuyenDung", DaXacThuc = true, TrangThai = "HoatDong", NgayTao = DateTime.UtcNow };
        db.TaiKhoans.Add(taiKhoan);
        await db.SaveChangesAsync();
        db.NhaTuyenDungs.Add(new NhaTuyenDung { MaTK = taiKhoan.MaTK, TenCongTy = "Co", SoTinDangTuyen = 0 });
        await db.SaveChangesAsync();

        var service = new AdminAccountService(db, new NotificationService(db));

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.KhoaTaiKhoanAsync(taiKhoan.MaTK, ""));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    [Trait("Category", "BR25")]
    public async Task KhoaTaiKhoanNTD_TinBienMatKhoiCongKhai()
    {
        var db = TestHelpers.NewInMemoryDb();
        var taiKhoan = new TaiKhoan { Email = "ntd@t.local", MatKhau = "x", VaiTro = "NhaTuyenDung", DaXacThuc = true, TrangThai = "HoatDong", NgayTao = DateTime.UtcNow };
        db.TaiKhoans.Add(taiKhoan);
        await db.SaveChangesAsync();
        db.NhaTuyenDungs.Add(new NhaTuyenDung { MaTK = taiKhoan.MaTK, TenCongTy = "Co", SoTinDangTuyen = 0 });
        await db.SaveChangesAsync();
        db.TinTuyenDungs.Add(new TinTuyenDung
        {
            MaTK = taiKhoan.MaTK, TieuDe = "Job", MoTaCongViec = "mo ta",
            NgayDang = DateTime.UtcNow, HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            TrangThai = "DaDuyet", SoDonUngTuyen = 0,
        });
        await db.SaveChangesAsync();

        var accountService = new AdminAccountService(db, new NotificationService(db));
        var jobService = new JobService(db, new ThamSoService(db), new NotificationService(db));

        var truoc = await jobService.XemDanhSachCongKhaiAsync(null, null);
        Assert.Single(truoc);

        await accountService.KhoaTaiKhoanAsync(taiKhoan.MaTK, "Vi pham dieu khoan");

        var sau = await jobService.XemDanhSachCongKhaiAsync(null, null);
        Assert.Empty(sau); // BR25

        await accountService.MoKhoaTaiKhoanAsync(taiKhoan.MaTK);
        var sauMoKhoa = await jobService.XemDanhSachCongKhaiAsync(null, null);
        Assert.Single(sauMoKhoa);
    }
}
