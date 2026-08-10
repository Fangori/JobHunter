using JobHunter.API.Data;
using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Models;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.API.Services;

public class GoiDichVuService : IGoiDichVuService
{
    private const int GioiHanMienPhi = 3; // QD18 - mac dinh khi chua mua/da het han goi nao

    private readonly JobHunterDbContext _db;

    public GoiDichVuService(JobHunterDbContext db)
    {
        _db = db;
    }

    public async Task<List<GoiDichVuDto>> LayDanhSachAdminAsync()
    {
        return await _db.GoiDichVus.OrderBy(x => x.GiaTien)
            .Select(x => ToDto(x))
            .ToListAsync();
    }

    public async Task<GoiDichVuDto> ThemGoiAsync(GoiDichVuUpsertRequest request)
    {
        var trung = await _db.GoiDichVus.AnyAsync(x => x.TenGoi.ToLower() == request.TenGoi.Trim().ToLower());
        if (trung)
            throw new BusinessRuleException(400, "Tên gói dịch vụ đã tồn tại."); // MS63, BR26

        var goi = new GoiDichVu
        {
            TenGoi = request.TenGoi.Trim(),
            GioiHanTin = request.GioiHanTin,
            CoNoiBat = request.CoNoiBat,
            GiaTien = request.GiaTien,
            ThoiHan = 30,
            TrangThai = "DangBan",
        };
        _db.GoiDichVus.Add(goi);
        await _db.SaveChangesAsync();
        return ToDto(goi);
    }

    public async Task<GoiDichVuDto> SuaGoiAsync(int maGoi, GoiDichVuUpsertRequest request)
    {
        var goi = await _db.GoiDichVus.FindAsync(maGoi);
        if (goi is null)
            throw new BusinessRuleException(404, "Không tìm thấy gói dịch vụ.");

        var trung = await _db.GoiDichVus.AnyAsync(x => x.MaGoi != maGoi && x.TenGoi.ToLower() == request.TenGoi.Trim().ToLower());
        if (trung)
            throw new BusinessRuleException(400, "Tên gói dịch vụ đã tồn tại."); // MS63, BR26

        goi.TenGoi = request.TenGoi.Trim();
        goi.GioiHanTin = request.GioiHanTin;
        goi.CoNoiBat = request.CoNoiBat;
        goi.GiaTien = request.GiaTien;
        await _db.SaveChangesAsync();
        return ToDto(goi);
    }

    public async Task<string> XoaGoiAsync(int maGoi)
    {
        var goi = await _db.GoiDichVus.FindAsync(maGoi);
        if (goi is null)
            throw new BusinessRuleException(404, "Không tìm thấy gói dịch vụ.");

        var dangDuoc = await _db.GiaoDichMuaGois.AnyAsync(x => x.MaGoi == maGoi);
        if (dangDuoc)
        {
            goi.TrangThai = "NgungBan";
            await _db.SaveChangesAsync();
            return "Gói dịch vụ đã chuyển sang trạng thái ngừng bán."; // MS65, BR27
        }

        _db.GoiDichVus.Remove(goi);
        await _db.SaveChangesAsync();
        return "Xóa gói dịch vụ thành công."; // MS64
    }

    public async Task<DanhSachGoiResponse> LayDanhSachChoNtdAsync(int maTkNtd)
    {
        var danhSach = await _db.GoiDichVus.Where(x => x.TrangThai == "DangBan")
            .OrderBy(x => x.GiaTien)
            .Select(x => ToDto(x))
            .ToListAsync();

        return new DanhSachGoiResponse
        {
            GoiHienTai = await LayGoiHienTaiAsync(maTkNtd),
            DanhSachGoi = danhSach,
        };
    }

    public async Task<MuaGoiResponse> MuaGoiAsync(int maTkNtd, int maGoi, MuaGoiRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ThongTinThanhToan))
            throw new BusinessRuleException(400, "Thanh toán thất bại, vui lòng thử lại."); // MS61, Alt 4a
        if (request.PhuongThucThanhToan is not ("TheNganHang" or "ChuyenKhoan"))
            throw new BusinessRuleException(400, "Thanh toán thất bại, vui lòng thử lại."); // MS61

        var goi = await _db.GoiDichVus.FirstOrDefaultAsync(x => x.MaGoi == maGoi && x.TrangThai == "DangBan");
        if (goi is null)
            throw new BusinessRuleException(404, "Không tìm thấy gói dịch vụ.");

        var giaoDich = new GiaoDichMuaGoi
        {
            MaTK = maTkNtd,
            MaGoi = maGoi,
            NgayMua = DateTime.UtcNow,
            NgayHetHan = DateTime.UtcNow.AddDays(goi.ThoiHan), // BR25
            SoTien = goi.GiaTien,
            PhuongThucThanhToan = request.PhuongThucThanhToan,
            TrangThai = "ThanhCong",
        };
        _db.GiaoDichMuaGois.Add(giaoDich);
        await _db.SaveChangesAsync();

        return new MuaGoiResponse
        {
            Message = "Mua gói dịch vụ thành công.", // MS60
            GoiHienTai = await LayGoiHienTaiAsync(maTkNtd),
        };
    }

    public async Task<int> LayGioiHanHieuLucAsync(int maTkNtd)
    {
        var gioiHanCaoNhat = await _db.GiaoDichMuaGois
            .Where(x => x.MaTK == maTkNtd && x.TrangThai == "ThanhCong" && x.NgayHetHan >= DateTime.UtcNow)
            .Select(x => (int?)x.GoiDichVu.GioiHanTin)
            .MaxAsync();

        return gioiHanCaoNhat ?? GioiHanMienPhi; // QD18
    }

    private async Task<GoiHienTaiDto> LayGoiHienTaiAsync(int maTkNtd)
    {
        var giaoDichConHan = await _db.GiaoDichMuaGois
            .Include(x => x.GoiDichVu)
            .Where(x => x.MaTK == maTkNtd && x.TrangThai == "ThanhCong" && x.NgayHetHan >= DateTime.UtcNow)
            .OrderByDescending(x => x.GoiDichVu.GioiHanTin)
            .FirstOrDefaultAsync();

        if (giaoDichConHan is null)
            return new GoiHienTaiDto { TenGoi = "Miễn phí", GioiHanTin = GioiHanMienPhi, NgayHetHan = null };

        return new GoiHienTaiDto
        {
            TenGoi = giaoDichConHan.GoiDichVu.TenGoi,
            GioiHanTin = giaoDichConHan.GoiDichVu.GioiHanTin,
            NgayHetHan = giaoDichConHan.NgayHetHan,
        };
    }

    private static GoiDichVuDto ToDto(GoiDichVu x) => new()
    {
        MaGoi = x.MaGoi,
        TenGoi = x.TenGoi,
        GioiHanTin = x.GioiHanTin,
        CoNoiBat = x.CoNoiBat,
        GiaTien = x.GiaTien,
        ThoiHan = x.ThoiHan,
        TrangThai = x.TrangThai,
    };
}
