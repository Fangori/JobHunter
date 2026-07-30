namespace JobHunter.API.Models;

public class DonUngTuyen
{
    public int MaDon { get; set; }
    public int MaTin { get; set; }
    public int MaCV { get; set; }
    public string? ThuGioiThieu { get; set; }
    public DateTime NgayNop { get; set; }
    public string TrangThai { get; set; } = "DaNop"; // DaNop/DangXemXet/PhongVan/TuChoi/Nhan/DaHuy
    public string? GhiChuNoiBo { get; set; }
    public bool DaXem { get; set; }

    public TinTuyenDung TinTuyenDung { get; set; } = null!;
    public Cv Cv { get; set; } = null!;
}
