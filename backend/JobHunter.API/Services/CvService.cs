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

    public async Task<List<CvSummaryDto>> LayDanhSachCuaToiAsync(int maTkUv)
    {
        return await _db.Cvs
            .Where(x => x.MaTK == maTkUv && x.TrangThai == "HoatDong")
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
