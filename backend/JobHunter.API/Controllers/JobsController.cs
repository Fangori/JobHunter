using System.Security.Claims;
using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobHunter.API.Controllers;

[ApiController]
[Route("api/jobs")]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;

    public JobsController(IJobService jobService)
    {
        _jobService = jobService;
    }

    private int CurrentMaTK => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> XemDanhSachCongKhai([FromQuery] string? keyword, [FromQuery] string? diaDiem)
        => Ok(await _jobService.XemDanhSachCongKhaiAsync(keyword, diaDiem));

    [HttpGet("featured")]
    public async Task<IActionResult> XemNoiBat([FromQuery] int top = 6)
        => Ok(await _jobService.XemNoiBatAsync(top));

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> XemDanhSachChoDuyet()
        => Ok(await _jobService.XemDanhSachChoDuyetAsync());

    [HttpGet("pending/stats")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ThongKeChoDuyet()
        => Ok(await _jobService.ThongKeChoDuyetAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> XemChiTiet(int id)
    {
        try
        {
            return Ok(await _jobService.XemChiTietAsync(id));
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "NhaTuyenDung")]
    public async Task<IActionResult> DangTin(DangTinRequest request)
    {
        try
        {
            var result = await _jobService.DangTinAsync(CurrentMaTK, request);
            return StatusCode(201, result);
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpPost("{id:int}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DuyetTin(int id)
    {
        try
        {
            await _jobService.DuyetTinAsync(id);
            return Ok(new { message = "Duyệt tin thành công. Tin đã được công khai." }); // MS43
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpPost("{id:int}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> TuChoiTin(int id, TuChoiTinRequest request)
    {
        try
        {
            await _jobService.TuChoiTinAsync(id, request.LyDo);
            return Ok(new { message = "Tin đã bị từ chối." }); // MS44
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }
}
