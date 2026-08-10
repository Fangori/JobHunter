using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobHunter.API.Controllers;

[ApiController]
[Route("api/admin/packages")]
[Authorize(Roles = "Admin")]
public class AdminPackagesController : ControllerBase
{
    private readonly IGoiDichVuService _service;

    public AdminPackagesController(IGoiDichVuService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> DanhSach() => Ok(await _service.LayDanhSachAdminAsync());

    [HttpPost]
    public async Task<IActionResult> Them(GoiDichVuUpsertRequest request)
    {
        try
        {
            var result = await _service.ThemGoiAsync(request);
            return StatusCode(201, new { goi = result, message = "Lưu gói dịch vụ thành công." }); // MS62
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Sua(int id, GoiDichVuUpsertRequest request)
    {
        try
        {
            var result = await _service.SuaGoiAsync(id, request);
            return Ok(new { goi = result, message = "Lưu gói dịch vụ thành công." }); // MS62
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Xoa(int id)
    {
        try
        {
            var message = await _service.XoaGoiAsync(id);
            return Ok(new { message });
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }
}
