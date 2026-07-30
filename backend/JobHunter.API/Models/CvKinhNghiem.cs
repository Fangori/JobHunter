namespace JobHunter.API.Models;

public class CvKinhNghiem
{
    public int MaKinhNghiem { get; set; }
    public int MaCV { get; set; }
    public string CongTy { get; set; } = null!;
    public string? ViTri { get; set; }
    public DateOnly TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; } // null = dang lam viec
    public string? MoTaCongViec { get; set; }
}
