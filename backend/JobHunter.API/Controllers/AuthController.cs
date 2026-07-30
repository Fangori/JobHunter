using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobHunter.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register/candidate")]
    public async Task<IActionResult> RegisterCandidate(DangKyUngVienRequest request)
    {
        try
        {
            var result = await _authService.DangKyUngVienAsync(request);
            return StatusCode(201, result);
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpPost("register/employer")]
    public async Task<IActionResult> RegisterEmployer(DangKyNhaTuyenDungRequest request)
    {
        try
        {
            var result = await _authService.DangKyNhaTuyenDungAsync(request);
            return StatusCode(201, result);
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        try
        {
            var result = await _authService.DangNhapAsync(request);
            return Ok(result);
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }
}
