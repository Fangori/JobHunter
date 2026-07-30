using System.ComponentModel.DataAnnotations;

namespace JobHunter.API.DTOs;

// Field dung theo BM01 (Dang ky Ung vien)
public class DangKyUngVienRequest
{
    [Required] public string HoTen { get; set; } = null!;
    [Required] public string MatKhau { get; set; } = null!;
    [Required, EmailAddress] public string Email { get; set; } = null!;
    [Required] public string XacNhanMatKhau { get; set; } = null!;
    public string? Sdt { get; set; }
}

// Field dung theo BM02 (Dang ky Nha tuyen dung) - KHONG co Ma so thue
public class DangKyNhaTuyenDungRequest
{
    [Required] public string TenCongTy { get; set; } = null!;
    [Required] public string DiaChi { get; set; } = null!;
    [Required, EmailAddress] public string Email { get; set; } = null!;
    [Required] public string MatKhau { get; set; } = null!;
    public string? Sdt { get; set; }
    [Required] public string XacNhanMatKhau { get; set; } = null!;
}

public class DangKyResponse
{
    public int MaTK { get; set; }
    // MS12 nguyen van (Phase 7: UC03 that, khong con auto-verify nhu Phase 1)
    public string Message { get; set; } = "Đăng ký thành công. Vui lòng kiểm tra email để xác thực tài khoản.";
}

public class ErrorResponse
{
    public string Message { get; set; } = null!;
}

public class LoginRequest
{
    [Required, EmailAddress] public string Email { get; set; } = null!;
    [Required] public string MatKhau { get; set; } = null!;
}

public class LoginResponse
{
    public string Token { get; set; } = null!;
    public string VaiTro { get; set; } = null!;
    public string HoTenOrTenCongTy { get; set; } = null!;
}

public class VerifyEmailRequest
{
    [Required] public string Token { get; set; } = null!;
}

public class ResendVerificationRequest
{
    [Required, EmailAddress] public string Email { get; set; } = null!;
}

public class ForgotPasswordRequest
{
    [Required, EmailAddress] public string Email { get; set; } = null!;
}

public class ResetPasswordRequest
{
    [Required] public string Token { get; set; } = null!;
    [Required] public string MatKhauMoi { get; set; } = null!;
    [Required] public string XacNhanMatKhauMoi { get; set; } = null!;
}

public class MessageResponse
{
    public string Message { get; set; } = null!;
}
