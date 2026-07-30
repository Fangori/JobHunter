using JobHunter.API.Data;
using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Models;
using JobHunter.API.Services;
using Xunit;

namespace JobHunter.Tests;

public class ApplicationServiceTests
{
    private static async Task<(ApplicationService service, JobHunterDbContext db, int maTkUv, int maTin, int maCv)> SetupAsync()
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
        var cv = new Cv { MaTK = taiKhoanUv.MaTK, TenCV = "CV1", LoaiCV = "TrucTuyen", TrangThai = "HoatDong", NgayTao = DateTime.UtcNow };
        db.Cvs.Add(cv);
        await db.SaveChangesAsync();

        var service = new ApplicationService(db);
        return (service, db, taiKhoanUv.MaTK, tin.MaTin, cv.MaCV);
    }

    [Fact]
    [Trait("Category", "QD10")]
    public async Task UngTuyen_DaCoDonActive_ThatBai()
    {
        var (service, db, maTkUv, maTin, maCv) = await SetupAsync();
        await service.UngTuyenAsync(maTkUv, new UngTuyenRequest { MaCv = maCv, MaTin = maTin });

        var cv2 = new Cv { MaTK = maTkUv, TenCV = "CV2", LoaiCV = "TrucTuyen", TrangThai = "HoatDong", NgayTao = DateTime.UtcNow };
        db.Cvs.Add(cv2);
        await db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.UngTuyenAsync(maTkUv, new UngTuyenRequest { MaCv = cv2.MaCV, MaTin = maTin }));
        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    [Trait("Category", "QD10")]
    public async Task UngTuyen_SauKhiDonCuDaHuy_ThanhCong()
    {
        var (service, db, maTkUv, maTin, maCv) = await SetupAsync();
        var don1 = await service.UngTuyenAsync(maTkUv, new UngTuyenRequest { MaCv = maCv, MaTin = maTin });

        var donEntity = await db.DonUngTuyens.FindAsync(don1.MaDon);
        donEntity!.TrangThai = "DaHuy";
        await db.SaveChangesAsync();

        var result = await service.UngTuyenAsync(maTkUv, new UngTuyenRequest { MaCv = maCv, MaTin = maTin });
        Assert.Equal("DaNop", result.TrangThai);
    }

    [Fact]
    [Trait("Category", "QD10")]
    public async Task UngTuyen_TinHetHanNop_ThatBai()
    {
        var (service, db, maTkUv, maTin, maCv) = await SetupAsync();
        var tin = await db.TinTuyenDungs.FindAsync(maTin);
        tin!.HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        await db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            service.UngTuyenAsync(maTkUv, new UngTuyenRequest { MaCv = maCv, MaTin = maTin }));
        Assert.Equal(400, ex.StatusCode);
    }
}
