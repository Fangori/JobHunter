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
    public string Message { get; set; } = "Đăng ký thành công."; // MS12 (đã sửa, không gửi email thật hôm nay)
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
