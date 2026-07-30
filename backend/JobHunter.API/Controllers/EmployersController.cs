using System.Security.Claims;
using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobHunter.API.Controllers;

[ApiController]
[Route("api/employers")]
public class EmployersController : ControllerBase
{
    private readonly IEmployerProfileService _profileService;

    public EmployersController(IEmployerProfileService profileService)
    {
        _profileService = profileService;
    }

    private int CurrentMaTK => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("me")]
    [Authorize(Roles = "NhaTuyenDung")]
    public async Task<IActionResult> Me() => Ok(await _profileService.LayHoSoAsync(CurrentMaTK));

    [HttpPut("me")]
    [Authorize(Roles = "NhaTuyenDung")]
    public async Task<IActionResult> UpdateMe([FromForm] CapNhatEmployerProfileRequest request, IFormFile? logo)
    {
        try
        {
            var result = await _profileService.CapNhatHoSoAsync(CurrentMaTK, request, logo);
            return Ok(result);
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    // UC12 - cong khai, khong can dang nhap
    [HttpGet("{id:int}")]
    public async Task<IActionResult> PublicProfile(int id)
    {
        try
        {
            return Ok(await _profileService.LayHoSoCongKhaiAsync(id));
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }
}
