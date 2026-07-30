using System.Security.Claims;
using JobHunter.API.Exceptions;
using JobHunter.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobHunter.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    private int CurrentMaTK => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("mine")]
    public async Task<IActionResult> Mine() => Ok(await _notificationService.LayCuaToiAsync(CurrentMaTK));

    [HttpPost("{id:int}/mark-read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        try
        {
            await _notificationService.DanhDauDaDocAsync(CurrentMaTK, id);
            return Ok();
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }
}
