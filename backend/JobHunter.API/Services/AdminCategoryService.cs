using JobHunter.API.Data;
using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Models;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.API.Services;

public class AdminCategoryService : IAdminCategoryService
{
    private readonly JobHunterDbContext _db;

    public AdminCategoryService(JobHunterDbContext db)
    {
        _db = db;
    }

    public async Task<List<AdminSkillDto>> LayDanhSachKyNangAsync()
    {
        return await _db.DanhMucKyNangs.OrderBy(x => x.TenKyNang)
            .Select(x => new AdminSkillDto { MaKyNang = x.MaKyNang, TenKyNang = x.TenKyNang, NhomNganh = x.NhomNganh, TrangThai = x.TrangThai })
            .ToListAsync();
    }

    public async Task<AdminSkillDto> ThemKyNangAsync(DanhMucKyNangUpsertRequest request)
    {
        var trung = await _db.DanhMucKyNangs.AnyAsync(x => x.TenKyNang.ToLower() == request.TenKyNang.Trim().ToLower());
        if (trung)
            throw new BusinessRuleException(400, "Tên kỹ năng đã tồn tại trong danh mục."); // MS50, BR19

        var skill = new DanhMucKyNang { TenKyNang = request.TenKyNang.Trim(), NhomNganh = request.NhomNganh, TrangThai = "HoatDong" };
        _db.DanhMucKyNangs.Add(skill);
        await _db.SaveChangesAsync();
        return new AdminSkillDto { MaKyNang = skill.MaKyNang, TenKyNang = skill.TenKyNang, NhomNganh = skill.NhomNganh, TrangThai = skill.TrangThai };
    }

    public async Task<AdminSkillDto> SuaKyNangAsync(int maKyNang, DanhMucKyNangUpsertRequest request)
    {
        var skill = await _db.DanhMucKyNangs.FindAsync(maKyNang);
        if (skill is null)
            throw new BusinessRuleException(404, "Không tìm thấy kỹ năng.");

        var trung = await _db.DanhMucKyNangs.AnyAsync(x => x.MaKyNang != maKyNang && x.TenKyNang.ToLower() == request.TenKyNang.Trim().ToLower());
        if (trung)
            throw new BusinessRuleException(400, "Tên kỹ năng đã tồn tại trong danh mục."); // MS50, BR19

        skill.TenKyNang = request.TenKyNang.Trim();
        skill.NhomNganh = request.NhomNganh;
        await _db.SaveChangesAsync();
        return new AdminSkillDto { MaKyNang = skill.MaKyNang, TenKyNang = skill.TenKyNang, NhomNganh = skill.NhomNganh, TrangThai = skill.TrangThai };
    }

    public async Task<string> XoaKyNangAsync(int maKyNang)
    {
        var skill = await _db.DanhMucKyNangs.FindAsync(maKyNang);
        if (skill is null)
            throw new BusinessRuleException(404, "Không tìm thấy kỹ năng.");

        var dangDuoc = await _db.CvKyNangs.AnyAsync(x => x.MaKyNang == maKyNang)
                     || await _db.TinKyNangs.AnyAsync(x => x.MaKyNang == maKyNang);
        if (dangDuoc)
        {
            skill.TrangThai = "NgungSuDung";
            await _db.SaveChangesAsync();
            return "Kỹ năng đang được sử dụng, không thể xóa. Đã chuyển sang trạng thái ngừng sử dụng."; // MS52, BR20
        }

        _db.DanhMucKyNangs.Remove(skill);
        await _db.SaveChangesAsync();
        return "Xóa kỹ năng thành công."; // MS51
    }

    public async Task<List<AdminIndustryDto>> LayDanhSachNganhNgheAsync()
    {
        return await _db.DanhMucNganhNghes.OrderBy(x => x.TenNganhNghe)
            .Select(x => new AdminIndustryDto { MaNganhNghe = x.MaNganhNghe, TenNganhNghe = x.TenNganhNghe, TrangThai = x.TrangThai })
            .ToListAsync();
    }

    public async Task<AdminIndustryDto> ThemNganhNgheAsync(DanhMucNganhNgheUpsertRequest request)
    {
        var trung = await _db.DanhMucNganhNghes.AnyAsync(x => x.TenNganhNghe.ToLower() == request.TenNganhNghe.Trim().ToLower());
        if (trung)
            throw new BusinessRuleException(400, "Tên ngành nghề đã tồn tại trong danh mục."); // MS57, BR21

        var nganh = new DanhMucNganhNghe { TenNganhNghe = request.TenNganhNghe.Trim(), TrangThai = "HoatDong" };
        _db.DanhMucNganhNghes.Add(nganh);
        await _db.SaveChangesAsync();
        return new AdminIndustryDto { MaNganhNghe = nganh.MaNganhNghe, TenNganhNghe = nganh.TenNganhNghe, TrangThai = nganh.TrangThai };
    }

    public async Task<AdminIndustryDto> SuaNganhNgheAsync(int maNganhNghe, DanhMucNganhNgheUpsertRequest request)
    {
        var nganh = await _db.DanhMucNganhNghes.FindAsync(maNganhNghe);
        if (nganh is null)
            throw new BusinessRuleException(404, "Không tìm thấy ngành nghề.");

        var trung = await _db.DanhMucNganhNghes.AnyAsync(x => x.MaNganhNghe != maNganhNghe && x.TenNganhNghe.ToLower() == request.TenNganhNghe.Trim().ToLower());
        if (trung)
            throw new BusinessRuleException(400, "Tên ngành nghề đã tồn tại trong danh mục."); // MS57, BR21

        nganh.TenNganhNghe = request.TenNganhNghe.Trim();
        await _db.SaveChangesAsync();
        return new AdminIndustryDto { MaNganhNghe = nganh.MaNganhNghe, TenNganhNghe = nganh.TenNganhNghe, TrangThai = nganh.TrangThai };
    }

    public async Task<string> XoaNganhNgheAsync(int maNganhNghe)
    {
        var nganh = await _db.DanhMucNganhNghes.FindAsync(maNganhNghe);
        if (nganh is null)
            throw new BusinessRuleException(404, "Không tìm thấy ngành nghề.");

        var dangDuoc = await _db.NhaTuyenDungs.AnyAsync(x => x.MaNganhNghe == maNganhNghe);
        if (dangDuoc)
        {
            nganh.TrangThai = "NgungSuDung";
            await _db.SaveChangesAsync();
            return "Ngành nghề đang được sử dụng, không thể xóa. Đã chuyển sang trạng thái ngừng sử dụng."; // MS59, BR22
        }

        _db.DanhMucNganhNghes.Remove(nganh);
        await _db.SaveChangesAsync();
        return "Xóa ngành nghề thành công."; // MS58
    }
}
