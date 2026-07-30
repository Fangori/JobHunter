using JobHunter.API.Data;
using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Models;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.API.Services;

public class AuthService : IAuthService
{
    private readonly JobHunterDbContext _db;
    private readonly IThamSoService _thamSo;
    private readonly IJwtService _jwt;

    public AuthService(JobHunterDbContext db, IThamSoService thamSo, IJwtService jwt)
    {
        _db = db;
        _thamSo = thamSo;
        _jwt = jwt;
    }

    public async Task<DangKyResponse> DangKyUngVienAsync(DangKyUngVienRequest request)
    {
        if (request.MatKhau != request.XacNhanMatKhau)
            throw new BusinessRuleException(400, "Mật khẩu xác nhận không khớp.");

        await KiemTraEmailVaMatKhauAsync(request.Email, request.MatKhau); // QD01, MS13/MS14

        var taiKhoan = new TaiKhoan
        {
            Email = request.Email,
            MatKhau = BCrypt.Net.BCrypt.HashPassword(request.MatKhau),
            VaiTro = "UngVien",
            DaXacThuc = true, // khong gui email xac thuc that hom nay (QD03 & UC03 ngoai pham vi)
            TrangThai = "HoatDong",
            SoLanDangNhapSai = 0,
            NgayTao = DateTime.UtcNow,
        };
        _db.TaiKhoans.Add(taiKhoan);
        await _db.SaveChangesAsync();

        _db.UngViens.Add(new UngVien
        {
            MaTK = taiKhoan.MaTK,
            HoTen = request.HoTen,
            SDT = request.Sdt,
            SoCV = 0,
        });
        await _db.SaveChangesAsync();

        return new DangKyResponse { MaTK = taiKhoan.MaTK };
    }

    public async Task<DangKyResponse> DangKyNhaTuyenDungAsync(DangKyNhaTuyenDungRequest request)
    {
        if (request.MatKhau != request.XacNhanMatKhau)
            throw new BusinessRuleException(400, "Mật khẩu xác nhận không khớp.");

        await KiemTraEmailVaMatKhauAsync(request.Email, request.MatKhau); // QD02, MS13/MS14

        var taiKhoan = new TaiKhoan
        {
            Email = request.Email,
            MatKhau = BCrypt.Net.BCrypt.HashPassword(request.MatKhau),
            VaiTro = "NhaTuyenDung",
            DaXacThuc = true,
            TrangThai = "HoatDong",
            SoLanDangNhapSai = 0,
            NgayTao = DateTime.UtcNow,
        };
        _db.TaiKhoans.Add(taiKhoan);
        await _db.SaveChangesAsync();

        _db.NhaTuyenDungs.Add(new NhaTuyenDung
        {
            MaTK = taiKhoan.MaTK,
            TenCongTy = request.TenCongTy,
            DiaChi = request.DiaChi,
            SDT = request.Sdt,
            SoTinDangTuyen = 0,
        });
        await _db.SaveChangesAsync();

        return new DangKyResponse { MaTK = taiKhoan.MaTK };
    }

    public async Task<LoginResponse> DangNhapAsync(LoginRequest request)
    {
        var taiKhoan = await _db.TaiKhoans
            .Include(x => x.UngVien)
            .Include(x => x.NhaTuyenDung)
            .FirstOrDefaultAsync(x => x.Email == request.Email);

        if (taiKhoan is null)
            throw new BusinessRuleException(401, "Email và/hoặc mật khẩu không chính xác. Vui lòng kiểm tra và thử lại."); // MS01

        // QD03: dang trong thoi gian khoa tam -> tu choi du mat khau dung hay sai
        if (taiKhoan.KhoaTamThoiDenLuc.HasValue && taiKhoan.KhoaTamThoiDenLuc.Value > DateTime.UtcNow)
            throw new BusinessRuleException(403, "Tài khoản của bạn đã bị tạm khóa do nhập sai mật khẩu quá 5 lần. Vui lòng thử lại sau 15 phút."); // MS02

        var matKhauDung = BCrypt.Net.BCrypt.Verify(request.MatKhau, taiKhoan.MatKhau);
        if (!matKhauDung)
        {
            taiKhoan.SoLanDangNhapSai++;
            var soLanToiDa = await _thamSo.LayGiaTriIntAsync("TS2");
            if (taiKhoan.SoLanDangNhapSai >= soLanToiDa)
            {
                var soPhutKhoa = await _thamSo.LayGiaTriIntAsync("TS3");
                taiKhoan.KhoaTamThoiDenLuc = DateTime.UtcNow.AddMinutes(soPhutKhoa);
                await _db.SaveChangesAsync();
                throw new BusinessRuleException(403, "Tài khoản của bạn đã bị tạm khóa do nhập sai mật khẩu quá 5 lần. Vui lòng thử lại sau 15 phút."); // MS02
            }
            await _db.SaveChangesAsync();
            throw new BusinessRuleException(401, "Email và/hoặc mật khẩu không chính xác. Vui lòng kiểm tra và thử lại."); // MS01
        }

        taiKhoan.SoLanDangNhapSai = 0;
        taiKhoan.KhoaTamThoiDenLuc = null;
        await _db.SaveChangesAsync();

        var displayName = taiKhoan.VaiTro switch
        {
            "NhaTuyenDung" => taiKhoan.NhaTuyenDung?.TenCongTy ?? "",
            "UngVien" => taiKhoan.UngVien?.HoTen ?? "",
            _ => "Admin",
        };

        return new LoginResponse
        {
            Token = _jwt.GenerateToken(taiKhoan, displayName),
            VaiTro = taiKhoan.VaiTro,
            HoTenOrTenCongTy = displayName,
        };
    }

    private async Task KiemTraEmailVaMatKhauAsync(string email, string matKhau)
    {
        var trung = await _db.TaiKhoans.AnyAsync(x => x.Email == email);
        if (trung)
            throw new BusinessRuleException(400, "Email này đã được sử dụng. Vui lòng chọn email khác hoặc đăng nhập."); // MS13

        var doDaiToiThieu = await _thamSo.LayGiaTriIntAsync("TS1");
        var coChuSo = matKhau.Any(char.IsDigit);
        if (matKhau.Length < doDaiToiThieu || !coChuSo)
            throw new BusinessRuleException(400, "Mật khẩu phải có tối thiểu 8 ký tự và ít nhất 1 chữ số."); // MS14
    }
}
