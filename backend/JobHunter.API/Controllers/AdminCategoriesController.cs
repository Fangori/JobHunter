using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobHunter.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminCategoriesController : ControllerBase
{
    private readonly IAdminCategoryService _service;

    public AdminCategoriesController(IAdminCategoryService service)
    {
        _service = service;
    }

    [HttpGet("skills")]
    public async Task<IActionResult> DanhSachKyNang() => Ok(await _service.LayDanhSachKyNangAsync());

    [HttpPost("skills")]
    public async Task<IActionResult> ThemKyNang(DanhMucKyNangUpsertRequest request)
    {
        try
        {
            var result = await _service.ThemKyNangAsync(request);
            return StatusCode(201, new { kyNang = result, message = "Lưu danh mục kỹ năng thành công." }); // MS49
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpPut("skills/{id:int}")]
    public async Task<IActionResult> SuaKyNang(int id, DanhMucKyNangUpsertRequest request)
    {
        try
        {
            var result = await _service.SuaKyNangAsync(id, request);
            return Ok(new { kyNang = result, message = "Lưu danh mục kỹ năng thành công." }); // MS49
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpDelete("skills/{id:int}")]
    public async Task<IActionResult> XoaKyNang(int id)
    {
        try
        {
            var message = await _service.XoaKyNangAsync(id);
            return Ok(new { message });
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpGet("industries")]
    public async Task<IActionResult> DanhSachNganhNghe() => Ok(await _service.LayDanhSachNganhNgheAsync());

    [HttpPost("industries")]
    public async Task<IActionResult> ThemNganhNghe(DanhMucNganhNgheUpsertRequest request)
    {
        try
        {
            var result = await _service.ThemNganhNgheAsync(request);
            return StatusCode(201, new { nganhNghe = result, message = "Lưu danh mục ngành nghề thành công." }); // MS56
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpPut("industries/{id:int}")]
    public async Task<IActionResult> SuaNganhNghe(int id, DanhMucNganhNgheUpsertRequest request)
    {
        try
        {
            var result = await _service.SuaNganhNgheAsync(id, request);
            return Ok(new { nganhNghe = result, message = "Lưu danh mục ngành nghề thành công." }); // MS56
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpDelete("industries/{id:int}")]
    public async Task<IActionResult> XoaNganhNghe(int id)
    {
        try
        {
            var message = await _service.XoaNganhNgheAsync(id);
            return Ok(new { message });
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }
}
