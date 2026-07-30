using JobHunter.API.Data;
using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.API.Services;

public class AdminAccountService : IAdminAccountService
{
    private readonly JobHunterDbContext _db;
    private readonly INotificationService _notification;

    public AdminAccountService(JobHunterDbContext db, INotificationService notification)
    {
        _db = db;
        _notification = notification;
    }

    public async Task<List<AdminAccountDto>> LayDanhSachAsync(string? vaiTro)
    {
        var query = _db.TaiKhoans
            .Include(x => x.UngVien)
            .Include(x => x.NhaTuyenDung)
            .Where(x => x.VaiTro != "Admin")
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(vaiTro))
            query = query.Where(x => x.VaiTro == vaiTro);

        return await query.OrderByDescending(x => x.NgayTao)
            .Select(x => new AdminAccountDto
            {
                MaTk = x.MaTK,
                Email = x.Email,
                VaiTro = x.VaiTro,
                TrangThai = x.TrangThai,
                LyDoKhoa = x.LyDoKhoa,
                HoTenOrTenCongTy = x.VaiTro == "NhaTuyenDung" ? x.NhaTuyenDung!.TenCongTy : x.UngVien!.HoTen,
                NgayTao = x.NgayTao,
            })
            .ToListAsync();
    }

    public async Task<string> KhoaTaiKhoanAsync(int maTk, string lyDo)
    {
        if (string.IsNullOrWhiteSpace(lyDo))
            throw new BusinessRuleException(400, "Vui lòng nhập lý do khóa tài khoản."); // MS55, BR18

        var taiKhoan = await _db.TaiKhoans.FindAsync(maTk);
        if (taiKhoan is null || taiKhoan.VaiTro == "Admin")
            throw new BusinessRuleException(404, "Không tìm thấy tài khoản.");

        taiKhoan.TrangThai = "BiKhoa";
        taiKhoan.LyDoKhoa = lyDo;
        await _db.SaveChangesAsync();

        await _notification.TaoThongBaoAsync(taiKhoan.MaTK,
            $"Tài khoản của bạn đã bị khóa. Lý do: {lyDo}", "TaiKhoanBiKhoa", "/login");

        // MS47 (NTD) / MS48 (Ung vien) - cung 1 chuoi chu, giu 2 hang so rieng dung thiet ke da chot
        return taiKhoan.VaiTro == "NhaTuyenDung"
            ? "Cập nhật trạng thái tài khoản thành công." // MS47
            : "Cập nhật trạng thái tài khoản thành công."; // MS48
    }

    public async Task MoKhoaTaiKhoanAsync(int maTk)
    {
        var taiKhoan = await _db.TaiKhoans.FindAsync(maTk);
        if (taiKhoan is null || taiKhoan.VaiTro == "Admin")
            throw new BusinessRuleException(404, "Không tìm thấy tài khoản.");

        taiKhoan.TrangThai = "HoatDong";
        taiKhoan.LyDoKhoa = null;
        await _db.SaveChangesAsync();

        await _notification.TaoThongBaoAsync(taiKhoan.MaTK,
            "Tài khoản của bạn đã được mở khóa.", "TaiKhoanDuocMoKhoa", "/login");
    }
}
