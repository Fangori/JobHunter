namespace JobHunter.API.DTOs;

public class UngVienPhuHopDto
{
    public int MaDon { get; set; }
    public string HoTen { get; set; } = null!;
    public string TenCV { get; set; } = null!;
    public double PhanTramPhuHop { get; set; }
    public List<string> KyNangKhop { get; set; } = new();
    public int SoNamKinhNghiem { get; set; }
    public string? TrinhDoHocVan { get; set; }
    public string TrangThai { get; set; } = null!;
}
