using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobHunter.API.Controllers;

[ApiController]
[Route("api/admin/accounts")]
[Authorize(Roles = "Admin")]
public class AdminAccountsController : ControllerBase
{
    private readonly IAdminAccountService _service;

    public AdminAccountsController(IAdminAccountService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> DanhSach([FromQuery] string? vaiTro) => Ok(await _service.LayDanhSachAsync(vaiTro));

    [HttpPost("{id:int}/lock")]
    public async Task<IActionResult> Lock(int id, LockAccountRequest request)
    {
        try
        {
            var message = await _service.KhoaTaiKhoanAsync(id, request.LyDo);
            return Ok(new { message });
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpPost("{id:int}/unlock")]
    public async Task<IActionResult> Unlock(int id)
    {
        try
        {
            await _service.MoKhoaTaiKhoanAsync(id);
            return Ok(new { message = "Cập nhật trạng thái tài khoản thành công." });
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }
}
