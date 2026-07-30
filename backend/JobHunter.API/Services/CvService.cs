using JobHunter.API.Data;
using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.API.Services;

public class CvService : ICvService
{
    private static readonly string[] DinhDangChoPhep = { ".pdf", ".doc", ".docx" };

    private readonly JobHunterDbContext _db;
    private readonly IThamSoService _thamSo;
    private readonly ICloudinaryFileService _cloudinary;

    public CvService(JobHunterDbContext db, IThamSoService thamSo, ICloudinaryFileService cloudinary)
    {
        _db = db;
        _thamSo = thamSo;
        _cloudinary = cloudinary;
    }

    public async Task<CvSummaryDto> TaoCvTrucTuyenAsync(int maTkUv, TaoCvTrucTuyenRequest request)
    {
        var cv = new Cv
        {
            MaTK = maTkUv,
            TenCV = request.TenCv,
            LoaiCV = "TrucTuyen",
            TrinhDoHocVan = request.TrinhDoHocVan,
            ViTriMongMuon = request.ViTriMongMuon,
            MucLuongMongMuon = request.MucLuongMongMuon,
            TrangThai = "HoatDong",
            NgayTao = DateTime.UtcNow,
        };
        foreach (var kn in request.KyNang)
            cv.CvKyNangs.Add(new CvKyNang { MaKyNang = kn.MaKyNang, MucDoThanhThao = kn.MucDoThanhThao });
        foreach (var kn in request.KinhNghiem)
            cv.CvKinhNghiems.Add(new CvKinhNghiem
            {
                CongTy = kn.CongTy, ViTri = kn.ViTri, TuNgay = kn.TuNgay, DenNgay = kn.DenNgay, MoTaCongViec = kn.MoTaCongViec,
            });
        foreach (var hv in request.HocVan)
            cv.CvHocVans.Add(new CvHocVan { Truong = hv.Truong, ChuyenNganh = hv.ChuyenNganh, TuNam = hv.TuNam, DenNam = hv.DenNam });

        _db.Cvs.Add(cv);

        var ungVien = await _db.UngViens.FirstAsync(x => x.MaTK == maTkUv);
        ungVien.SoCV++;

        await _db.SaveChangesAsync();
        return ToSummary(cv);
    }

    public async Task<CvSummaryDto> UploadCvAsync(int maTkUv, string tenCv, IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var dungLuongToiDaMb = await _thamSo.LayGiaTriIntAsync("TS5");

        if (!DinhDangChoPhep.Contains(extension) || file.Length > dungLuongToiDaMb * 1024L * 1024L)
            throw new BusinessRuleException(400, "File không đúng định dạng hoặc vượt quá 10MB."); // MS36

        var url = await _cloudinary.UploadRawAsync(file, $"jobhunter/cv/{maTkUv}");

        var cv = new Cv
        {
            MaTK = maTkUv,
            TenCV = tenCv,
            LoaiCV = "Upload",
            DuongDanFile = url,
            TrangThai = "HoatDong",
            NgayTao = DateTime.UtcNow,
        };
        _db.Cvs.Add(cv);

        var ungVien = await _db.UngViens.FirstAsync(x => x.MaTK == maTkUv);
        ungVien.SoCV++;

        await _db.SaveChangesAsync();
        return ToSummary(cv);
    }

    public async Task<List<CvSummaryDto>> LayDanhSachCuaToiAsync(int maTkUv, string trangThai = "HoatDong")
    {
        return await _db.Cvs
            .Where(x => x.MaTK == maTkUv && x.TrangThai == trangThai)
            .OrderByDescending(x => x.NgayTao)
            .Select(x => new CvSummaryDto
            {
                MaCV = x.MaCV,
                TenCV = x.TenCV,
                LoaiCV = x.LoaiCV,
                ViTriMongMuon = x.ViTriMongMuon,
                TrinhDoHocVan = x.TrinhDoHocVan,
                TrangThai = x.TrangThai,
                DuongDanFile = x.DuongDanFile,
            })
            .ToListAsync();
    }

    public async Task<CvDetailDto> LayChiTietAsync(int maTkUv, int maCv)
    {
        var cv = await _db.Cvs
            .Include(x => x.CvKyNangs)
            .Include(x => x.CvKinhNghiems)
            .Include(x => x.CvHocVans)
            .FirstOrDefaultAsync(x => x.MaCV == maCv);
        if (cv is null || cv.MaTK != maTkUv)
            throw new BusinessRuleException(404, "Không tìm thấy CV.");

        return new CvDetailDto
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
        };
    }

    public async Task<CvSummaryDto> SuaCvTrucTuyenAsync(int maTkUv, int maCv, TaoCvTrucTuyenRequest request)
    {
        var cv = await _db.Cvs
            .Include(x => x.CvKyNangs)
            .Include(x => x.CvKinhNghiems)
            .Include(x => x.CvHocVans)
            .FirstOrDefaultAsync(x => x.MaCV == maCv);
        if (cv is null || cv.MaTK != maTkUv)
            throw new BusinessRuleException(404, "Không tìm thấy CV.");
        if (cv.LoaiCV != "TrucTuyen")
            throw new BusinessRuleException(400, "Chỉ có thể sửa CV tạo trực tuyến."); // MS04 (thieu/sai du lieu)

        cv.TenCV = request.TenCv;
        cv.ViTriMongMuon = request.ViTriMongMuon;
        cv.MucLuongMongMuon = request.MucLuongMongMuon;
        cv.TrinhDoHocVan = request.TrinhDoHocVan;

        _db.CvKyNangs.RemoveRange(cv.CvKyNangs);
        _db.CvKinhNghiems.RemoveRange(cv.CvKinhNghiems);
        _db.CvHocVans.RemoveRange(cv.CvHocVans);

        foreach (var kn in request.KyNang)
            _db.CvKyNangs.Add(new CvKyNang { MaCV = cv.MaCV, MaKyNang = kn.MaKyNang, MucDoThanhThao = kn.MucDoThanhThao });
        foreach (var kn in request.KinhNghiem)
            _db.CvKinhNghiems.Add(new CvKinhNghiem { MaCV = cv.MaCV, CongTy = kn.CongTy, ViTri = kn.ViTri, TuNgay = kn.TuNgay, DenNgay = kn.DenNgay, MoTaCongViec = kn.MoTaCongViec });
        foreach (var hv in request.HocVan)
            _db.CvHocVans.Add(new CvHocVan { MaCV = cv.MaCV, Truong = hv.Truong, ChuyenNganh = hv.ChuyenNganh, TuNam = hv.TuNam, DenNam = hv.DenNam });

        await _db.SaveChangesAsync();
        return ToSummary(cv);
    }

    public async Task<string> XoaCvAsync(int maTkUv, int maCv)
    {
        var cv = await _db.Cvs.FirstOrDefaultAsync(x => x.MaCV == maCv);
        if (cv is null || cv.MaTK != maTkUv)
            throw new BusinessRuleException(404, "Không tìm thấy CV.");

        var daTungUngTuyen = await _db.DonUngTuyens.AnyAsync(x => x.MaCV == maCv);
        if (daTungUngTuyen)
        {
            cv.TrangThai = "DaAn";
            await _db.SaveChangesAsync();
            return "CV đã được ẩn khỏi hồ sơ của bạn. Bạn có thể phục hồi lại sau."; // MS39, BR13
        }

        var kyNangs = await _db.CvKyNangs.Where(x => x.MaCV == maCv).ToListAsync();
        var kinhNghiems = await _db.CvKinhNghiems.Where(x => x.MaCV == maCv).ToListAsync();
        var hocVans = await _db.CvHocVans.Where(x => x.MaCV == maCv).ToListAsync();
        _db.CvKyNangs.RemoveRange(kyNangs);
        _db.CvKinhNghiems.RemoveRange(kinhNghiems);
        _db.CvHocVans.RemoveRange(hocVans);
        _db.Cvs.Remove(cv);

        var ungVien = await _db.UngViens.FirstAsync(x => x.MaTK == maTkUv);
        ungVien.SoCV--;

        await _db.SaveChangesAsync();
        return "Đã xóa CV vĩnh viễn."; // MS38, BR13
    }

    public async Task PhucHoiCvAsync(int maTkUv, int maCv)
    {
        var cv = await _db.Cvs.FirstOrDefaultAsync(x => x.MaCV == maCv);
        if (cv is null || cv.MaTK != maTkUv)
            throw new BusinessRuleException(404, "Không tìm thấy CV.");
        if (cv.TrangThai != "DaAn")
            throw new BusinessRuleException(400, "CV này không ở trạng thái đã ẩn.");

        cv.TrangThai = "HoatDong";
        await _db.SaveChangesAsync();
    }

    private static CvSummaryDto ToSummary(Cv cv) => new()
    {
        MaCV = cv.MaCV,
        TenCV = cv.TenCV,
        LoaiCV = cv.LoaiCV,
        ViTriMongMuon = cv.ViTriMongMuon,
        TrinhDoHocVan = cv.TrinhDoHocVan,
        TrangThai = cv.TrangThai,
        DuongDanFile = cv.DuongDanFile,
    };
}
