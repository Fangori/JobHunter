namespace JobHunter.API.Models;

public class TinTuyenDung
{
    public int MaTin { get; set; }
    public int MaTK { get; set; } // FK -> NhaTuyenDung
    public string TieuDe { get; set; } = null!;
    public string MoTaCongViec { get; set; } = null!;
    public string? YeuCauUngVien { get; set; }
    public string? QuyenLoi { get; set; }
    public string? MucLuong { get; set; }
    public int? LuongToiThieu { get; set; } // trieu VND
    public int? LuongToiDa { get; set; } // trieu VND
    public int? SoLuongTuyen { get; set; }
    public string? DiaDiem { get; set; }
    public string? HinhThucLamViec { get; set; } // FullTime / PartTime / Remote
    public int? SoNamKinhNghiemYeuCau { get; set; }
    public DateTime NgayDang { get; set; }
    public DateOnly HanNopHoSo { get; set; }
    public string TrangThai { get; set; } = "ChoDuyet"; // ChoDuyet/DaDuyet/TuChoi/DaGo/DaDong
    public string? LyDoTuChoi { get; set; }
    public string? LyDoGo { get; set; }
    public int SoDonUngTuyen { get; set; }

    public NhaTuyenDung NhaTuyenDung { get; set; } = null!;
    public List<TinKyNang> TinKyNangs { get; set; } = new();
}
