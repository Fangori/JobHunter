using System.Security.Claims;
using JobHunter.API.Exceptions;
using JobHunter.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobHunter.API.Controllers;

[ApiController]
[Route("api/favorites")]
[Authorize(Roles = "UngVien")]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    private int CurrentMaTK => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("{maTin:int}")]
    public async Task<IActionResult> Them(int maTin)
    {
        try
        {
            await _favoriteService.ThemAsync(CurrentMaTK, maTin);
            return StatusCode(201, new { message = "Đã lưu tin vào danh sách yêu thích." });
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    [HttpDelete("{maTin:int}")]
    public async Task<IActionResult> Go(int maTin)
    {
        await _favoriteService.GoAsync(CurrentMaTK, maTin);
        return NoContent();
    }

    [HttpGet("mine")]
    public async Task<IActionResult> Mine() => Ok(await _favoriteService.LayCuaToiAsync(CurrentMaTK));
}
