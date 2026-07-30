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
