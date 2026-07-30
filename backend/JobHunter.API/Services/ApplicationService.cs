using JobHunter.API.Data;
using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Models;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.API.Services;

public class ApplicationService : IApplicationService
{
    // BR05/QD11: DaNop -> DangXemXet -> PhongVan -> (Nhan|TuChoi); TuChoi hop le tu bat ky buoc dang mo
    private static readonly Dictionary<string, string[]> ChuyenTiepHopLe = new()
    {
        ["DaNop"] = new[] { "DangXemXet", "TuChoi" },
        ["DangXemXet"] = new[] { "PhongVan", "TuChoi" },
        ["PhongVan"] = new[] { "Nhan", "TuChoi" },
    };

    private static readonly Dictionary<string, string> TenTrangThaiTiengViet = new()
    {
        ["DangXemXet"] = "Đang xem xét",
        ["PhongVan"] = "Phỏng vấn",
        ["TuChoi"] = "Từ chối",
        ["Nhan"] = "Nhận",
    };

    private readonly JobHunterDbContext _db;
    private readonly INotificationService _notification;

    public ApplicationService(JobHunterDbContext db, INotificationService notification)
    {
        _db = db;
        _notification = notification;
    }

    public async Task<DonUngTuyenResponse> UngTuyenAsync(int maTkUv, UngTuyenRequest request)
    {
        var tin = await _db.TinTuyenDungs.FindAsync(request.MaTin);
        if (tin is null)
            throw new BusinessRuleException(404, "Không tìm thấy tin tuyển dụng.");

        if (tin.HanNopHoSo < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new BusinessRuleException(400, "Tin tuyển dụng đã hết hạn nộp hồ sơ."); // MS32

        var cv = await _db.Cvs.FindAsync(request.MaCv);
        if (cv is null || cv.MaTK != maTkUv)
            throw new BusinessRuleException(404, "Không tìm thấy CV.");

        // QD10/TS8: "dang hoat dong" = tat ca trang thai TRU TuChoi va DaHuy
        var coDonActive = await _db.DonUngTuyens
            .Include(x => x.Cv)
            .AnyAsync(x => x.MaTin == request.MaTin
                        && x.Cv.MaTK == maTkUv
                        && x.TrangThai != "TuChoi"
                        && x.TrangThai != "DaHuy");
        if (coDonActive)
            throw new BusinessRuleException(409, "Bạn đã ứng tuyển vào tin này rồi."); // MS31

        var don = new DonUngTuyen
        {
            MaTin = request.MaTin,
            MaCV = request.MaCv,
            ThuGioiThieu = request.ThuGioiThieu,
            NgayNop = DateTime.UtcNow,
            TrangThai = "DaNop",
            DaXem = false,
        };
        _db.DonUngTuyens.Add(don);
        tin.SoDonUngTuyen++;

        await _db.SaveChangesAsync();
        return new DonUngTuyenResponse { MaDon = don.MaDon, TrangThai = don.TrangThai };
    }

    public async Task HuyDonAsync(int maTkUv, int maDon)
    {
        var don = await _db.DonUngTuyens.Include(x => x.Cv).FirstOrDefaultAsync(x => x.MaDon == maDon);
        if (don is null || don.Cv.MaTK != maTkUv)
            throw new BusinessRuleException(404, "Không tìm thấy đơn ứng tuyển.");

        // BR10: chi huy khi dang "DaNop"/"DangXemXet"
        if (don.TrangThai != "DaNop" && don.TrangThai != "DangXemXet")
            throw new BusinessRuleException(400, "Không thể hủy đơn đã qua bước phỏng vấn."); // MS34

        don.TrangThai = "DaHuy";
        await _db.SaveChangesAsync();
    }

    public async Task<List<DonUngTuyenMineDto>> LayCuaToiAsync(int maTkUv)
    {
        return await _db.DonUngTuyens.AsNoTracking()
            .Include(x => x.Cv)
            .Include(x => x.TinTuyenDung).ThenInclude(t => t.NhaTuyenDung)
            .Where(x => x.Cv.MaTK == maTkUv)
            .OrderByDescending(x => x.NgayNop)
            .Select(x => new DonUngTuyenMineDto
            {
                MaDon = x.MaDon,
                MaTin = x.MaTin,
                TieuDe = x.TinTuyenDung.TieuDe,
                TenCongTy = x.TinTuyenDung.NhaTuyenDung.TenCongTy,
                TrangThai = x.TrangThai,
                NgayNop = x.NgayNop,
            })
            .ToListAsync();
    }

    public async Task<DonUngTuyenDetailDto> LayChiTietAsync(int maTkNtd, int maDon)
    {
        var don = await _db.DonUngTuyens
            .Include(x => x.TinTuyenDung)
            .Include(x => x.Cv).ThenInclude(cv => cv.UngVien)
            .Include(x => x.Cv).ThenInclude(cv => cv.CvKyNangs)
            .Include(x => x.Cv).ThenInclude(cv => cv.CvKinhNghiems)
            .Include(x => x.Cv).ThenInclude(cv => cv.CvHocVans)
            .FirstOrDefaultAsync(x => x.MaDon == maDon);
        if (don is null)
            throw new BusinessRuleException(404, "Không tìm thấy đơn ứng tuyển.");
        if (don.TinTuyenDung.MaTK != maTkNtd)
            throw new BusinessRuleException(403, "Bạn không có quyền xem đơn ứng tuyển này.");

        if (!don.DaXem)
        {
            don.DaXem = true;
            await _db.SaveChangesAsync();
        }

        var cv = don.Cv;
        return new DonUngTuyenDetailDto
        {
            MaDon = don.MaDon,
            TrangThai = don.TrangThai,
            ThuGioiThieu = don.ThuGioiThieu,
            NgayNop = don.NgayNop,
            GhiChuNoiBo = don.GhiChuNoiBo,
            HoTenUngVien = cv.UngVien.HoTen,
            Cv = new CvDetailDto
            {
                MaCV = cv.MaCV,
                TenCV = cv.TenCV,
                LoaiCV = cv.LoaiCV,
                ViTriMongMuon = cv.ViTriMongMuon,
                MucLuongMongMuon = cv.MucLuongMongMuon,
                TrinhDoHocVan = cv.TrinhDoHocVan,
                TrangThai = cv.TrangThai,
                DuongDanFile = cv.DuongDanFile,
                KyNang = cv.CvKyNangs.Select(k => new CvKyNangDto { MaKyNang = k.MaKyNang, MucDoThanhThao = k.MucDoThanhThao }).ToList(),
                KinhNghiem = cv.CvKinhNghiems.Select(k => new CvKinhNghiemDto { CongTy = k.CongTy, ViTri = k.ViTri, TuNgay = k.TuNgay, DenNgay = k.DenNgay, MoTaCongViec = k.MoTaCongViec }).ToList(),
                HocVan = cv.CvHocVans.Select(h => new CvHocVanDto { Truong = h.Truong, ChuyenNganh = h.ChuyenNganh, TuNam = h.TuNam, DenNam = h.DenNam }).ToList(),
            },
        };
    }

    public async Task CapNhatTrangThaiAsync(int maTkNtd, int maDon, string trangThaiMoi, string? ghiChuNoiBo)
    {
        var don = await _db.DonUngTuyens.Include(x => x.TinTuyenDung).FirstOrDefaultAsync(x => x.MaDon == maDon);
        if (don is null)
            throw new BusinessRuleException(404, "Không tìm thấy đơn ứng tuyển.");
        if (don.TinTuyenDung.MaTK != maTkNtd)
            throw new BusinessRuleException(403, "Bạn không có quyền cập nhật đơn ứng tuyển này.");

        if (!ChuyenTiepHopLe.TryGetValue(don.TrangThai, out var cacBuocKeTiep) || !cacBuocKeTiep.Contains(trangThaiMoi))
            throw new BusinessRuleException(400, "Không thể chuyển sang trạng thái này. Vui lòng kiểm tra lại thứ tự xét duyệt."); // MS09, BR05

        don.TrangThai = trangThaiMoi;
        if (ghiChuNoiBo is not null)
            don.GhiChuNoiBo = ghiChuNoiBo;
        await _db.SaveChangesAsync();

        var maTkUv = (await _db.Cvs.FindAsync(don.MaCV))!.MaTK;
        await _notification.TaoThongBaoAsync(maTkUv,
            $"Đơn ứng tuyển vào \"{don.TinTuyenDung.TieuDe}\" đã chuyển sang trạng thái \"{TenTrangThaiTiengViet[trangThaiMoi]}\".",
            "TrangThaiDon", "/candidate/applications");
    }
}
