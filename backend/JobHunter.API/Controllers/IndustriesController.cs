using JobHunter.API.Data;
using JobHunter.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.API.Controllers;

// GET cong khai de BM08 (Ho so cong ty) co danh sach chon "Linh vuc
// hoat dong". CRUD (them/sua/xoa) thuoc Admin, se lam o Phase 12.
[ApiController]
[Route("api/industries")]
public class IndustriesController : ControllerBase
{
    private readonly JobHunterDbContext _db;

    public IndustriesController(JobHunterDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> DanhSach()
    {
        var industries = await _db.DanhMucNganhNghes
            .Where(x => x.TrangThai == "HoatDong")
            .OrderBy(x => x.TenNganhNghe)
            .Select(x => new DanhMucNganhNgheDto { MaNganhNghe = x.MaNganhNghe, TenNganhNghe = x.TenNganhNghe })
            .ToListAsync();
        return Ok(industries);
    }
}
