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
