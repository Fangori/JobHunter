namespace JobHunter.API.Models;

public class TinKyNang
{
    public int MaTin { get; set; }
    public int MaKyNang { get; set; }
    public string? MucDoUuTien { get; set; } // BatBuoc / UuTien

    public TinTuyenDung TinTuyenDung { get; set; } = null!;
    public DanhMucKyNang DanhMucKyNang { get; set; } = null!;
}
