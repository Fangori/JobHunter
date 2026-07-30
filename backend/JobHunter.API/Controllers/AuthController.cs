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

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request)
    {
        try
        {
            await _authService.VerifyEmailAsync(request.Token);
            return Ok(new MessageResponse { Message = "Xác thực email thành công. Bạn có thể đăng nhập ngay bây giờ." }); // MS18
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification(ResendVerificationRequest request)
    {
        try
        {
            await _authService.ResendVerificationAsync(request.Email);
            return Ok(new MessageResponse { Message = "Đã gửi lại liên kết xác thực." });
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        try
        {
            await _authService.ForgotPasswordAsync(request.Email);
            return Ok(new MessageResponse { Message = "Đã gửi email chứa liên kết đặt lại mật khẩu." }); // MS20
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        try
        {
            await _authService.ResetPasswordAsync(request.Token, request.MatKhauMoi, request.XacNhanMatKhauMoi);
            return Ok(new MessageResponse { Message = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập lại." }); // MS21
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }
}
