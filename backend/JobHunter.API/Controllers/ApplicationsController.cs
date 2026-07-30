using System.Security.Claims;
using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobHunter.API.Controllers;

[ApiController]
[Route("api/applications")]
[Authorize(Roles = "UngVien")]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _applicationService;

    public ApplicationsController(IApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    private int CurrentMaTK => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<IActionResult> UngTuyen(UngTuyenRequest request)
    {
        try
        {
            var result = await _applicationService.UngTuyenAsync(CurrentMaTK, request);
            return StatusCode(201, result);
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            await _applicationService.HuyDonAsync(CurrentMaTK, id);
            return Ok(new { message = "Hủy đơn ứng tuyển thành công." });
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpGet("mine")]
    public async Task<IActionResult> Mine() => Ok(await _applicationService.LayCuaToiAsync(CurrentMaTK));
}

[ApiController]
[Route("api/applications")]
[Authorize(Roles = "NhaTuyenDung")]
public class ApplicationsEmployerController : ControllerBase
{
    private readonly IApplicationService _applicationService;

    public ApplicationsEmployerController(IApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    private int CurrentMaTK => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("{id:int}/detail")]
    public async Task<IActionResult> Detail(int id)
    {
        try
        {
            return Ok(await _applicationService.LayChiTietAsync(CurrentMaTK, id));
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, CapNhatTrangThaiRequest request)
    {
        try
        {
            await _applicationService.CapNhatTrangThaiAsync(CurrentMaTK, id, request.TrangThaiMoi, request.GhiChuNoiBo);
            return Ok(new { message = "Cập nhật trạng thái thành công." }); // MS08
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }
}
