using System.ComponentModel.DataAnnotations;

namespace JobHunter.API.DTOs;

// UC37/38 - quan ly tai khoan NTD/Ung vien
public class AdminAccountDto
{
    public int MaTk { get; set; }
    public string Email { get; set; } = null!;
    public string VaiTro { get; set; } = null!;
    public string TrangThai { get; set; } = null!;
    public string? LyDoKhoa { get; set; }
    public string HoTenOrTenCongTy { get; set; } = null!;
    public DateTime NgayTao { get; set; }
}

// BR18 - ly do khoa bat buoc
public class LockAccountRequest
{
    [Required] public string LyDo { get; set; } = null!;
}

// UC39/40 - danh muc Ky nang
public class DanhMucKyNangUpsertRequest
{
    [Required] public string TenKyNang { get; set; } = null!;
    public string? NhomNganh { get; set; }
}

public class AdminSkillDto
{
    public int MaKyNang { get; set; }
    public string TenKyNang { get; set; } = null!;
    public string? NhomNganh { get; set; }
    public string TrangThai { get; set; } = null!;
}

// UC41/42 - danh muc Nganh nghe
public class DanhMucNganhNgheUpsertRequest
{
    [Required] public string TenNganhNghe { get; set; } = null!;
}

public class AdminIndustryDto
{
    public int MaNganhNghe { get; set; }
    public string TenNganhNghe { get; set; } = null!;
    public string TrangThai { get; set; } = null!;
}

// UC43 - bao cao thang (BR23/QD17)
public class ChiTieuBaoCaoDto
{
    public string Ten { get; set; } = null!;
    public decimal SoLuong { get; set; }
}

public class BaoCaoThangDto
{
    public int Thang { get; set; }
    public int Nam { get; set; }
    public List<ChiTieuBaoCaoDto> ChiTieu { get; set; } = new();
}

// LAB4 - "bieu do phan bo vai tro nguoi dung" o trang tong quan Admin.
// Tong so tai khoan THEO VAI TRO tinh den hien tai (khong theo thang/nam
// nhu BaoCaoThangDto - day la snapshot, khong phai bao cao ky).
public class PhanBoVaiTroDto
{
    public int SoUngVien { get; set; }
    public int SoNhaTuyenDung { get; set; }
    public int SoAdmin { get; set; }
}
