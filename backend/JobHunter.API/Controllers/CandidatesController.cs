using System.Security.Claims;
using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobHunter.API.Controllers;

[ApiController]
[Route("api/candidates")]
[Authorize(Roles = "UngVien")]
public class CandidatesController : ControllerBase
{
    private readonly ICandidateProfileService _profileService;

    public CandidatesController(ICandidateProfileService profileService)
    {
        _profileService = profileService;
    }

    private int CurrentMaTK => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("me")]
    public async Task<IActionResult> Me() => Ok(await _profileService.LayHoSoAsync(CurrentMaTK));

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe([FromForm] CapNhatCandidateProfileRequest request, IFormFile? anhDaiDien)
    {
        try
        {
            var result = await _profileService.CapNhatHoSoAsync(CurrentMaTK, request, anhDaiDien);
            return Ok(result);
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }
}
