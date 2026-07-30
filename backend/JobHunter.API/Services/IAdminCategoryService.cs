using JobHunter.API.DTOs;

namespace JobHunter.API.Services;

public interface IAdminCategoryService
{
    Task<List<AdminSkillDto>> LayDanhSachKyNangAsync();
    Task<AdminSkillDto> ThemKyNangAsync(DanhMucKyNangUpsertRequest request); // MS49/MS50, BR19
    Task<AdminSkillDto> SuaKyNangAsync(int maKyNang, DanhMucKyNangUpsertRequest request); // MS49/MS50, BR19
    Task<string> XoaKyNangAsync(int maKyNang); // MS51/MS52, BR20

    Task<List<AdminIndustryDto>> LayDanhSachNganhNgheAsync();
    Task<AdminIndustryDto> ThemNganhNgheAsync(DanhMucNganhNgheUpsertRequest request); // MS56/MS57, BR21
    Task<AdminIndustryDto> SuaNganhNgheAsync(int maNganhNghe, DanhMucNganhNgheUpsertRequest request); // MS56/MS57, BR21
    Task<string> XoaNganhNgheAsync(int maNganhNghe); // MS58/MS59, BR22
}
