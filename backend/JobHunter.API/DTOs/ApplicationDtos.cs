using System.ComponentModel.DataAnnotations;

namespace JobHunter.API.DTOs;

// Field dung theo BM10
public class UngTuyenRequest
{
    [Required] public int MaCv { get; set; }
    [Required] public int MaTin { get; set; }
    public string? ThuGioiThieu { get; set; }
}

public class DonUngTuyenResponse
{
    public int MaDon { get; set; }
    public string TrangThai { get; set; } = null!;
}

public class DonUngTuyenMineDto
{
    public int MaDon { get; set; }
    public int MaTin { get; set; }
    public string TieuDe { get; set; } = null!;
    public string TenCongTy { get; set; } = null!;
    public string TrangThai { get; set; } = null!;
    public DateTime NgayNop { get; set; }
}

// UC32 - NTD xem chi tiet 1 don ung tuyen (CV day du)
public class DonUngTuyenDetailDto
{
    public int MaDon { get; set; }
    public string TrangThai { get; set; } = null!;
    public string? ThuGioiThieu { get; set; }
    public DateTime NgayNop { get; set; }
    public string? GhiChuNoiBo { get; set; }
    public string HoTenUngVien { get; set; } = null!;
    public CvDetailDto Cv { get; set; } = null!;
}

// UC33 - cap nhat trang thai don (BR05/QD11)
public class CapNhatTrangThaiRequest
{
    [Required] public string TrangThaiMoi { get; set; } = null!;
    public string? GhiChuNoiBo { get; set; }
}
