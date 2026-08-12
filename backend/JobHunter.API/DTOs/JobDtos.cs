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
    // MucLuong (chuoi hien thi) khong nhan tu client nua - Service tu sinh
    // tu LuongToiThieu/LuongToiDa (gop y bao cao 11-12/08: form nhap
    // "khoang Luong toi thieu/toi da" dang so thay vi go text tu do).
    public int? LuongToiThieu { get; set; } // trieu VND
    public int? LuongToiDa { get; set; } // trieu VND
    public int? SoLuongTuyen { get; set; }
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
    public int MaTkNtd { get; set; }
    public string TenCongTy { get; set; } = null!;
    // Logo cua NHA_TUYEN_DUNG (cot da co san trong schema) - de the tin
    // tuyen dung hien duoc logo that thay vi luon la chu cai dau placeholder.
    public string? Logo { get; set; }
    // MaNganhNghe cua NHA_TUYEN_DUNG (cot da co san) - frontend dung de chon
    // anh minh hoa phu hop nganh nghe cho the tin, khong luu them gi vao DB.
    public int? MaNganhNghe { get; set; }
    public string? DiaDiem { get; set; }
    public string? MucLuong { get; set; }
    public int? LuongToiThieu { get; set; }
    public int? LuongToiDa { get; set; }
    public int? SoLuongTuyen { get; set; }
    public string? HinhThucLamViec { get; set; }
    public DateTime NgayDang { get; set; }
    public DateOnly HanNopHoSo { get; set; }
    public string TrangThai { get; set; } = null!;
}

public class TinTuyenDungDetailDto : TinTuyenDungSummaryDto
{
    // Anh bia cong ty NTD tu chon (UC08) - de trang Chi tiet viec lam hien
    // dung banner NTD da chon thay vi luon roi vao mac dinh xoay theo MaTK.
    public string? AnhBiaCongTy { get; set; }
    public string MoTaCongViec { get; set; } = null!;
    public string? YeuCauUngVien { get; set; }
    public string? QuyenLoi { get; set; }
    public int? SoNamKinhNghiemYeuCau { get; set; }
    public List<KyNangYeuCauDto> KyNangYeuCau { get; set; } = new();
}

// UC10 (LAB4) - tim kiem/loc nang cao: loai hinh cong viec, khoang luong,
// sap xep, phan trang theo so trang. Endpoint rieng (GET /api/jobs/search)
// de khong doi shape cua GET /api/jobs cu (dang duoc IntegrationTests.cs
// va cac noi khac dung nguyen dang mang phang).
public class TimKiemVaLocRequest
{
    public string? Keyword { get; set; }
    public string? DiaDiem { get; set; }
    public int? MaNganhNghe { get; set; }
    public List<string>? HinhThucLamViec { get; set; }
    public int? LuongMin { get; set; } // trieu VND
    public int? LuongMax { get; set; } // trieu VND
    public string SortBy { get; set; } = "moi_nhat"; // moi_nhat | luong_giam | luong_tang
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 9;
}

public class DanhSachViecLamPhanTrangDto
{
    public List<TinTuyenDungSummaryDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
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

// UC27/BR24 - gia han tin
public class GiaHanRequest
{
    [Required] public DateOnly HanNopMoi { get; set; }
}

// UC26 - message phu thuoc co bi BR15 dua ve ChoDuyet hay khong, quyet dinh o Service (noi biet trang thai truoc khi sua)
public class SuaTinResponse
{
    public TinTuyenDungSummaryDto Tin { get; set; } = null!;
    public string Message { get; set; } = null!;
}
