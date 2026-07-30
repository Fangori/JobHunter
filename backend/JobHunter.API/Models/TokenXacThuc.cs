namespace JobHunter.API.Models;

public class TokenXacThuc
{
    public int MaToken { get; set; }
    public int MaTK { get; set; }
    public string LoaiToken { get; set; } = null!; // XacThucEmail / DatLaiMatKhau
    public DateTime ThoiHanHetHan { get; set; }
    public bool DaSuDung { get; set; }
}
