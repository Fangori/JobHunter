using JobHunter.API.Data;
using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.API.Services;

public class CandidateMatchService : ICandidateMatchService
{
    private readonly JobHunterDbContext _db;

    public CandidateMatchService(JobHunterDbContext db)
    {
        _db = db;
    }

    // QD14/BR04: % = (so ky nang CV khop / tong so ky nang tin yeu cau) x 100
    // KHONG phan biet MucDoUuTien (BatBuoc/UuTien) - da xac nhan trong docs/IMPLEMENTATION_PLAN.md muc 2
    public async Task<double> TinhDiemPhuHopAsync(int maCv, int maTin)
    {
        var kyNangTin = await _db.TinKyNangs.Where(x => x.MaTin == maTin).Select(x => x.MaKyNang).ToListAsync();
        if (kyNangTin.Count == 0) return 100; // BR04: tin khong yeu cau ky nang nao -> mac dinh 100%

        var kyNangCv = await _db.CvKyNangs.Where(x => x.MaCV == maCv).Select(x => x.MaKyNang).ToListAsync();
        var soKhop = kyNangCv.Intersect(kyNangTin).Count();

        return Math.Round(soKhop * 100.0 / kyNangTin.Count, 1);
    }

    public async Task<List<UngVienPhuHopDto>> XemDanhSachUngVienAsync(int maTin, int maTkNtd)
    {
        await KiemTraChuTinAsync(maTin, maTkNtd);
        var toanBo = await LayToanBoAsync(maTin);
        return toanBo.OrderByDescending(x => x.Dto.PhanTramPhuHop).Select(x => x.Dto).ToList();
    }

    public async Task<List<UngVienPhuHopDto>> LocUngVienAsync(int maTin, int maTkNtd, List<int>? maKyNang, int? minNamKinhNghiem, List<string>? trinhDoHocVan)
    {
        await KiemTraChuTinAsync(maTin, maTkNtd);
        var toanBo = await LayToanBoAsync(maTin);

        IEnumerable<UngVienNoiBo> ketQua = toanBo;

        // Facet-filter kieu shop: ung vien phai co DU tat ca ky nang da chon (AND), doc lap voi cong thuc %
        if (maKyNang is { Count: > 0 })
            ketQua = ketQua.Where(x => maKyNang.All(id => x.MaKyNangCv.Contains(id)));
        if (minNamKinhNghiem.HasValue)
            ketQua = ketQua.Where(x => x.Dto.SoNamKinhNghiem >= minNamKinhNghiem.Value);
        if (trinhDoHocVan is { Count: > 0 })
            ketQua = ketQua.Where(x => x.Dto.TrinhDoHocVan != null && trinhDoHocVan.Contains(x.Dto.TrinhDoHocVan));

        return ketQua.OrderByDescending(x => x.Dto.PhanTramPhuHop).Select(x => x.Dto).ToList();
    }

    // UC14: tinh diem tren TUNG CV rieng, lay diem CAO NHAT giua cac CV cho
    // moi tin (khong co khai niem "CV mac dinh") - chi goi y tin co diem > 0
    public async Task<GoiYViecLamResultDto> GoiYViecLamAsync(int maTkUv)
    {
        var cvList = await _db.Cvs
            .Where(x => x.MaTK == maTkUv && x.TrangThai == "HoatDong")
            .Include(x => x.CvKyNangs)
            .ToListAsync();
        if (cvList.Count == 0) return new GoiYViecLamResultDto { CoCv = false, GoiY = new List<ViecLamGoiYDto>() };

        var jobs = await _db.TinTuyenDungs
            .Include(x => x.NhaTuyenDung).ThenInclude(n => n.TaiKhoan)
            .Include(x => x.TinKyNangs)
            .Where(x => x.TrangThai == "DaDuyet" && x.NhaTuyenDung.TaiKhoan.TrangThai != "BiKhoa") // BR25
            .ToListAsync();

        var ketQua = new List<ViecLamGoiYDto>();
        foreach (var job in jobs)
        {
            var kyNangTin = job.TinKyNangs.Select(k => k.MaKyNang).ToList();

            double diemCaoNhat;
            int maCvPhuHopNhat;
            if (kyNangTin.Count == 0)
            {
                // BR04: tin khong yeu cau ky nang nao -> mac dinh 100% cho moi CV,
                // lay CV dau tien lam CV phu hop nhat (khong CV nao "hop" hon CV nao)
                diemCaoNhat = 100;
                maCvPhuHopNhat = cvList[0].MaCV;
            }
            else
            {
                diemCaoNhat = 0;
                maCvPhuHopNhat = 0;
                foreach (var cv in cvList)
                {
                    var soKhop = cv.CvKyNangs.Select(k => k.MaKyNang).Intersect(kyNangTin).Count();
                    var phanTram = Math.Round(soKhop * 100.0 / kyNangTin.Count, 1);
                    if (phanTram > diemCaoNhat)
                    {
                        diemCaoNhat = phanTram;
                        maCvPhuHopNhat = cv.MaCV;
                    }
                }
                if (diemCaoNhat <= 0) continue;
            }

            ketQua.Add(new ViecLamGoiYDto
            {
                MaTin = job.MaTin,
                TieuDe = job.TieuDe,
                MaTkNtd = job.MaTK,
                TenCongTy = job.NhaTuyenDung.TenCongTy,
                DiaDiem = job.DiaDiem,
                MucLuong = job.MucLuong,
                HinhThucLamViec = job.HinhThucLamViec,
                NgayDang = job.NgayDang,
                HanNopHoSo = job.HanNopHoSo,
                TrangThai = job.TrangThai,
                PhanTramPhuHop = diemCaoNhat,
                MaCvPhuHopNhat = maCvPhuHopNhat,
            });
        }

        return new GoiYViecLamResultDto
        {
            CoCv = true,
            GoiY = ketQua.OrderByDescending(x => x.PhanTramPhuHop).ThenByDescending(x => x.NgayDang).ToList(),
        };
    }

    private async Task KiemTraChuTinAsync(int maTin, int maTkNtd)
    {
        var tin = await _db.TinTuyenDungs.FindAsync(maTin);
        if (tin is null)
            throw new BusinessRuleException(404, "Không tìm thấy tin tuyển dụng.");
        if (tin.MaTK != maTkNtd)
            throw new BusinessRuleException(403, "Bạn không có quyền xem danh sách ứng viên của tin này.");
    }

    // Giu ca MaKyNang (id) cua CV de loc chinh xac theo BM14, khong chi giu ban DTO tra ve
    private class UngVienNoiBo
    {
        public required UngVienPhuHopDto Dto { get; init; }
        public required List<int> MaKyNangCv { get; init; }
    }

    private async Task<List<UngVienNoiBo>> LayToanBoAsync(int maTin)
    {
        var kyNangTin = await _db.TinKyNangs.Where(x => x.MaTin == maTin).Select(x => x.MaKyNang).ToListAsync();

        var donList = await _db.DonUngTuyens
            .Where(x => x.MaTin == maTin)
            .Include(x => x.Cv).ThenInclude(cv => cv.UngVien)
            .Include(x => x.Cv).ThenInclude(cv => cv.CvKyNangs)
            .Include(x => x.Cv).ThenInclude(cv => cv.CvKinhNghiems)
            .ToListAsync();

        var tatCaMaKyNang = donList.SelectMany(d => d.Cv.CvKyNangs.Select(k => k.MaKyNang)).Distinct().ToList();
        var tenKyNangMap = await _db.DanhMucKyNangs
            .Where(x => tatCaMaKyNang.Contains(x.MaKyNang))
            .ToDictionaryAsync(x => x.MaKyNang, x => x.TenKyNang);

        var ketQua = new List<UngVienNoiBo>();
        foreach (var don in donList)
        {
            var maKyNangCv = don.Cv.CvKyNangs.Select(k => k.MaKyNang).ToList();
            var soKhop = maKyNangCv.Intersect(kyNangTin).Count();
            var phanTram = kyNangTin.Count == 0 ? 100 : Math.Round(soKhop * 100.0 / kyNangTin.Count, 1); // BR04
            var soNam = TinhSoNamKinhNghiem(don.Cv.CvKinhNghiems);
            var kyNangKhopTen = maKyNangCv.Intersect(kyNangTin).Select(id => tenKyNangMap.GetValueOrDefault(id, id.ToString())).ToList();

            ketQua.Add(new UngVienNoiBo
            {
                Dto = new UngVienPhuHopDto
                {
                    MaDon = don.MaDon,
                    HoTen = don.Cv.UngVien.HoTen,
                    TenCV = don.Cv.TenCV,
                    PhanTramPhuHop = phanTram,
                    KyNangKhop = kyNangKhopTen,
                    SoNamKinhNghiem = soNam,
                    TrinhDoHocVan = don.Cv.TrinhDoHocVan,
                    TrangThai = don.TrangThai,
                },
                MaKyNangCv = maKyNangCv,
            });
        }
        return ketQua;
    }

    private static int TinhSoNamKinhNghiem(IEnumerable<Models.CvKinhNghiem> danhSach)
    {
        var homNay = DateOnly.FromDateTime(DateTime.UtcNow);
        double tongNgay = 0;
        foreach (var kn in danhSach)
        {
            var denNgay = kn.DenNgay ?? homNay;
            tongNgay += Math.Max(0, denNgay.DayNumber - kn.TuNgay.DayNumber);
        }
        return (int)(tongNgay / 365.25);
    }
}
