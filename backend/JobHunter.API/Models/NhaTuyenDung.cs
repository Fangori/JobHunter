namespace JobHunter.API.Models;

public class NhaTuyenDung
{
    public int MaTK { get; set; } // PK, cung la FK -> TaiKhoan
    public string TenCongTy { get; set; } = null!;
    public string? MaSoThue { get; set; }
    public string? Logo { get; set; }
    public string? AnhBia { get; set; } // key anh banner tu chon tu bo curated (khong phai URL)
    public int? MaNganhNghe { get; set; }
    public string? DiaChi { get; set; }
    public string? Website { get; set; }
    public string? GioiThieuCongTy { get; set; }
    public string? SDT { get; set; }
    public string? QuyMo { get; set; } // <50 / 50-200 / 200-500 / >500
    public int SoTinDangTuyen { get; set; }

    public TaiKhoan TaiKhoan { get; set; } = null!;
}
