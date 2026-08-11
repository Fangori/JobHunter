using JobHunter.API.Data;
using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.API.Services;

public class EmployerProfileService : IEmployerProfileService
{
    private static readonly string[] DinhDangAnhChoPhep = { ".jpg", ".jpeg", ".png" };
    private const long DungLuongAnhToiDaByte = 2 * 1024 * 1024; // 2MB, hardcode - khong co TS param

    private readonly JobHunterDbContext _db;
    private readonly ICloudinaryFileService _cloudinary;

    public EmployerProfileService(JobHunterDbContext db, ICloudinaryFileService cloudinary)
    {
        _db = db;
        _cloudinary = cloudinary;
    }

    public async Task<EmployerProfileDto> LayHoSoAsync(int maTk)
    {
        var ntd = await _db.NhaTuyenDungs.AsNoTracking().FirstAsync(x => x.MaTK == maTk);
        return ToDto(ntd);
    }

    public async Task<EmployerProfileDto> CapNhatHoSoAsync(int maTk, CapNhatEmployerProfileRequest request, IFormFile? logo)
    {
        var ntd = await _db.NhaTuyenDungs.FirstAsync(x => x.MaTK == maTk);

        if (logo is not null)
        {
            var extension = Path.GetExtension(logo.FileName).ToLowerInvariant();
            if (!DinhDangAnhChoPhep.Contains(extension) || logo.Length > DungLuongAnhToiDaByte)
                throw new BusinessRuleException(400, "Logo phải có định dạng .jpg, .png và dung lượng dưới 2MB."); // MS60

            ntd.Logo = await _cloudinary.UploadImageAsync(logo, $"jobhunter/logo/{maTk}");
        }

        ntd.TenCongTy = request.TenCongTy;
        ntd.QuyMo = request.QuyMo;
        ntd.MaNganhNghe = request.MaNganhNghe;
        ntd.DiaChi = request.DiaChi;
        ntd.Website = request.Website;
        ntd.GioiThieuCongTy = request.GioiThieuCongTy;

        await _db.SaveChangesAsync();
        return ToDto(ntd);
    }

    public async Task<EmployerPublicProfileDto> LayHoSoCongKhaiAsync(int maTk)
    {
        var ntd = await _db.NhaTuyenDungs.AsNoTracking().FirstOrDefaultAsync(x => x.MaTK == maTk);
        if (ntd is null)
            throw new BusinessRuleException(404, "Không tìm thấy hồ sơ công ty.");

        var tins = await _db.TinTuyenDungs.AsNoTracking()
            .Where(x => x.MaTK == maTk && x.TrangThai == "DaDuyet")
            .OrderByDescending(x => x.NgayDang)
            .Select(x => new TinTuyenDungSummaryDto
            {
                MaTin = x.MaTin,
                TieuDe = x.TieuDe,
                TenCongTy = ntd.TenCongTy,
                Logo = ntd.Logo,
                MaNganhNghe = ntd.MaNganhNghe,
                DiaDiem = x.DiaDiem,
                MucLuong = x.MucLuong,
                HinhThucLamViec = x.HinhThucLamViec,
                NgayDang = x.NgayDang,
                HanNopHoSo = x.HanNopHoSo,
                TrangThai = x.TrangThai,
            })
            .ToListAsync();

        var dto = ToDto(ntd);
        return new EmployerPublicProfileDto
        {
            TenCongTy = dto.TenCongTy,
            Logo = dto.Logo,
            QuyMo = dto.QuyMo,
            MaNganhNghe = dto.MaNganhNghe,
            DiaChi = dto.DiaChi,
            Website = dto.Website,
            GioiThieuCongTy = dto.GioiThieuCongTy,
            TinDangTuyen = tins,
        };
    }

    private static EmployerProfileDto ToDto(NhaTuyenDung ntd) => new()
    {
        TenCongTy = ntd.TenCongTy,
        Logo = ntd.Logo,
        QuyMo = ntd.QuyMo,
        MaNganhNghe = ntd.MaNganhNghe,
        DiaChi = ntd.DiaChi,
        Website = ntd.Website,
        GioiThieuCongTy = ntd.GioiThieuCongTy,
    };
}
