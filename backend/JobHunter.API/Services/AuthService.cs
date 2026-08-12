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
    private readonly IEmailService _email;

    public AuthService(JobHunterDbContext db, IThamSoService thamSo, IJwtService jwt, IEmailService email)
    {
        _db = db;
        _thamSo = thamSo;
        _jwt = jwt;
        _email = email;
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

        // Admin khoa vinh vien (Phase 12) - khac hoan toan voi khoa tam QD03 o tren
        if (taiKhoan.TrangThai == "BiKhoa")
            throw new BusinessRuleException(403, "Tài khoản của bạn đã bị khóa. Vui lòng liên hệ quản trị viên."); // MS11

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

        var chuKy = _jwt.KyTokenMucDich(token.MaToken, "DatLaiMatKhau", token.ThoiHanHetHan);
        await _email.GuiDatLaiMatKhauAsync(taiKhoan.Email, $"{token.MaToken}.{chuKy}");
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

        var chuKy = _jwt.KyTokenMucDich(token.MaToken, "XacThucEmail", token.ThoiHanHetHan);
        await _email.GuiXacThucEmailAsync(taiKhoan.Email, $"{token.MaToken}.{chuKy}");
    }

    // Token = "{MaToken}.{chu ky HMAC}" - MaToken (INT tu tang) doan duoc,
    // nhung khong the gia mao chu ky neu khong biet Jwt:Key (xem
    // IJwtService.KyTokenMucDich/XacMinhTokenMucDich). Khong luu chu ky vao
    // DB - tu tinh lai va so sanh moi lan verify.
    private async Task<TokenXacThuc> LayTokenHopLeAsync(string tokenValue, string loaiToken)
    {
        var parts = tokenValue.Split('.', 2);
        if (parts.Length != 2 || !int.TryParse(parts[0], out var maToken))
            throw new BusinessRuleException(400, LayThongBaoTokenSai(loaiToken));
        var chuKy = parts[1];

        var token = await _db.TokenXacThucs.FirstOrDefaultAsync(x => x.MaToken == maToken && x.LoaiToken == loaiToken);
        if (token is null || token.DaSuDung || token.ThoiHanHetHan < DateTime.UtcNow)
            throw new BusinessRuleException(400, LayThongBaoTokenSai(loaiToken));

        if (!_jwt.XacMinhTokenMucDich(chuKy, token.MaToken, token.LoaiToken, token.ThoiHanHetHan))
            throw new BusinessRuleException(400, LayThongBaoTokenSai(loaiToken));

        return token;
    }

    private static string LayThongBaoTokenSai(string loaiToken) => loaiToken == "XacThucEmail"
        ? "Liên kết xác thực không hợp lệ hoặc đã hết hạn. Vui lòng yêu cầu gửi lại." // MS19
        : "Liên kết đặt lại mật khẩu đã hết hạn. Vui lòng thực hiện lại."; // MS23
}
