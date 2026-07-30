using JobHunter.API.Data;
using JobHunter.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.API.Controllers;

[ApiController]
[Route("api/skills")]
public class SkillsController : ControllerBase
{
    private readonly JobHunterDbContext _db;

    public SkillsController(JobHunterDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> DanhSach()
    {
        var skills = await _db.DanhMucKyNangs
            .Where(x => x.TrangThai == "HoatDong")
            .OrderBy(x => x.TenKyNang)
            .Select(x => new DanhMucKyNangDto { MaKyNang = x.MaKyNang, TenKyNang = x.TenKyNang })
            .ToListAsync();
        return Ok(skills);
    }
}
