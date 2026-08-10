namespace JobHunter.API.Models;

public class GiaoDichMuaGoi
{
    public int MaGiaoDich { get; set; }
    public int MaTK { get; set; } // FK -> NhaTuyenDung
    public int MaGoi { get; set; } // FK -> GoiDichVu
    public DateTime NgayMua { get; set; }
    public DateTime NgayHetHan { get; set; }
    public decimal SoTien { get; set; }
    public string PhuongThucThanhToan { get; set; } = null!; // TheNganHang/ChuyenKhoan
    public string TrangThai { get; set; } = "ThanhCong"; // ThanhCong/ThatBai

    public GoiDichVu GoiDichVu { get; set; } = null!;
}
