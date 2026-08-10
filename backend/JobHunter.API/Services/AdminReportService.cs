using JobHunter.API.Data;
using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.API.Services;

public class AdminReportService : IAdminReportService
{
    private readonly JobHunterDbContext _db;

    public AdminReportService(JobHunterDbContext db)
    {
        _db = db;
    }

    // BR23/QD17: bao cao tinh tu ngay 01 den ngay cuoi cung cua thang da chon
    public async Task<BaoCaoThangDto> LayBaoCaoThangAsync(int thang, int nam)
    {
        if (thang is < 1 or > 12)
            throw new BusinessRuleException(400, "Tháng không hợp lệ.");

        var tuNgay = new DateTime(nam, thang, 1, 0, 0, 0, DateTimeKind.Utc);
        var denNgay = tuNgay.AddMonths(1);

        var soUngVienMoi = await _db.TaiKhoans.CountAsync(x => x.VaiTro == "UngVien" && x.NgayTao >= tuNgay && x.NgayTao < denNgay);
        var soNtdMoi = await _db.TaiKhoans.CountAsync(x => x.VaiTro == "NhaTuyenDung" && x.NgayTao >= tuNgay && x.NgayTao < denNgay);
        var soTinMoi = await _db.TinTuyenDungs.CountAsync(x => x.NgayDang >= tuNgay && x.NgayDang < denNgay);
        var soDonMoi = await _db.DonUngTuyens.CountAsync(x => x.NgayNop >= tuNgay && x.NgayNop < denNgay);
        var soDoanhThu = await _db.GiaoDichMuaGois
            .Where(x => x.TrangThai == "ThanhCong" && x.NgayMua >= tuNgay && x.NgayMua < denNgay)
            .SumAsync(x => (decimal?)x.SoTien) ?? 0m; // BR24

        return new BaoCaoThangDto
        {
            Thang = thang,
            Nam = nam,
            ChiTieu = new List<ChiTieuBaoCaoDto>
            {
                new() { Ten = "Tài khoản Ứng viên mới", SoLuong = soUngVienMoi },
                new() { Ten = "Tài khoản NTD mới", SoLuong = soNtdMoi },
                new() { Ten = "Tin tuyển dụng mới", SoLuong = soTinMoi },
                new() { Ten = "Đơn ứng tuyển mới", SoLuong = soDonMoi },
                new() { Ten = "Doanh thu gói dịch vụ", SoLuong = soDoanhThu },
            },
        };
    }
}
