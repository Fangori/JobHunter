namespace JobHunter.API.Models;

public class DanhMucNganhNghe
{
    public int MaNganhNghe { get; set; }
    public string TenNganhNghe { get; set; } = null!;
    public string TrangThai { get; set; } = "HoatDong"; // HoatDong / NgungSuDung
}
