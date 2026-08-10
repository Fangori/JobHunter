namespace JobHunter.API.Models;

public class GoiDichVu
{
    public int MaGoi { get; set; }
    public string TenGoi { get; set; } = null!;
    public int GioiHanTin { get; set; }
    public bool CoNoiBat { get; set; }
    public decimal GiaTien { get; set; }
    public int ThoiHan { get; set; } = 30;
    public string TrangThai { get; set; } = "DangBan"; // DangBan/NgungBan
}
