using System.ComponentModel.DataAnnotations;

namespace JobHunter.API.DTOs;

public class KyNangYeuCauDto
{
    [Required] public int MaKyNang { get; set; }
    public string? MucDoUuTien { get; set; } // BatBuoc / UuTien
}

// Field dung theo BM09 (Dang tin tuyen dung)
public class DangTinRequest
{
    [Required] public string TieuDe { get; set; } = null!;
    [Required] public string MoTaCongViec { get; set; } = null!;
    public string? YeuCauUngVien { get; set; }
    public string? QuyenLoi { get; set; }
    public string? MucLuong { get; set; }
    public string? DiaDiem { get; set; }
    public string? HinhThucLamViec { get; set; }
    public int? SoNamKinhNghiemYeuCau { get; set; }
    [Required] public DateOnly HanNopHoSo { get; set; }
    public List<KyNangYeuCauDto> KyNangYeuCau { get; set; } = new();
}

public class TinTuyenDungSummaryDto
{
    public int MaTin { get; set; }
    public string TieuDe { get; set; } = null!;
    public string TenCongTy { get; set; } = null!;
    public string? DiaDiem { get; set; }
    public string? MucLuong { get; set; }
    public string? HinhThucLamViec { get; set; }
    public DateTime NgayDang { get; set; }
    public DateOnly HanNopHoSo { get; set; }
    public string TrangThai { get; set; } = null!;
}

public class TinTuyenDungDetailDto : TinTuyenDungSummaryDto
{
    public string MoTaCongViec { get; set; } = null!;
    public string? YeuCauUngVien { get; set; }
    public string? QuyenLoi { get; set; }
    public int? SoNamKinhNghiemYeuCau { get; set; }
    public List<KyNangYeuCauDto> KyNangYeuCau { get; set; } = new();
}

public class TuChoiTinRequest
{
    [Required] public string LyDo { get; set; } = null!;
}

public class PendingStatsResponse
{
    public int SoChoDuyet { get; set; }
    public int SoDaDuyet { get; set; }
}

public class DanhMucKyNangDto
{
    public int MaKyNang { get; set; }
    public string TenKyNang { get; set; } = null!;
}
