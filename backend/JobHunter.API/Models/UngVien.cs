namespace JobHunter.API.Models;

public class UngVien
{
    public int MaTK { get; set; } // PK, cung la FK -> TaiKhoan
    public string HoTen { get; set; } = null!;
    public DateOnly? NgaySinh { get; set; }
    public string? SDT { get; set; }
    public string? DiaChi { get; set; }
    public string? AnhDaiDien { get; set; }
    public string? GioiThieuBanThan { get; set; }
    public int SoCV { get; set; }

    public TaiKhoan TaiKhoan { get; set; } = null!;
}
