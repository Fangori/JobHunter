using System.ComponentModel.DataAnnotations;

namespace JobHunter.API.DTOs;

public class CvKyNangDto
{
    [Required] public int MaKyNang { get; set; }
    public string? MucDoThanhThao { get; set; } // CoBan / Kha / ThanhThao
}

public class CvKinhNghiemDto
{
    public string CongTy { get; set; } = null!;
    public string? ViTri { get; set; }
    public DateOnly TuNgay { get; set; }
    public DateOnly? DenNgay { get; set; }
    public string? MoTaCongViec { get; set; }
}

public class CvHocVanDto
{
    public string Truong { get; set; } = null!;
    public string? ChuyenNganh { get; set; }
    public int? TuNam { get; set; }
    public int? DenNam { get; set; }
}

// Field dung theo BM06, cong them "Trinh do hoc van" (lo hong da phat hien va sua trong docs/IMPLEMENTATION_PLAN.md muc 2)
public class TaoCvTrucTuyenRequest
{
    [Required] public string TenCv { get; set; } = null!;
    public string? ViTriMongMuon { get; set; }
    public string? MucLuongMongMuon { get; set; }
    public string? TrinhDoHocVan { get; set; } // TrungCap/CaoDang/DaiHoc/SauDaiHoc
    public List<CvKyNangDto> KyNang { get; set; } = new();
    public List<CvKinhNghiemDto> KinhNghiem { get; set; } = new();
    public List<CvHocVanDto> HocVan { get; set; } = new();
}

public class CvSummaryDto
{
    public int MaCV { get; set; }
    public string TenCV { get; set; } = null!;
    public string LoaiCV { get; set; } = null!;
    public string? ViTriMongMuon { get; set; }
    public string? TrinhDoHocVan { get; set; }
    public string TrangThai { get; set; } = null!;
    public string? DuongDanFile { get; set; }
}

// UC22 - doc chi tiet 1 CV de preload form sua
public class CvDetailDto : CvSummaryDto
{
    public string? MucLuongMongMuon { get; set; }
    public List<CvKyNangDto> KyNang { get; set; } = new();
    public List<CvKinhNghiemDto> KinhNghiem { get; set; } = new();
    public List<CvHocVanDto> HocVan { get; set; } = new();
}
