using System.Security.Claims;
using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobHunter.API.Controllers;

[ApiController]
[Route("api/cvs")]
[Authorize(Roles = "UngVien")]
public class CvsController : ControllerBase
{
    private readonly ICvService _cvService;

    public CvsController(ICvService cvService)
    {
        _cvService = cvService;
    }

    private int CurrentMaTK => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("mine")]
    public async Task<IActionResult> Mine()
        => Ok(await _cvService.LayDanhSachCuaToiAsync(CurrentMaTK));

    [HttpPost("online")]
    public async Task<IActionResult> TaoTrucTuyen(TaoCvTrucTuyenRequest request)
    {
        var result = await _cvService.TaoCvTrucTuyenAsync(CurrentMaTK, request);
        return StatusCode(201, result);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] string tenCv, IFormFile file)
    {
        try
        {
            var result = await _cvService.UploadCvAsync(CurrentMaTK, tenCv, file);
            return StatusCode(201, result);
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }
}
