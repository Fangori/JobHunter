using System.Security.Claims;
using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobHunter.API.Controllers;

[ApiController]
[Route("api/packages")]
[Authorize(Roles = "NhaTuyenDung")]
public class PackagesController : ControllerBase
{
    private readonly IGoiDichVuService _service;

    public PackagesController(IGoiDichVuService service)
    {
        _service = service;
    }

    private int CurrentMaTK => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> DanhSach() => Ok(await _service.LayDanhSachChoNtdAsync(CurrentMaTK));

    [HttpPost("{id:int}/mua")]
    public async Task<IActionResult> Mua(int id, MuaGoiRequest request)
    {
        try
        {
            var result = await _service.MuaGoiAsync(CurrentMaTK, id, request);
            return Ok(result);
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }
}
