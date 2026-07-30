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

    public JobService(JobHunterDbContext db, IThamSoService thamSo)
    {
        _db = db;
        _thamSo = thamSo;
    }

    public async Task<TinTuyenDungSummaryDto> DangTinAsync(int maTkNtd, DangTinRequest request)
    {
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

        return await LayTomTatAsync(tin.MaTin);
    }

    public async Task<List<TinTuyenDungSummaryDto>> XemDanhSachCongKhaiAsync(string? keyword, string? diaDiem)
    {
        var query = _db.TinTuyenDungs.Include(x => x.NhaTuyenDung)
            .Where(x => x.TrangThai == "DaDuyet");

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(x => x.TieuDe.Contains(keyword));
        if (!string.IsNullOrWhiteSpace(diaDiem))
            query = query.Where(x => x.DiaDiem != null && x.DiaDiem.Contains(diaDiem));

        return await query.OrderByDescending(x => x.NgayDang)
            .Select(x => ToSummary(x))
            .ToListAsync();
    }

    public async Task<List<TinTuyenDungSummaryDto>> XemNoiBatAsync(int top)
    {
        return await _db.TinTuyenDungs.Include(x => x.NhaTuyenDung)
            .Where(x => x.TrangThai == "DaDuyet")
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
            TenCongTy = tin.NhaTuyenDung.TenCongTy,
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
        TenCongTy = x.NhaTuyenDung.TenCongTy,
        DiaDiem = x.DiaDiem,
        MucLuong = x.MucLuong,
        HinhThucLamViec = x.HinhThucLamViec,
        NgayDang = x.NgayDang,
        HanNopHoSo = x.HanNopHoSo,
        TrangThai = x.TrangThai,
    };
}
