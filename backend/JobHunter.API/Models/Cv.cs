namespace JobHunter.API.Models;

public class Cv
{
    public int MaCV { get; set; }
    public int MaTK { get; set; } // FK -> UngVien
    public string TenCV { get; set; } = null!;
    public string LoaiCV { get; set; } = null!; // TrucTuyen / Upload
    public string? DuongDanFile { get; set; }
    public string? TrinhDoHocVan { get; set; } // TrungCap/CaoDang/DaiHoc/SauDaiHoc
    public string? ViTriMongMuon { get; set; }
    public string? MucLuongMongMuon { get; set; }
    public string TrangThai { get; set; } = "HoatDong"; // HoatDong / DaAn
    public DateTime NgayTao { get; set; }

    public UngVien UngVien { get; set; } = null!;
    public List<CvKyNang> CvKyNangs { get; set; } = new();
    public List<CvKinhNghiem> CvKinhNghiems { get; set; } = new();
    public List<CvHocVan> CvHocVans { get; set; } = new();
}
