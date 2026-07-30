using JobHunter.API.Exceptions;
using JobHunter.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobHunter.API.Controllers;

[ApiController]
[Route("api/admin/reports")]
[Authorize(Roles = "Admin")]
public class AdminReportsController : ControllerBase
{
    private readonly IAdminReportService _service;

    public AdminReportsController(IAdminReportService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> BaoCao([FromQuery] int thang, [FromQuery] int nam)
    {
        try
        {
            return Ok(await _service.LayBaoCaoThangAsync(thang, nam));
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }
}
