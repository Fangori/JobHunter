using JobHunter.API.Data;
using JobHunter.API.Models;
using JobHunter.API.Services;
using Xunit;

namespace JobHunter.Tests;

public class CandidateMatchServiceTests
{
    private static async Task<(CandidateMatchService service, JobHunterDbContext db, int maTin, int maCv)> SetupAsync(
        (int maKyNang, string? mucDoUuTien)[] kyNangTin, int[] maKyNangCv)
    {
        var db = TestHelpers.NewInMemoryDb();

        var taiKhoanNtd = new TaiKhoan { Email = "ntd@t.local", MatKhau = "x", VaiTro = "NhaTuyenDung", DaXacThuc = true, TrangThai = "HoatDong", NgayTao = DateTime.UtcNow };
        var taiKhoanUv = new TaiKhoan { Email = "uv@t.local", MatKhau = "x", VaiTro = "UngVien", DaXacThuc = true, TrangThai = "HoatDong", NgayTao = DateTime.UtcNow };
        db.TaiKhoans.AddRange(taiKhoanNtd, taiKhoanUv);
        await db.SaveChangesAsync();
        db.NhaTuyenDungs.Add(new NhaTuyenDung { MaTK = taiKhoanNtd.MaTK, TenCongTy = "Co", SoTinDangTuyen = 0 });
        db.UngViens.Add(new UngVien { MaTK = taiKhoanUv.MaTK, HoTen = "UV", SoCV = 0 });
        await db.SaveChangesAsync();

        for (var i = 1; i <= 4; i++)
            db.DanhMucKyNangs.Add(new DanhMucKyNang { MaKyNang = i, TenKyNang = $"KyNang{i}", TrangThai = "HoatDong" });
        await db.SaveChangesAsync();

        var tin = new TinTuyenDung
        {
            MaTK = taiKhoanNtd.MaTK, TieuDe = "Job", MoTaCongViec = "mo ta",
            NgayDang = DateTime.UtcNow, HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            TrangThai = "DaDuyet", SoDonUngTuyen = 0,
        };
        foreach (var (maKyNang, mucDo) in kyNangTin)
            tin.TinKyNangs.Add(new TinKyNang { MaKyNang = maKyNang, MucDoUuTien = mucDo });
        db.TinTuyenDungs.Add(tin);

        var cv = new Cv { MaTK = taiKhoanUv.MaTK, TenCV = "CV1", LoaiCV = "TrucTuyen", TrangThai = "HoatDong", NgayTao = DateTime.UtcNow };
        foreach (var maKyNang in maKyNangCv)
            cv.CvKyNangs.Add(new CvKyNang { MaKyNang = maKyNang });
        db.Cvs.Add(cv);
        await db.SaveChangesAsync();

        var service = new CandidateMatchService(db);
        return (service, db, tin.MaTin, cv.MaCV);
    }

    [Fact]
    [Trait("Category", "QD14")]
    public async Task TinhDiemPhuHop_TinhDungCongThuc()
    {
        // Tin yeu cau 4 ky nang (1,2,3,4); CV khop 3/4 (1,2,3)
        var (service, _, maTin, maCv) = await SetupAsync(
            new (int, string?)[] { (1, "BatBuoc"), (2, "BatBuoc"), (3, "UuTien"), (4, "UuTien") },
            new[] { 1, 2, 3 });

        var ketQua = await service.TinhDiemPhuHopAsync(maCv, maTin);
        Assert.Equal(75.0, ketQua);
    }

    [Fact]
    [Trait("Category", "QD14")]
    public async Task TinhDiemPhuHop_KhongPhanBietMucDoUuTien()
    {
        // Tin co 2 BatBuoc (1,2) + 2 UuTien (3,4); CV chi khop 1 ky nang BatBuoc (1)
        var (service, _, maTin, maCv) = await SetupAsync(
            new (int, string?)[] { (1, "BatBuoc"), (2, "BatBuoc"), (3, "UuTien"), (4, "UuTien") },
            new[] { 1 });

        var ketQua = await service.TinhDiemPhuHopAsync(maCv, maTin);
        Assert.Equal(25.0, ketQua); // 1/4, khong duoc "nang" len vi la BatBuoc
    }
}
