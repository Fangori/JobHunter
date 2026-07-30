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
            DaXacThuc = false,
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

        await TaoVaGuiTokenXacThucAsync(taiKhoan);

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
            DaXacThuc = false,
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

        await TaoVaGuiTokenXacThucAsync(taiKhoan);

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

        // Mat khau DUNG nhung chua xac thuc email -> MS10 (chi kiem tra sau khi
        // biet mat khau dung, tranh lo email ton tai qua thong bao khac nhau)
        if (!taiKhoan.DaXacThuc)
            throw new BusinessRuleException(403, "Tài khoản chưa xác thực email. Vui lòng kiểm tra email hoặc bấm gửi lại liên kết xác thực."); // MS10

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

    public async Task VerifyEmailAsync(string token)
    {
        var tokenEntity = await LayTokenHopLeAsync(token, "XacThucEmail"); // throw MS19 neu sai/het han

        var taiKhoan = await _db.TaiKhoans.FindAsync(tokenEntity.MaTK);
        taiKhoan!.DaXacThuc = true;
        tokenEntity.DaSuDung = true;
        await _db.SaveChangesAsync();
    }

    public async Task ResendVerificationAsync(string email)
    {
        var taiKhoan = await _db.TaiKhoans.FirstOrDefaultAsync(x => x.Email == email);
        if (taiKhoan is null)
            throw new BusinessRuleException(404, "Email không tồn tại trong hệ thống."); // MS22 (dung chung wording)
        if (taiKhoan.DaXacThuc)
            throw new BusinessRuleException(400, "Tài khoản đã được xác thực trước đó.");

        await TaoVaGuiTokenXacThucAsync(taiKhoan);
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var taiKhoan = await _db.TaiKhoans.FirstOrDefaultAsync(x => x.Email == email);
        if (taiKhoan is null)
            throw new BusinessRuleException(404, "Email không tồn tại trong hệ thống."); // MS22

        var soPhutHieuLuc = await _thamSo.LayGiaTriIntAsync("TS4"); // BR08
        var token = new TokenXacThuc
        {
            MaTK = taiKhoan.MaTK,
            LoaiToken = "DatLaiMatKhau",
            ThoiHanHetHan = DateTime.UtcNow.AddMinutes(soPhutHieuLuc),
            DaSuDung = false,
        };
        _db.TokenXacThucs.Add(token);
        await _db.SaveChangesAsync();

        Console.WriteLine($"[EMAIL MOCK] Gửi tới {taiKhoan.Email}: http://localhost:5173/reset-password?token={token.MaToken}-{Guid.NewGuid():N}");
        // Luu y: token that nen la chuoi ngau nhien rieng, dung MaToken lam
        // dinh danh don gian cho demo (xem GhiChu o LayTokenHopLeAsync)
    }

    public async Task ResetPasswordAsync(string token, string matKhauMoi, string xacNhanMatKhauMoi)
    {
        if (matKhauMoi != xacNhanMatKhauMoi)
            throw new BusinessRuleException(400, "Mật khẩu xác nhận không khớp.");

        var tokenEntity = await LayTokenHopLeAsync(token, "DatLaiMatKhau"); // MS23 neu sai/het han

        var doDaiToiThieu = await _thamSo.LayGiaTriIntAsync("TS1");
        var coChuSo = matKhauMoi.Any(char.IsDigit);
        if (matKhauMoi.Length < doDaiToiThieu || !coChuSo)
            throw new BusinessRuleException(400, "Mật khẩu phải có tối thiểu 8 ký tự và ít nhất 1 chữ số."); // MS14

        var taiKhoan = await _db.TaiKhoans.FindAsync(tokenEntity.MaTK);
        taiKhoan!.MatKhau = BCrypt.Net.BCrypt.HashPassword(matKhauMoi);
        tokenEntity.DaSuDung = true;
        await _db.SaveChangesAsync();
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

    private async Task TaoVaGuiTokenXacThucAsync(TaiKhoan taiKhoan)
    {
        var soPhutHieuLuc = await _thamSo.LayGiaTriIntAsync("TS4");
        var token = new TokenXacThuc
        {
            MaTK = taiKhoan.MaTK,
            LoaiToken = "XacThucEmail",
            ThoiHanHetHan = DateTime.UtcNow.AddMinutes(soPhutHieuLuc),
            DaSuDung = false,
        };
        _db.TokenXacThucs.Add(token);
        await _db.SaveChangesAsync();

        Console.WriteLine($"[EMAIL MOCK] Gửi tới {taiKhoan.Email}: http://localhost:5173/verify-email?token={token.MaToken}");
    }

    // Ghi chu: dung thang MaToken (int, tu tang) lam gia tri token trong URL de
    // don gian hoa demo (khong nham lan voi token that dang bao mat cao hon
    // dang chuoi ngau nhien) - chap nhan duoc vi UC03/UC06 chi mock/log console
    // hom nay, khong gui email that (xem quyet dinh da chot voi nguoi dung).
    private async Task<TokenXacThuc> LayTokenHopLeAsync(string tokenValue, string loaiToken)
    {
        if (!int.TryParse(tokenValue.Split('-')[0], out var maToken))
            throw new BusinessRuleException(400, LayThongBaoTokenSai(loaiToken));

        var token = await _db.TokenXacThucs.FirstOrDefaultAsync(x => x.MaToken == maToken && x.LoaiToken == loaiToken);
        if (token is null || token.DaSuDung || token.ThoiHanHetHan < DateTime.UtcNow)
            throw new BusinessRuleException(400, LayThongBaoTokenSai(loaiToken));

        return token;
    }

    private static string LayThongBaoTokenSai(string loaiToken) => loaiToken == "XacThucEmail"
        ? "Liên kết xác thực không hợp lệ hoặc đã hết hạn. Vui lòng yêu cầu gửi lại." // MS19
        : "Liên kết đặt lại mật khẩu đã hết hạn. Vui lòng thực hiện lại."; // MS23
}
