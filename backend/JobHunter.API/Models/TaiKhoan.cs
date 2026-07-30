namespace JobHunter.API.Models;

public class TaiKhoan
{
    public int MaTK { get; set; }
    public string Email { get; set; } = null!;
    public string MatKhau { get; set; } = null!;
    public string VaiTro { get; set; } = null!; // UngVien / NhaTuyenDung / Admin
    public bool DaXacThuc { get; set; }
    public string TrangThai { get; set; } = "HoatDong"; // HoatDong / BiKhoa
    public int SoLanDangNhapSai { get; set; }
    public DateTime? KhoaTamThoiDenLuc { get; set; }
    public string? LyDoKhoa { get; set; }
    public DateTime NgayTao { get; set; }

    public UngVien? UngVien { get; set; }
    public NhaTuyenDung? NhaTuyenDung { get; set; }
}
