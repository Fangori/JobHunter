using System.ComponentModel.DataAnnotations;

namespace JobHunter.API.DTOs;

public class GoiDichVuUpsertRequest
{
    [Required] public string TenGoi { get; set; } = null!;
    [Range(1, int.MaxValue)] public int GioiHanTin { get; set; }
    public bool CoNoiBat { get; set; }
    [Range(0, double.MaxValue)] public decimal GiaTien { get; set; }
}

public class GoiDichVuDto
{
    public int MaGoi { get; set; }
    public string TenGoi { get; set; } = null!;
    public int GioiHanTin { get; set; }
    public bool CoNoiBat { get; set; }
    public decimal GiaTien { get; set; }
    public int ThoiHan { get; set; }
    public string TrangThai { get; set; } = null!;
}

public class GoiHienTaiDto
{
    public string TenGoi { get; set; } = null!; // "Mien phi" neu chua tung mua/da het han
    public int GioiHanTin { get; set; }
    public DateTime? NgayHetHan { get; set; } // null neu la goi Mien phi
}

public class DanhSachGoiResponse
{
    public GoiHienTaiDto GoiHienTai { get; set; } = null!;
    public List<GoiDichVuDto> DanhSachGoi { get; set; } = new();
}

public class MuaGoiRequest
{
    [Required] public string PhuongThucThanhToan { get; set; } = null!; // TheNganHang/ChuyenKhoan
    [Required] public string ThongTinThanhToan { get; set; } = null!;   // gia lap, chi can khong rong
}

public class MuaGoiResponse
{
    public string Message { get; set; } = null!;
    public GoiHienTaiDto GoiHienTai { get; set; } = null!;
}
