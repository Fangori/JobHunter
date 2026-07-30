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

        var service = new JobService(db, new ThamSoService(db), new NotificationService(db));
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

    [Fact]
    [Trait("Category", "BR15")]
    public async Task SuaTin_DaDuyet_TuDongVeChoDuyet()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        var job = await service.DangTinAsync(maTkNtd, new DangTinRequest
        {
            TieuDe = "Test", MoTaCongViec = "mo ta",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
        });
        await service.DuyetTinAsync(job.MaTin);

        var result = await service.SuaTinAsync(maTkNtd, job.MaTin, new DangTinRequest
        {
            TieuDe = "Test Da Sua", MoTaCongViec = "mo ta moi",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
        });

        Assert.Equal("ChoDuyet", result.Tin.TrangThai);
        Assert.Equal("Cập nhật tin thành công. Tin sẽ được duyệt lại trước khi hiển thị công khai.", result.Message);
    }

    [Fact]
    [Trait("Category", "BR15")]
    public async Task SuaTin_DangChoDuyet_KhongDoiTrangThaiVaThongBaoKhacMS41()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        var job = await service.DangTinAsync(maTkNtd, new DangTinRequest
        {
            TieuDe = "Test", MoTaCongViec = "mo ta",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
        });

        var result = await service.SuaTinAsync(maTkNtd, job.MaTin, new DangTinRequest
        {
            TieuDe = "Test Da Sua", MoTaCongViec = "mo ta moi",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
        });

        Assert.Equal("ChoDuyet", result.Tin.TrangThai);
        Assert.Equal("Cập nhật tin thành công.", result.Message);
    }

    [Fact]
    [Trait("Category", "BR24")]
    public async Task GiaHan_HanMoiKhongHopLe_ThatBai()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        var job = await service.DangTinAsync(maTkNtd, new DangTinRequest
        {
            TieuDe = "Test", MoTaCongViec = "mo ta",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
        });
        await service.DuyetTinAsync(job.MaTin);

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.GiaHanAsync(maTkNtd, job.MaTin, DateOnly.FromDateTime(DateTime.UtcNow)));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    [Trait("Category", "BR24")]
    public async Task GiaHan_HanMoiHopLe_ThanhCong()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        var job = await service.DangTinAsync(maTkNtd, new DangTinRequest
        {
            TieuDe = "Test", MoTaCongViec = "mo ta",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
        });
        await service.DuyetTinAsync(job.MaTin);

        var hanMoi = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
        var result = await service.GiaHanAsync(maTkNtd, job.MaTin, hanMoi);
        Assert.Equal(hanMoi, result.HanNopHoSo);
    }

    [Fact]
    [Trait("Category", "UC27")]
    public async Task DongTin_DangDaDuyet_ThanhCong()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        var job = await service.DangTinAsync(maTkNtd, new DangTinRequest
        {
            TieuDe = "Test", MoTaCongViec = "mo ta",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
        });
        await service.DuyetTinAsync(job.MaTin);

        var result = await service.DongTinAsync(maTkNtd, job.MaTin);
        Assert.Equal("DaDong", result.TrangThai);
    }

    [Fact]
    [Trait("Category", "UC27")]
    public async Task DongTin_ChuaDuocDuyet_ThatBai()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        var job = await service.DangTinAsync(maTkNtd, new DangTinRequest
        {
            TieuDe = "Test", MoTaCongViec = "mo ta",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
        });

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.DongTinAsync(maTkNtd, job.MaTin));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    [Trait("Category", "BR17")]
    public async Task GoTin_KhongCoLyDo_ThatBai()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        var job = await service.DangTinAsync(maTkNtd, new DangTinRequest
        {
            TieuDe = "Test", MoTaCongViec = "mo ta",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
        });
        await service.DuyetTinAsync(job.MaTin);

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.GoTinAsync(job.MaTin, ""));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    [Trait("Category", "UC35")]
    public async Task GoTin_CoLyDo_ThanhCongVaLuuLyDo()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        var job = await service.DangTinAsync(maTkNtd, new DangTinRequest
        {
            TieuDe = "Test", MoTaCongViec = "mo ta",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
        });
        await service.DuyetTinAsync(job.MaTin);

        await service.GoTinAsync(job.MaTin, "Vi pham chinh sach");

        var chiTiet = await service.XemChiTietAsync(job.MaTin);
        Assert.Equal("DaGo", chiTiet.TrangThai);
    }

    [Fact]
    [Trait("Category", "UC36")]
    public async Task PhucHoiTinDaGo_ThanhCong()
    {
        var (service, maTkNtd) = await NewServiceWithNtdAsync();
        var job = await service.DangTinAsync(maTkNtd, new DangTinRequest
        {
            TieuDe = "Test", MoTaCongViec = "mo ta",
            HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
        });
        await service.DuyetTinAsync(job.MaTin);
        await service.GoTinAsync(job.MaTin, "Vi pham chinh sach");

        await service.PhucHoiTinDaGoAsync(job.MaTin);

        var chiTiet = await service.XemChiTietAsync(job.MaTin);
        Assert.Equal("DaDuyet", chiTiet.TrangThai);
    }
}
