using JobHunter.API.Data;
using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.API.Services;

public class CandidateProfileService : ICandidateProfileService
{
    private static readonly string[] DinhDangAnhChoPhep = { ".jpg", ".jpeg", ".png" };
    private const long DungLuongAnhToiDaByte = 2 * 1024 * 1024; // 2MB, hardcode - khong co TS param (xem plan Phase 7)

    private readonly JobHunterDbContext _db;
    private readonly ICloudinaryFileService _cloudinary;

    public CandidateProfileService(JobHunterDbContext db, ICloudinaryFileService cloudinary)
    {
        _db = db;
        _cloudinary = cloudinary;
    }

    public async Task<CandidateProfileDto> LayHoSoAsync(int maTk)
    {
        var uv = await _db.UngViens.AsNoTracking().FirstAsync(x => x.MaTK == maTk);
        return ToDto(uv);
    }

    public async Task<CandidateProfileDto> CapNhatHoSoAsync(int maTk, CapNhatCandidateProfileRequest request, IFormFile? anhDaiDien)
    {
        var uv = await _db.UngViens.FirstAsync(x => x.MaTK == maTk);

        if (anhDaiDien is not null)
        {
            var extension = Path.GetExtension(anhDaiDien.FileName).ToLowerInvariant();
            if (!DinhDangAnhChoPhep.Contains(extension) || anhDaiDien.Length > DungLuongAnhToiDaByte)
                throw new BusinessRuleException(400, "Ảnh đại diện phải có định dạng .jpg, .png và dung lượng dưới 2MB."); // MS53

            uv.AnhDaiDien = await _cloudinary.UploadImageAsync(anhDaiDien, $"jobhunter/avatar/{maTk}");
        }

        uv.HoTen = request.HoTen;
        uv.NgaySinh = request.NgaySinh;
        uv.SDT = request.Sdt;
        uv.DiaChi = request.DiaChi;
        uv.GioiThieuBanThan = request.GioiThieuBanThan;

        await _db.SaveChangesAsync();
        return ToDto(uv);
    }

    private static CandidateProfileDto ToDto(Models.UngVien uv) => new()
    {
        HoTen = uv.HoTen,
        NgaySinh = uv.NgaySinh,
        Sdt = uv.SDT,
        AnhDaiDien = uv.AnhDaiDien,
        DiaChi = uv.DiaChi,
        GioiThieuBanThan = uv.GioiThieuBanThan,
    };
}
