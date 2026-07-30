namespace JobHunter.API.Models;

public class DanhMucKyNang
{
    public int MaKyNang { get; set; }
    public string TenKyNang { get; set; } = null!;
    public string? NhomNganh { get; set; }
    public string TrangThai { get; set; } = "HoatDong"; // HoatDong / NgungSuDung
}
