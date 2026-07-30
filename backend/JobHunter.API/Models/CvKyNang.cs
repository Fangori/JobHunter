namespace JobHunter.API.Models;

public class CvKyNang
{
    public int MaCV { get; set; }
    public int MaKyNang { get; set; }
    public string? MucDoThanhThao { get; set; } // CoBan / Kha / ThanhThao

    public Cv Cv { get; set; } = null!;
    public DanhMucKyNang DanhMucKyNang { get; set; } = null!;
}
