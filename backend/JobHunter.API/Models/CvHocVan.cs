namespace JobHunter.API.Models;

public class CvHocVan
{
    public int MaHocVan { get; set; }
    public int MaCV { get; set; }
    public string Truong { get; set; } = null!;
    public string? ChuyenNganh { get; set; }
    public int? TuNam { get; set; }
    public int? DenNam { get; set; }
}
