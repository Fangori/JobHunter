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
