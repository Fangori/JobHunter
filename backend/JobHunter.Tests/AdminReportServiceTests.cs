using JobHunter.API.DTOs;
using JobHunter.API.Models;
using JobHunter.API.Services;
using Xunit;

namespace JobHunter.Tests;

public class AdminReportServiceTests
{
    [Fact]
    [Trait("Category", "BR24")]
    public async Task LayBaoCaoThang_CoGiaoDichThanhCong_TinhDungDoanhThu()
    {
        var db = TestHelpers.NewInMemoryDb();
        var taiKhoan = new TaiKhoan
        {
            Email = "ntd-report@test.local", MatKhau = "x", VaiTro = "NhaTuyenDung",
            DaXacThuc = true, TrangThai = "HoatDong", NgayTao = DateTime.UtcNow,
        };
        db.TaiKhoans.Add(taiKhoan);
        await db.SaveChangesAsync();
        db.NhaTuyenDungs.Add(new NhaTuyenDung { MaTK = taiKhoan.MaTK, TenCongTy = "Test Co", SoTinDangTuyen = 0 });
        await db.SaveChangesAsync();

        var goiService = new GoiDichVuService(db);
        var goi = await goiService.ThemGoiAsync(new GoiDichVuUpsertRequest { TenGoi = "Gold", GioiHanTin = 20, GiaTien = 599000 });
        await goiService.MuaGoiAsync(taiKhoan.MaTK, goi.MaGoi, new MuaGoiRequest { PhuongThucThanhToan = "ChuyenKhoan", ThongTinThanhToan = "STK 1" });

        var reportService = new AdminReportService(db);
        var now = DateTime.UtcNow;
        var baoCao = await reportService.LayBaoCaoThangAsync(now.Month, now.Year);

        var doanhThu = baoCao.ChiTieu.First(x => x.Ten == "Doanh thu gói dịch vụ");
        Assert.Equal(599000m, doanhThu.SoLuong);
    }

    [Fact]
    [Trait("Category", "BR24")]
    public async Task LayBaoCaoThang_KhongCoGiaoDich_DoanhThuBang0()
    {
        var db = TestHelpers.NewInMemoryDb();
        var reportService = new AdminReportService(db);
        var now = DateTime.UtcNow;
        var baoCao = await reportService.LayBaoCaoThangAsync(now.Month, now.Year);

        var doanhThu = baoCao.ChiTieu.First(x => x.Ten == "Doanh thu gói dịch vụ");
        Assert.Equal(0m, doanhThu.SoLuong);
    }

    [Fact]
    [Trait("Category", "LAB4")]
    public async Task LayPhanBoVaiTro_DemDungSoLuongTheoTungVaiTro()
    {
        var db = TestHelpers.NewInMemoryDb();
        db.TaiKhoans.AddRange(
            new TaiKhoan { Email = "uv1@test.local", MatKhau = "x", VaiTro = "UngVien", DaXacThuc = true, TrangThai = "HoatDong", NgayTao = DateTime.UtcNow },
            new TaiKhoan { Email = "uv2@test.local", MatKhau = "x", VaiTro = "UngVien", DaXacThuc = true, TrangThai = "HoatDong", NgayTao = DateTime.UtcNow },
            new TaiKhoan { Email = "ntd1@test.local", MatKhau = "x", VaiTro = "NhaTuyenDung", DaXacThuc = true, TrangThai = "HoatDong", NgayTao = DateTime.UtcNow },
            new TaiKhoan { Email = "admin1@test.local", MatKhau = "x", VaiTro = "Admin", DaXacThuc = true, TrangThai = "HoatDong", NgayTao = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var reportService = new AdminReportService(db);
        var phanBo = await reportService.LayPhanBoVaiTroAsync();

        Assert.Equal(2, phanBo.SoUngVien);
        Assert.Equal(1, phanBo.SoNhaTuyenDung);
        Assert.Equal(1, phanBo.SoAdmin);
    }
}
