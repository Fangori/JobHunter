using System.Security.Claims;
using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobHunter.API.Controllers;

[ApiController]
[Route("api/cvs")]
[Authorize(Roles = "UngVien")]
public class CvsController : ControllerBase
{
    private readonly ICvService _cvService;

    public CvsController(ICvService cvService)
    {
        _cvService = cvService;
    }

    private int CurrentMaTK => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("mine")]
    public async Task<IActionResult> Mine([FromQuery] string trangThai = "HoatDong")
        => Ok(await _cvService.LayDanhSachCuaToiAsync(CurrentMaTK, trangThai));

    [HttpPost("online")]
    public async Task<IActionResult> TaoTrucTuyen(TaoCvTrucTuyenRequest request)
    {
        var result = await _cvService.TaoCvTrucTuyenAsync(CurrentMaTK, request);
        return StatusCode(201, result);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] string tenCv, IFormFile file)
    {
        try
        {
            var result = await _cvService.UploadCvAsync(CurrentMaTK, tenCv, file);
            return StatusCode(201, result);
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detail(int id)
    {
        try
        {
            return Ok(await _cvService.LayChiTietAsync(CurrentMaTK, id));
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, TaoCvTrucTuyenRequest request)
    {
        try
        {
            var result = await _cvService.SuaCvTrucTuyenAsync(CurrentMaTK, id, request);
            return Ok(new { cv = result, message = "Cập nhật CV thành công." }); // MS37
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var message = await _cvService.XoaCvAsync(CurrentMaTK, id);
            return Ok(new { message });
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpPost("{id:int}/restore")]
    public async Task<IActionResult> Restore(int id)
    {
        try
        {
            await _cvService.PhucHoiCvAsync(CurrentMaTK, id);
            return Ok(new { message = "Phục hồi CV thành công." }); // MS40
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }
}
