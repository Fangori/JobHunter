using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Models;
using JobHunter.API.Services;
using Xunit;

namespace JobHunter.Tests;

public class JobServiceTests
{
    private static async Task<(JobService service, int maTkNtd)> NewServiceWithNtdAsync()
    {
        var db = TestHelpers.NewInMemoryDb();
        var taiKhoan = new TaiKhoan
        {
            Email = "ntd@test.local", MatKhau = "x", VaiTro = "NhaTuyenDung",
            DaXacThuc = true, TrangThai = "HoatDong", NgayTao = DateTime.UtcNow,
        };
        db.TaiKhoans.Add(taiKhoan);
        await db.SaveChangesAsync();
        db.NhaTuyenDungs.Add(new NhaTuyenDung { MaTK = taiKhoan.MaTK, TenCongTy = "Test Co", SoTinDangTuyen = 0 });
        await db.SaveChangesAsync();

        var service = new JobService(db, new ThamSoService(db));
        return (service, taiKhoan.MaTK);
    }

    [Fact]
    [Trait("Category", "QD09")]
    public async Task DangTin_HanNopKhongDuTS7Ngay_ThatBai()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        var request = new DangTinRequest
        {
            TieuDe = "Test",
            MoTaCongViec = "mo ta",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow), // = hom nay, khong du TS7=1 ngay
        };

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.DangTinAsync(maTkNtd, request));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    [Trait("Category", "QD09")]
    public async Task DangTin_HanNopHopLe_ThanhCong()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        var request = new DangTinRequest
        {
            TieuDe = "Test",
            MoTaCongViec = "mo ta",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
        };

        var result = await service.DangTinAsync(maTkNtd, request);
        Assert.Equal("ChoDuyet", result.TrangThai);
    }

    [Fact]
    [Trait("Category", "BR16")]
    public async Task TuChoiTin_KhongCoLyDo_ThatBai()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        var job = await service.DangTinAsync(maTkNtd, new DangTinRequest
        {
            TieuDe = "Test", MoTaCongViec = "mo ta",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
        });

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.TuChoiTinAsync(job.MaTin, ""));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    [Trait("Category", "BR16")]
    public async Task TuChoiTin_CoLyDo_ThanhCongVaLuuLyDo()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        var job = await service.DangTinAsync(maTkNtd, new DangTinRequest
        {
            TieuDe = "Test", MoTaCongViec = "mo ta",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
        });

        await service.TuChoiTinAsync(job.MaTin, "Khong phu hop tieu chuan dang tin");

        var chiTiet = await service.XemChiTietAsync(job.MaTin);
        Assert.Equal("TuChoi", chiTiet.TrangThai);
    }
}
