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

// UC14 - goi y viec lam phu hop, tinh tren TUNG CV rieng, lay diem CAO NHAT
// giua cac CV cho moi tin (KHONG co khai niem "CV mac dinh")
public class ViecLamGoiYDto : TinTuyenDungSummaryDto
{
    public double PhanTramPhuHop { get; set; }
    public int MaCvPhuHopNhat { get; set; }
}

// UC14 - bao ca co CV hay chua, de phan biet MS28 ("chua co CV nao") voi
// truong hop co CV nhung khong co tin nao phu hop (im lang, khong phai loi)
public class GoiYViecLamResultDto
{
    public bool CoCv { get; set; }
    public List<ViecLamGoiYDto> GoiY { get; set; } = new();
}
