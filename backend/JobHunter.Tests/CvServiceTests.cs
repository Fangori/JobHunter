using JobHunter.API.Data;
using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Models;
using JobHunter.API.Services;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace JobHunter.Tests;

public class CvServiceTests
{
    private class FakeCloudinary : ICloudinaryFileService
    {
        public Task<string> UploadRawAsync(IFormFile file, string publicIdPrefix) => throw new NotImplementedException();
        public Task<string> UploadImageAsync(IFormFile file, string publicIdPrefix) => throw new NotImplementedException();
    }

    private static async Task<(CvService service, JobHunterDbContext db, int maTkUv, int maTin)> SetupAsync()
    {
        var db = TestHelpers.NewInMemoryDb();

        var taiKhoanNtd = new TaiKhoan { Email = "ntd@t.local", MatKhau = "x", VaiTro = "NhaTuyenDung", DaXacThuc = true, TrangThai = "HoatDong", NgayTao = DateTime.UtcNow };
        var taiKhoanUv = new TaiKhoan { Email = "uv@t.local", MatKhau = "x", VaiTro = "UngVien", DaXacThuc = true, TrangThai = "HoatDong", NgayTao = DateTime.UtcNow };
        db.TaiKhoans.AddRange(taiKhoanNtd, taiKhoanUv);
        await db.SaveChangesAsync();

        db.NhaTuyenDungs.Add(new NhaTuyenDung { MaTK = taiKhoanNtd.MaTK, TenCongTy = "Co", SoTinDangTuyen = 0 });
        db.UngViens.Add(new UngVien { MaTK = taiKhoanUv.MaTK, HoTen = "UV", SoCV = 0 });
        await db.SaveChangesAsync();

        var tin = new TinTuyenDung
        {
            MaTK = taiKhoanNtd.MaTK, TieuDe = "Job", MoTaCongViec = "mo ta",
            NgayDang = DateTime.UtcNow, HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            TrangThai = "DaDuyet", SoDonUngTuyen = 0,
        };
        db.TinTuyenDungs.Add(tin);
        await db.SaveChangesAsync();

        var service = new CvService(db, new ThamSoService(db), new FakeCloudinary());
        return (service, db, taiKhoanUv.MaTK, tin.MaTin);
    }

    [Fact]
    [Trait("Category", "BR13")]
    public async Task XoaCv_ChuaUngTuyen_XoaVatLy()
    {
        var (service, db, maTkUv, _) = await SetupAsync();
        var cv = await service.TaoCvTrucTuyenAsync(maTkUv, new TaoCvTrucTuyenRequest { TenCv = "CV1" });

        var message = await service.XoaCvAsync(maTkUv, cv.MaCV);

        Assert.Equal("Đã xóa CV vĩnh viễn.", message);
        Assert.Null(await db.Cvs.FindAsync(cv.MaCV));
    }

    [Fact]
    [Trait("Category", "BR13")]
    public async Task XoaCv_DaUngTuyen_ChiXoaLogic()
    {
        var (service, db, maTkUv, maTin) = await SetupAsync();
        var cv = await service.TaoCvTrucTuyenAsync(maTkUv, new TaoCvTrucTuyenRequest { TenCv = "CV1" });

        db.DonUngTuyens.Add(new DonUngTuyen { MaTin = maTin, MaCV = cv.MaCV, NgayNop = DateTime.UtcNow, TrangThai = "DaNop" });
        await db.SaveChangesAsync();

        var message = await service.XoaCvAsync(maTkUv, cv.MaCV);

        Assert.Equal("CV đã được ẩn khỏi hồ sơ của bạn. Bạn có thể phục hồi lại sau.", message);
        var cvEntity = await db.Cvs.FindAsync(cv.MaCV);
        Assert.NotNull(cvEntity);
        Assert.Equal("DaAn", cvEntity!.TrangThai);
    }

    [Fact]
    [Trait("Category", "BR13")]
    public async Task PhucHoiCv_CvDaAn_TroLaiHoatDong()
    {
        var (service, db, maTkUv, maTin) = await SetupAsync();
        var cv = await service.TaoCvTrucTuyenAsync(maTkUv, new TaoCvTrucTuyenRequest { TenCv = "CV1" });
        db.DonUngTuyens.Add(new DonUngTuyen { MaTin = maTin, MaCV = cv.MaCV, NgayNop = DateTime.UtcNow, TrangThai = "DaNop" });
        await db.SaveChangesAsync();
        await service.XoaCvAsync(maTkUv, cv.MaCV);

        await service.PhucHoiCvAsync(maTkUv, cv.MaCV);

        var cvEntity = await db.Cvs.FindAsync(cv.MaCV);
        Assert.Equal("HoatDong", cvEntity!.TrangThai);
    }

    [Fact]
    [Trait("Category", "BR13")]
    public async Task XoaCv_KhongPhaiChuSoHuu_ThatBai()
    {
        var (service, db, maTkUv, _) = await SetupAsync();
        var cv = await service.TaoCvTrucTuyenAsync(maTkUv, new TaoCvTrucTuyenRequest { TenCv = "CV1" });

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => service.XoaCvAsync(maTkUv + 999, cv.MaCV));
        Assert.Equal(404, ex.StatusCode);
    }
}
