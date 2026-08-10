using JobHunter.API.Data;
using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Models;
using JobHunter.API.Services;
using Xunit;

namespace JobHunter.Tests;

public class GoiDichVuServiceTests
{
    private static async Task<(GoiDichVuService service, JobHunterDbContext db, int maTkNtd)> NewServiceWithNtdAsync()
    {
        var db = TestHelpers.NewInMemoryDb();
        var taiKhoan = new TaiKhoan
        {
            Email = "ntd-goi@test.local", MatKhau = "x", VaiTro = "NhaTuyenDung",
            DaXacThuc = true, TrangThai = "HoatDong", NgayTao = DateTime.UtcNow,
        };
        db.TaiKhoans.Add(taiKhoan);
        await db.SaveChangesAsync();
        db.NhaTuyenDungs.Add(new NhaTuyenDung { MaTK = taiKhoan.MaTK, TenCongTy = "Test Co", SoTinDangTuyen = 0 });
        await db.SaveChangesAsync();

        var service = new GoiDichVuService(db);
        return (service, db, taiKhoan.MaTK);
    }

    [Fact]
    [Trait("Category", "BR26")]
    public async Task ThemGoi_TenTrung_ThatBai()
    {
        var (service, _, _) = await NewServiceWithNtdAsync();
        await service.ThemGoiAsync(new GoiDichVuUpsertRequest { TenGoi = "Standard", GioiHanTin = 10, GiaTien = 299000 });

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.ThemGoiAsync(new GoiDichVuUpsertRequest { TenGoi = "standard", GioiHanTin = 20, GiaTien = 599000 }));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    [Trait("Category", "BR27")]
    public async Task XoaGoi_KhongCoNtdSuDung_XoaVatLy()
    {
        var (service, db, _) = await NewServiceWithNtdAsync();
        var goi = await service.ThemGoiAsync(new GoiDichVuUpsertRequest { TenGoi = "Standard", GioiHanTin = 10, GiaTien = 299000 });

        var message = await service.XoaGoiAsync(goi.MaGoi);
        Assert.Equal("Xóa gói dịch vụ thành công.", message);
        Assert.Empty(db.GoiDichVus);
    }

    [Fact]
    [Trait("Category", "BR27")]
    public async Task XoaGoi_DangCoNtdSuDung_ChuyenNgungBan()
    {
        var (service, db, maTkNtd) = await NewServiceWithNtdAsync();
        var goi = await service.ThemGoiAsync(new GoiDichVuUpsertRequest { TenGoi = "Standard", GioiHanTin = 10, GiaTien = 299000 });
        await service.MuaGoiAsync(maTkNtd, goi.MaGoi, new MuaGoiRequest { PhuongThucThanhToan = "ChuyenKhoan", ThongTinThanhToan = "STK 123" });

        var message = await service.XoaGoiAsync(goi.MaGoi);
        Assert.Equal("Gói dịch vụ đã chuyển sang trạng thái ngừng bán.", message);
        var goiSauXoa = await db.GoiDichVus.FindAsync(goi.MaGoi);
        Assert.Equal("NgungBan", goiSauXoa!.TrangThai);
    }

    [Fact]
    [Trait("Category", "UC43")]
    public async Task MuaGoi_ThongTinThanhToanRong_ThatBai()
    {
        var (service, _, maTkNtd) = await NewServiceWithNtdAsync();
        var goi = await service.ThemGoiAsync(new GoiDichVuUpsertRequest { TenGoi = "Standard", GioiHanTin = 10, GiaTien = 299000 });

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.MuaGoiAsync(maTkNtd, goi.MaGoi, new MuaGoiRequest { PhuongThucThanhToan = "ChuyenKhoan", ThongTinThanhToan = "" }));
        Assert.Equal(400, ex.StatusCode);
        Assert.Equal("Thanh toán thất bại, vui lòng thử lại.", ex.Message);
    }

    [Fact]
    [Trait("Category", "UC43")]
    public async Task MuaGoi_HopLe_KichHoatGoiVaCapNhatGioiHan()
    {
        var (service, _, maTkNtd) = await NewServiceWithNtdAsync();
        var goi = await service.ThemGoiAsync(new GoiDichVuUpsertRequest { TenGoi = "Gold", GioiHanTin = 20, GiaTien = 599000 });

        var result = await service.MuaGoiAsync(maTkNtd, goi.MaGoi, new MuaGoiRequest { PhuongThucThanhToan = "TheNganHang", ThongTinThanhToan = "4111111111111111" });
        Assert.Equal("Mua gói dịch vụ thành công.", result.Message);
        Assert.Equal("Gold", result.GoiHienTai.TenGoi);
        Assert.Equal(20, result.GoiHienTai.GioiHanTin);

        var gioiHan = await service.LayGioiHanHieuLucAsync(maTkNtd);
        Assert.Equal(20, gioiHan);
    }

    [Fact]
    [Trait("Category", "QD18")]
    public async Task LayGioiHanHieuLuc_ChuaTungMua_TraVeMienPhi3()
    {
        var (service, _, maTkNtd) = await NewServiceWithNtdAsync();
        var gioiHan = await service.LayGioiHanHieuLucAsync(maTkNtd);
        Assert.Equal(3, gioiHan);
    }

    [Fact]
    [Trait("Category", "QD18")]
    public async Task LayGioiHanHieuLuc_GoiDaHetHan_TraVeMienPhi3()
    {
        var (service, db, maTkNtd) = await NewServiceWithNtdAsync();
        var goi = await service.ThemGoiAsync(new GoiDichVuUpsertRequest { TenGoi = "Gold", GioiHanTin = 20, GiaTien = 599000 });
        db.GiaoDichMuaGois.Add(new GiaoDichMuaGoi
        {
            MaTK = maTkNtd,
            MaGoi = goi.MaGoi,
            NgayMua = DateTime.UtcNow.AddDays(-60),
            NgayHetHan = DateTime.UtcNow.AddDays(-30), // da het han
            SoTien = 599000,
            PhuongThucThanhToan = "ChuyenKhoan",
            TrangThai = "ThanhCong",
        });
        await db.SaveChangesAsync();

        var gioiHan = await service.LayGioiHanHieuLucAsync(maTkNtd);
        Assert.Equal(3, gioiHan);
    }

    [Fact]
    [Trait("Category", "QD18")]
    public async Task LayGioiHanHieuLuc_NhieuGoiConHanCungLuc_LayGioiHanLonNhat()
    {
        var (service, _, maTkNtd) = await NewServiceWithNtdAsync();
        var goiStandard = await service.ThemGoiAsync(new GoiDichVuUpsertRequest { TenGoi = "Standard", GioiHanTin = 10, GiaTien = 299000 });
        var goiGold = await service.ThemGoiAsync(new GoiDichVuUpsertRequest { TenGoi = "Gold", GioiHanTin = 20, GiaTien = 599000 });
        await service.MuaGoiAsync(maTkNtd, goiStandard.MaGoi, new MuaGoiRequest { PhuongThucThanhToan = "ChuyenKhoan", ThongTinThanhToan = "STK 1" });
        await service.MuaGoiAsync(maTkNtd, goiGold.MaGoi, new MuaGoiRequest { PhuongThucThanhToan = "TheNganHang", ThongTinThanhToan = "4111111111111111" });

        var gioiHan = await service.LayGioiHanHieuLucAsync(maTkNtd);
        Assert.Equal(20, gioiHan); // lay gioi han lon nhat (Gold), khong phai gia tri mua sau cung

        var goiHienTai = (await service.LayDanhSachChoNtdAsync(maTkNtd)).GoiHienTai;
        Assert.Equal("Gold", goiHienTai.TenGoi);
    }

    [Fact]
    [Trait("Category", "BR26")]
    public async Task SuaGoi_TenTrungVoiGoiKhac_ThatBai()
    {
        var (service, _, _) = await NewServiceWithNtdAsync();
        await service.ThemGoiAsync(new GoiDichVuUpsertRequest { TenGoi = "Standard", GioiHanTin = 10, GiaTien = 299000 });
        var goiGold = await service.ThemGoiAsync(new GoiDichVuUpsertRequest { TenGoi = "Gold", GioiHanTin = 20, GiaTien = 599000 });

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.SuaGoiAsync(goiGold.MaGoi, new GoiDichVuUpsertRequest { TenGoi = "standard", GioiHanTin = 25, GiaTien = 699000 }));
        Assert.Equal(400, ex.StatusCode);
    }
}
