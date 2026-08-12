namespace JobHunter.API.DTOs;

// BM05 - Thong tin ca nhan (Ung vien, UC07)
public class CandidateProfileDto
{
    public string HoTen { get; set; } = null!;
    public DateOnly? NgaySinh { get; set; }
    public string? Sdt { get; set; }
    public string? AnhDaiDien { get; set; }
    public string? DiaChi { get; set; }
    public string? GioiThieuBanThan { get; set; }
}

public class CapNhatCandidateProfileRequest
{
    public string HoTen { get; set; } = null!;
    public DateOnly? NgaySinh { get; set; }
    public string? Sdt { get; set; }
    public string? DiaChi { get; set; }
    public string? GioiThieuBanThan { get; set; }
}

// BM08 - Ho so cong ty (Nha tuyen dung, UC08)
public class EmployerProfileDto
{
    public string TenCongTy { get; set; } = null!;
    public string? Logo { get; set; }
    public string? AnhBia { get; set; }
    public string? QuyMo { get; set; }
    public int? MaNganhNghe { get; set; }
    public string? DiaChi { get; set; }
    public string? Website { get; set; }
    public string? GioiThieuCongTy { get; set; }
}

public class CapNhatEmployerProfileRequest
{
    public string TenCongTy { get; set; } = null!;
    public string? QuyMo { get; set; }
    public int? MaNganhNghe { get; set; }
    public string? DiaChi { get; set; }
    public string? Website { get; set; }
    public string? GioiThieuCongTy { get; set; }
}

// UC12 - Xem ho so cong ty cong khai
public class EmployerPublicProfileDto : EmployerProfileDto
{
    public List<TinTuyenDungSummaryDto> TinDangTuyen { get; set; } = new();
}

public class DanhMucNganhNgheDto
{
    public int MaNganhNghe { get; set; }
    public string TenNganhNghe { get; set; } = null!;
}
