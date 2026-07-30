namespace JobHunter.API.Models;

public class ThongBao
{
    public int MaThongBao { get; set; }
    public int MaTK { get; set; }
    public string NoiDung { get; set; } = null!;
    public string LoaiThongBao { get; set; } = null!;
    public bool DaDoc { get; set; }
    public DateTime NgayTao { get; set; }
    public string? LienKet { get; set; }
}
