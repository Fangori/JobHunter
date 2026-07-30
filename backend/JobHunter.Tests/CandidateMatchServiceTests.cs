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

    private static async Task<(CandidateMatchService service, JobHunterDbContext db, int maTkUv, TinTuyenDung tin1, TinTuyenDung tin2)> SetupGoiYAsync()
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

        // Tin1 yeu cau ky nang 1,2 - Tin2 yeu cau ky nang 3,4 (khong lien quan gi den tin1)
        var tin1 = new TinTuyenDung { MaTK = taiKhoanNtd.MaTK, TieuDe = "Job1", MoTaCongViec = "mo ta", NgayDang = DateTime.UtcNow, HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)), TrangThai = "DaDuyet", SoDonUngTuyen = 0 };
        tin1.TinKyNangs.Add(new TinKyNang { MaKyNang = 1 });
        tin1.TinKyNangs.Add(new TinKyNang { MaKyNang = 2 });
        var tin2 = new TinTuyenDung { MaTK = taiKhoanNtd.MaTK, TieuDe = "Job2", MoTaCongViec = "mo ta", NgayDang = DateTime.UtcNow, HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)), TrangThai = "DaDuyet", SoDonUngTuyen = 0 };
        tin2.TinKyNangs.Add(new TinKyNang { MaKyNang = 3 });
        tin2.TinKyNangs.Add(new TinKyNang { MaKyNang = 4 });
        db.TinTuyenDungs.AddRange(tin1, tin2);

        // CV_A khop het tin1 (1,2) nhung khong khop tin2; CV_B khop 1 ky nang cua tin2 (3) nhung khong khop tin1
        var cvA = new Cv { MaTK = taiKhoanUv.MaTK, TenCV = "CV A", LoaiCV = "TrucTuyen", TrangThai = "HoatDong", NgayTao = DateTime.UtcNow };
        cvA.CvKyNangs.Add(new CvKyNang { MaKyNang = 1 });
        cvA.CvKyNangs.Add(new CvKyNang { MaKyNang = 2 });
        var cvB = new Cv { MaTK = taiKhoanUv.MaTK, TenCV = "CV B", LoaiCV = "TrucTuyen", TrangThai = "HoatDong", NgayTao = DateTime.UtcNow };
        cvB.CvKyNangs.Add(new CvKyNang { MaKyNang = 3 });
        db.Cvs.AddRange(cvA, cvB);
        await db.SaveChangesAsync();

        var service = new CandidateMatchService(db);
        return (service, db, taiKhoanUv.MaTK, tin1, tin2);
    }

    [Fact]
    [Trait("Category", "UC14")]
    public async Task GoiYViecLam_LayDiemCaoNhatGiuaCacCv()
    {
        var (service, _, maTkUv, tin1, tin2) = await SetupGoiYAsync();

        var goiY = await service.GoiYViecLamAsync(maTkUv);

        var job1 = goiY.Single(x => x.MaTin == tin1.MaTin);
        Assert.Equal(100.0, job1.PhanTramPhuHop); // CV A khop het 2/2

        var job2 = goiY.Single(x => x.MaTin == tin2.MaTin);
        Assert.Equal(50.0, job2.PhanTramPhuHop); // CV B khop 1/2
    }

    [Fact]
    [Trait("Category", "UC14")]
    public async Task GoiYViecLam_SapXepGiamDanTheoDiem()
    {
        var (service, _, maTkUv, tin1, tin2) = await SetupGoiYAsync();

        var goiY = await service.GoiYViecLamAsync(maTkUv);

        Assert.Equal(tin1.MaTin, goiY[0].MaTin); // 100% dung truoc 50%
        Assert.Equal(tin2.MaTin, goiY[1].MaTin);
    }

    [Fact]
    [Trait("Category", "UC14")]
    public async Task GoiYViecLam_TinKhongKhopKyNangNao_BiLoaiKhoiDanhSach()
    {
        var db = TestHelpers.NewInMemoryDb();
        var taiKhoanNtd = new TaiKhoan { Email = "ntd@t.local", MatKhau = "x", VaiTro = "NhaTuyenDung", DaXacThuc = true, TrangThai = "HoatDong", NgayTao = DateTime.UtcNow };
        var taiKhoanUv = new TaiKhoan { Email = "uv@t.local", MatKhau = "x", VaiTro = "UngVien", DaXacThuc = true, TrangThai = "HoatDong", NgayTao = DateTime.UtcNow };
        db.TaiKhoans.AddRange(taiKhoanNtd, taiKhoanUv);
        await db.SaveChangesAsync();
        db.NhaTuyenDungs.Add(new NhaTuyenDung { MaTK = taiKhoanNtd.MaTK, TenCongTy = "Co", SoTinDangTuyen = 0 });
        db.UngViens.Add(new UngVien { MaTK = taiKhoanUv.MaTK, HoTen = "UV", SoCV = 0 });
        await db.SaveChangesAsync();
        db.DanhMucKyNangs.Add(new DanhMucKyNang { MaKyNang = 1, TenKyNang = "A", TrangThai = "HoatDong" });
        db.DanhMucKyNangs.Add(new DanhMucKyNang { MaKyNang = 2, TenKyNang = "B", TrangThai = "HoatDong" });
        await db.SaveChangesAsync();

        var tin = new TinTuyenDung { MaTK = taiKhoanNtd.MaTK, TieuDe = "Job", MoTaCongViec = "mo ta", NgayDang = DateTime.UtcNow, HanNopHoSo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)), TrangThai = "DaDuyet", SoDonUngTuyen = 0 };
        tin.TinKyNangs.Add(new TinKyNang { MaKyNang = 1 });
        db.TinTuyenDungs.Add(tin);
        var cv = new Cv { MaTK = taiKhoanUv.MaTK, TenCV = "CV1", LoaiCV = "TrucTuyen", TrangThai = "HoatDong", NgayTao = DateTime.UtcNow };
        cv.CvKyNangs.Add(new CvKyNang { MaKyNang = 2 }); // hoan toan khong lien quan
        db.Cvs.Add(cv);
        await db.SaveChangesAsync();

        var service = new CandidateMatchService(db);
        var goiY = await service.GoiYViecLamAsync(taiKhoanUv.MaTK);

        Assert.Empty(goiY); // 0% -> khong goi y
    }
}
