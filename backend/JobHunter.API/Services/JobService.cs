using JobHunter.API.Data;
using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Models;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.API.Services;

public class JobService : IJobService
{
    private readonly JobHunterDbContext _db;
    private readonly IThamSoService _thamSo;
    private readonly INotificationService _notification;
    private readonly IGoiDichVuService _goiDichVu;

    public JobService(JobHunterDbContext db, IThamSoService thamSo, INotificationService notification, IGoiDichVuService goiDichVu)
    {
        _db = db;
        _thamSo = thamSo;
        _notification = notification;
        _goiDichVu = goiDichVu;
    }

    public async Task<TinTuyenDungSummaryDto> DangTinAsync(int maTkNtd, DangTinRequest request)
    {
        var gioiHan = await _goiDichVu.LayGioiHanHieuLucAsync(maTkNtd);
        var soDangHoatDong = await _db.TinTuyenDungs.CountAsync(x =>
            x.MaTK == maTkNtd && (x.TrangThai == "ChoDuyet" || x.TrangThai == "DaDuyet"));
        if (soDangHoatDong >= gioiHan)
            throw new BusinessRuleException(400,
                "Bạn đã đạt giới hạn số tin đăng tuyển đồng thời của gói dịch vụ hiện tại. Vui lòng mua thêm gói dịch vụ để đăng thêm tin."); // MS67, QD18

        var soNgayToiThieu = await _thamSo.LayGiaTriIntAsync("TS7");
        var ngayDang = DateTime.UtcNow;
        var ngayDangOnly = DateOnly.FromDateTime(ngayDang);
        if (request.HanNopHoSo < ngayDangOnly.AddDays(soNgayToiThieu))
            throw new BusinessRuleException(400, "Hạn nộp hồ sơ phải sau ngày đăng tin ít nhất 1 ngày."); // MS06
        KiemTraKhoangLuong(request.LuongToiThieu, request.LuongToiDa);

        var tin = new TinTuyenDung
        {
            MaTK = maTkNtd,
            TieuDe = request.TieuDe,
            MoTaCongViec = request.MoTaCongViec,
            YeuCauUngVien = request.YeuCauUngVien,
            QuyenLoi = request.QuyenLoi,
            MucLuong = FormatMucLuong(request.LuongToiThieu, request.LuongToiDa),
            LuongToiThieu = request.LuongToiThieu,
            LuongToiDa = request.LuongToiDa,
            SoLuongTuyen = request.SoLuongTuyen ?? 1,
            DiaDiem = request.DiaDiem,
            HinhThucLamViec = request.HinhThucLamViec,
            SoNamKinhNghiemYeuCau = request.SoNamKinhNghiemYeuCau,
            NgayDang = ngayDang,
            HanNopHoSo = request.HanNopHoSo,
            TrangThai = "ChoDuyet",
            SoDonUngTuyen = 0,
        };
        foreach (var kn in request.KyNangYeuCau)
        {
            tin.TinKyNangs.Add(new TinKyNang { MaKyNang = kn.MaKyNang, MucDoUuTien = kn.MucDoUuTien });
        }

        _db.TinTuyenDungs.Add(tin);

        var ntd = await _db.NhaTuyenDungs.FirstAsync(x => x.MaTK == maTkNtd);
        ntd.SoTinDangTuyen++;

        await _db.SaveChangesAsync();

        // Thong bao cho tat ca Admin (UC25 "gui thong bao cho Admin")
        var maAdmins = await _db.TaiKhoans.Where(x => x.VaiTro == "Admin").Select(x => x.MaTK).ToListAsync();
        foreach (var maAdmin in maAdmins)
        {
            await _notification.TaoThongBaoAsync(maAdmin,
                $"Tin \"{tin.TieuDe}\" của {ntd.TenCongTy} đang chờ duyệt.", "TinMoi", "/admin/pending-jobs");
        }

        return await LayTomTatAsync(tin.MaTin);
    }

    public async Task<List<TinTuyenDungSummaryDto>> XemDanhSachCongKhaiAsync(string? keyword, string? diaDiem, int? maNganhNghe = null)
    {
        var query = TinCongKhaiQuery(keyword, diaDiem, maNganhNghe);
        return await query.OrderByDescending(x => x.NgayDang)
            .Select(x => ToSummary(x))
            .ToListAsync();
    }

    // Dung chung giua XemDanhSachCongKhaiAsync (cu) va TimKiemVaLocAsync
    // (loc nang cao) - tranh 2 noi cung Where(BR25/keyword/diaDiem/nganhNghe)
    // ma sua lech nhau ve sau (vd sua dieu kien BR25 o 1 cho, quen cho kia).
    private IQueryable<TinTuyenDung> TinCongKhaiQuery(string? keyword, string? diaDiem, int? maNganhNghe)
    {
        var query = _db.TinTuyenDungs.Include(x => x.NhaTuyenDung).ThenInclude(n => n.TaiKhoan)
            .Where(x => x.TrangThai == "DaDuyet" && x.NhaTuyenDung.TaiKhoan.TrangThai != "BiKhoa"); // BR25

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(x => x.TieuDe.Contains(keyword));
        if (!string.IsNullOrWhiteSpace(diaDiem))
            query = query.Where(x => x.DiaDiem != null && x.DiaDiem.Contains(diaDiem));
        // Loc theo nganh nghe cua CONG TY dang tin (TIN_TUYEN_DUNG khong co
        // cot nganh nghe rieng trong schema - dung dung MaNganhNghe da co
        // san tren NHA_TUYEN_DUNG, khong phai khop chuoi ten nganh vao tieu
        // de tin nhu truoc, vi tieu de khong bao gio chua ten nganh nghe).
        if (maNganhNghe.HasValue)
            query = query.Where(x => x.NhaTuyenDung.MaNganhNghe == maNganhNghe.Value);

        return query;
    }

    public async Task<List<TinTuyenDungSummaryDto>> XemNoiBatAsync(int top)
    {
        return await _db.TinTuyenDungs.Include(x => x.NhaTuyenDung).ThenInclude(n => n.TaiKhoan)
            .Where(x => x.TrangThai == "DaDuyet" && x.NhaTuyenDung.TaiKhoan.TrangThai != "BiKhoa") // BR25
            .OrderByDescending(x => x.NgayDang)
            .Take(top)
            .Select(x => ToSummary(x))
            .ToListAsync();
    }

    public async Task<TinTuyenDungDetailDto> XemChiTietAsync(int maTin)
    {
        var tin = await _db.TinTuyenDungs
            .Include(x => x.NhaTuyenDung)
            .Include(x => x.TinKyNangs)
            .FirstOrDefaultAsync(x => x.MaTin == maTin);
        if (tin is null)
            throw new BusinessRuleException(404, "Không tìm thấy tin tuyển dụng.");

        return new TinTuyenDungDetailDto
        {
            MaTin = tin.MaTin,
            TieuDe = tin.TieuDe,
            MaTkNtd = tin.MaTK,
            TenCongTy = tin.NhaTuyenDung.TenCongTy,
            Logo = tin.NhaTuyenDung.Logo,
            MaNganhNghe = tin.NhaTuyenDung.MaNganhNghe,
            DiaDiem = tin.DiaDiem,
            MucLuong = tin.MucLuong,
            LuongToiThieu = tin.LuongToiThieu,
            LuongToiDa = tin.LuongToiDa,
            SoLuongTuyen = tin.SoLuongTuyen,
            HinhThucLamViec = tin.HinhThucLamViec,
            NgayDang = tin.NgayDang,
            HanNopHoSo = tin.HanNopHoSo,
            TrangThai = tin.TrangThai,
            MoTaCongViec = tin.MoTaCongViec,
            YeuCauUngVien = tin.YeuCauUngVien,
            QuyenLoi = tin.QuyenLoi,
            SoNamKinhNghiemYeuCau = tin.SoNamKinhNghiemYeuCau,
            KyNangYeuCau = tin.TinKyNangs.Select(k => new KyNangYeuCauDto { MaKyNang = k.MaKyNang, MucDoUuTien = k.MucDoUuTien }).ToList(),
        };
    }

    public async Task<List<TinTuyenDungSummaryDto>> XemDanhSachChoDuyetAsync()
    {
        return await _db.TinTuyenDungs.Include(x => x.NhaTuyenDung)
            .Where(x => x.TrangThai == "ChoDuyet")
            .OrderBy(x => x.NgayDang)
            .Select(x => ToSummary(x))
            .ToListAsync();
    }

    public async Task<PendingStatsResponse> ThongKeChoDuyetAsync()
    {
        return new PendingStatsResponse
        {
            SoChoDuyet = await _db.TinTuyenDungs.CountAsync(x => x.TrangThai == "ChoDuyet"),
            SoDaDuyet = await _db.TinTuyenDungs.CountAsync(x => x.TrangThai == "DaDuyet"),
        };
    }

    public async Task DuyetTinAsync(int maTin)
    {
        var tin = await _db.TinTuyenDungs.FindAsync(maTin);
        if (tin is null)
            throw new BusinessRuleException(404, "Không tìm thấy tin tuyển dụng.");
        tin.TrangThai = "DaDuyet";
        await _db.SaveChangesAsync();

        await _notification.TaoThongBaoAsync(tin.MaTK,
            $"Tin \"{tin.TieuDe}\" đã được duyệt và hiển thị công khai.", "TinDaDuyet", $"/jobs/{tin.MaTin}");
    }

    public async Task TuChoiTinAsync(int maTin, string lyDo)
    {
        if (string.IsNullOrWhiteSpace(lyDo))
            throw new BusinessRuleException(400, "Lý do từ chối là bắt buộc."); // BR16/QD15

        var tin = await _db.TinTuyenDungs.FindAsync(maTin);
        if (tin is null)
            throw new BusinessRuleException(404, "Không tìm thấy tin tuyển dụng.");
        tin.TrangThai = "TuChoi";
        tin.LyDoTuChoi = lyDo;
        await _db.SaveChangesAsync();

        await _notification.TaoThongBaoAsync(tin.MaTK,
            $"Tin \"{tin.TieuDe}\" đã bị từ chối. Lý do: {lyDo}", "TinBiTuChoi", "/employer/my-jobs");
    }

    public async Task<List<TinTuyenDungSummaryDto>> LayDanhSachCuaToiAsync(int maTkNtd)
    {
        return await _db.TinTuyenDungs.Include(x => x.NhaTuyenDung)
            .Where(x => x.MaTK == maTkNtd)
            .OrderByDescending(x => x.NgayDang)
            .Select(x => ToSummary(x))
            .ToListAsync();
    }

    public async Task<SuaTinResponse> SuaTinAsync(int maTkNtd, int maTin, DangTinRequest request)
    {
        var tin = await _db.TinTuyenDungs.Include(x => x.TinKyNangs).FirstOrDefaultAsync(x => x.MaTin == maTin);
        if (tin is null || tin.MaTK != maTkNtd)
            throw new BusinessRuleException(404, "Không tìm thấy tin tuyển dụng.");
        if (tin.TrangThai == "DaGo" || tin.TrangThai == "DaDong")
            throw new BusinessRuleException(400, "Không thể sửa tin đã gỡ hoặc đã đóng.");

        var soNgayToiThieu = await _thamSo.LayGiaTriIntAsync("TS7");
        var ngayDangOnly = DateOnly.FromDateTime(tin.NgayDang);
        if (request.HanNopHoSo < ngayDangOnly.AddDays(soNgayToiThieu))
            throw new BusinessRuleException(400, "Hạn nộp hồ sơ phải sau ngày đăng tin ít nhất 1 ngày."); // MS06
        KiemTraKhoangLuong(request.LuongToiThieu, request.LuongToiDa);

        tin.TieuDe = request.TieuDe;
        tin.MoTaCongViec = request.MoTaCongViec;
        tin.YeuCauUngVien = request.YeuCauUngVien;
        tin.QuyenLoi = request.QuyenLoi;
        tin.MucLuong = FormatMucLuong(request.LuongToiThieu, request.LuongToiDa);
        tin.LuongToiThieu = request.LuongToiThieu;
        tin.LuongToiDa = request.LuongToiDa;
        tin.SoLuongTuyen = request.SoLuongTuyen ?? 1;
        tin.DiaDiem = request.DiaDiem;
        tin.HinhThucLamViec = request.HinhThucLamViec;
        tin.SoNamKinhNghiemYeuCau = request.SoNamKinhNghiemYeuCau;
        tin.HanNopHoSo = request.HanNopHoSo;

        _db.TinKyNangs.RemoveRange(tin.TinKyNangs);
        foreach (var kn in request.KyNangYeuCau)
            _db.TinKyNangs.Add(new TinKyNang { MaTin = tin.MaTin, MaKyNang = kn.MaKyNang, MucDoUuTien = kn.MucDoUuTien });

        var daTungDuyet = tin.TrangThai == "DaDuyet";
        if (daTungDuyet)
            tin.TrangThai = "ChoDuyet"; // BR15

        await _db.SaveChangesAsync();

        if (daTungDuyet)
            await _notification.TaoThongBaoAsync(tin.MaTK,
                $"Tin \"{tin.TieuDe}\" đã được cập nhật và đang chờ duyệt lại.", "TinMoi", "/employer/my-jobs");

        var message = daTungDuyet
            ? "Cập nhật tin thành công. Tin sẽ được duyệt lại trước khi hiển thị công khai." // MS41, BR15
            : "Cập nhật tin thành công.";
        return new SuaTinResponse { Tin = await LayTomTatAsync(tin.MaTin), Message = message };
    }

    public async Task<TinTuyenDungSummaryDto> GiaHanAsync(int maTkNtd, int maTin, DateOnly hanNopMoi)
    {
        var tin = await _db.TinTuyenDungs.FindAsync(maTin);
        if (tin is null || tin.MaTK != maTkNtd)
            throw new BusinessRuleException(404, "Không tìm thấy tin tuyển dụng.");
        if (tin.TrangThai != "DaDuyet")
            throw new BusinessRuleException(400, "Chỉ có thể gia hạn tin đang hiển thị công khai.");

        var soNgayToiThieu = await _thamSo.LayGiaTriIntAsync("TS7");
        var homNay = DateOnly.FromDateTime(DateTime.UtcNow);
        if (hanNopMoi < homNay.AddDays(soNgayToiThieu))
            throw new BusinessRuleException(400, "Hạn nộp hồ sơ phải sau ngày đăng tin ít nhất 1 ngày."); // MS06, BR24

        tin.HanNopHoSo = hanNopMoi;
        await _db.SaveChangesAsync();
        return await LayTomTatAsync(tin.MaTin);
    }

    public async Task<TinTuyenDungSummaryDto> DongTinAsync(int maTkNtd, int maTin)
    {
        var tin = await _db.TinTuyenDungs.FindAsync(maTin);
        if (tin is null || tin.MaTK != maTkNtd)
            throw new BusinessRuleException(404, "Không tìm thấy tin tuyển dụng.");
        if (tin.TrangThai != "DaDuyet")
            throw new BusinessRuleException(400, "Chỉ có thể đóng tin đang hiển thị công khai.");

        tin.TrangThai = "DaDong";
        await _db.SaveChangesAsync();
        return await LayTomTatAsync(tin.MaTin);
    }

    public async Task GoTinAsync(int maTin, string lyDo)
    {
        if (string.IsNullOrWhiteSpace(lyDo))
            throw new BusinessRuleException(400, "Vui lòng nhập lý do gỡ tin."); // MS54, BR17

        var tin = await _db.TinTuyenDungs.FindAsync(maTin);
        if (tin is null)
            throw new BusinessRuleException(404, "Không tìm thấy tin tuyển dụng.");
        if (tin.TrangThai != "DaDuyet")
            throw new BusinessRuleException(400, "Chỉ có thể gỡ tin đang hiển thị công khai.");

        tin.TrangThai = "DaGo";
        tin.LyDoGo = lyDo;
        await _db.SaveChangesAsync();

        await _notification.TaoThongBaoAsync(tin.MaTK,
            $"Tin \"{tin.TieuDe}\" đã bị gỡ. Lý do: {lyDo}", "TinBiGo", "/employer/my-jobs");
    }

    public async Task PhucHoiTinDaGoAsync(int maTin)
    {
        var tin = await _db.TinTuyenDungs.FindAsync(maTin);
        if (tin is null)
            throw new BusinessRuleException(404, "Không tìm thấy tin tuyển dụng.");
        if (tin.TrangThai != "DaGo")
            throw new BusinessRuleException(400, "Chỉ có thể phục hồi tin đã bị gỡ.");

        tin.TrangThai = "DaDuyet";
        await _db.SaveChangesAsync();

        await _notification.TaoThongBaoAsync(tin.MaTK,
            $"Tin \"{tin.TieuDe}\" đã được phục hồi.", "TinDuocPhucHoi", $"/jobs/{tin.MaTin}");
    }

    public async Task<List<TinTuyenDungSummaryDto>> XemDanhSachDaGoAsync()
    {
        return await _db.TinTuyenDungs.Include(x => x.NhaTuyenDung)
            .Where(x => x.TrangThai == "DaGo")
            .OrderByDescending(x => x.NgayDang)
            .Select(x => ToSummary(x))
            .ToListAsync();
    }

    private async Task<TinTuyenDungSummaryDto> LayTomTatAsync(int maTin)
    {
        var tin = await _db.TinTuyenDungs.Include(x => x.NhaTuyenDung).FirstAsync(x => x.MaTin == maTin);
        return ToSummary(tin);
    }

    private static TinTuyenDungSummaryDto ToSummary(TinTuyenDung x) => new()
    {
        MaTin = x.MaTin,
        TieuDe = x.TieuDe,
        MaTkNtd = x.MaTK,
        TenCongTy = x.NhaTuyenDung.TenCongTy,
        Logo = x.NhaTuyenDung.Logo,
        MaNganhNghe = x.NhaTuyenDung.MaNganhNghe,
        DiaDiem = x.DiaDiem,
        MucLuong = x.MucLuong,
        LuongToiThieu = x.LuongToiThieu,
        LuongToiDa = x.LuongToiDa,
        SoLuongTuyen = x.SoLuongTuyen,
        HinhThucLamViec = x.HinhThucLamViec,
        NgayDang = x.NgayDang,
        HanNopHoSo = x.HanNopHoSo,
        TrangThai = x.TrangThai,
    };

    // MucLuong (chuoi hien thi cho JobCard/JobDetail...) sinh tu khoang so -
    // giu 1 nguon su that duy nhat, tranh lech giua so va text nhu truoc.
    private static string? FormatMucLuong(int? min, int? max)
    {
        if (min is null && max is null) return "Thỏa thuận";
        if (min is not null && max is not null) return $"{min}-{max} triệu";
        if (min is not null) return $"Từ {min} triệu";
        return $"Đến {max} triệu";
    }

    private static void KiemTraKhoangLuong(int? min, int? max)
    {
        if (min is not null && max is not null && min > max)
            throw new BusinessRuleException(400, "Lương tối thiểu phải nhỏ hơn hoặc bằng lương tối đa.");
    }

    // UC10 (LAB4) - tim kiem/loc nang cao: tu khoa, dia diem, nganh nghe (da
    // co san), them loai hinh cong viec, khoang luong, sap xep, phan trang.
    public async Task<DanhSachViecLamPhanTrangDto> TimKiemVaLocAsync(TimKiemVaLocRequest request)
    {
        var query = TinCongKhaiQuery(request.Keyword, request.DiaDiem, request.MaNganhNghe);

        if (request.HinhThucLamViec is { Count: > 0 })
            query = query.Where(x => x.HinhThucLamViec != null && request.HinhThucLamViec.Contains(x.HinhThucLamViec));
        // Loc theo khoang luong = phep giao 2 khoang [LuongToiThieu,LuongToiDa]
        // (cua tin) va [LuongMin,LuongMax] (nguoi dung chon). Mot dau NULL o
        // tin nghia la "khong gioi han phia do" (vd chi nhap "Tu 15 trieu")
        // -> KHONG duoc coi la khong khop, chi loai tin "Thoa thuan" (CA HAI
        // deu NULL) giong hanh vi cac trang tuyen dung thuc te khac. Truoc do
        // dieu kien doi hoi ca dau doi dien phai co gia tri nen tin chi nhap
        // 1 dau (luong mo) bi loai oan khoi moi khoang loc - da sua lai 12/08.
        if (request.LuongMin.HasValue || request.LuongMax.HasValue)
        {
            query = query.Where(x => x.LuongToiThieu != null || x.LuongToiDa != null);
            if (request.LuongMin.HasValue)
                query = query.Where(x => x.LuongToiDa == null || x.LuongToiDa >= request.LuongMin.Value);
            if (request.LuongMax.HasValue)
                query = query.Where(x => x.LuongToiThieu == null || x.LuongToiThieu <= request.LuongMax.Value);
        }

        query = request.SortBy switch
        {
            "luong_giam" => query.OrderByDescending(x => x.LuongToiDa ?? x.LuongToiThieu ?? -1),
            "luong_tang" => query.OrderBy(x => x.LuongToiThieu ?? x.LuongToiDa ?? int.MaxValue),
            _ => query.OrderByDescending(x => x.NgayDang),
        };

        var totalCount = await query.CountAsync();
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 50 ? 9 : request.PageSize;
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => ToSummary(x))
            .ToListAsync();

        return new DanhSachViecLamPhanTrangDto { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }
}
