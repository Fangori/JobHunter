using System.Security.Claims;
using JobHunter.API.Exceptions;
using JobHunter.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobHunter.API.Controllers;

[ApiController]
[Route("api/follows")]
[Authorize(Roles = "UngVien")]
public class FollowsController : ControllerBase
{
    private readonly IFollowService _followService;

    public FollowsController(IFollowService followService)
    {
        _followService = followService;
    }

    private int CurrentMaTK => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("{maTkNtd:int}")]
    public async Task<IActionResult> Them(int maTkNtd)
    {
        try
        {
            await _followService.ThemAsync(CurrentMaTK, maTkNtd);
            return StatusCode(201, new { message = "Đã theo dõi công ty. Bạn sẽ nhận thông báo khi có tin tuyển dụng mới." });
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    [HttpDelete("{maTkNtd:int}")]
    public async Task<IActionResult> Go(int maTkNtd)
    {
        await _followService.GoAsync(CurrentMaTK, maTkNtd);
        return NoContent();
    }

    [HttpGet("mine")]
    public async Task<IActionResult> Mine() => Ok(await _followService.LayCuaToiAsync(CurrentMaTK));
}
