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

        var tin = new TinTuyenDung
        {
            MaTK = maTkNtd,
            TieuDe = request.TieuDe,
            MoTaCongViec = request.MoTaCongViec,
            YeuCauUngVien = request.YeuCauUngVien,
            QuyenLoi = request.QuyenLoi,
            MucLuong = request.MucLuong,
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

        return await query.OrderByDescending(x => x.NgayDang)
            .Select(x => ToSummary(x))
            .ToListAsync();
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

        tin.TieuDe = request.TieuDe;
        tin.MoTaCongViec = request.MoTaCongViec;
        tin.YeuCauUngVien = request.YeuCauUngVien;
        tin.QuyenLoi = request.QuyenLoi;
        tin.MucLuong = request.MucLuong;
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
        HinhThucLamViec = x.HinhThucLamViec,
        NgayDang = x.NgayDang,
        HanNopHoSo = x.HanNopHoSo,
        TrangThai = x.TrangThai,
    };
}
