using System.Security.Claims;
using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobHunter.API.Controllers;

[ApiController]
[Route("api/jobs")]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;
    private readonly ICandidateMatchService _matchService;

    public JobsController(IJobService jobService, ICandidateMatchService matchService)
    {
        _jobService = jobService;
        _matchService = matchService;
    }

    private int CurrentMaTK => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> XemDanhSachCongKhai([FromQuery] string? keyword, [FromQuery] string? diaDiem, [FromQuery] int? maNganhNghe)
        => Ok(await _jobService.XemDanhSachCongKhaiAsync(keyword, diaDiem, maNganhNghe));

    [HttpGet("featured")]
    public async Task<IActionResult> XemNoiBat([FromQuery] int top = 6)
        => Ok(await _jobService.XemNoiBatAsync(top));

    [HttpGet("recommended")]
    [Authorize(Roles = "UngVien")]
    public async Task<IActionResult> GoiY() => Ok(await _matchService.GoiYViecLamAsync(CurrentMaTK));

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> XemDanhSachChoDuyet()
        => Ok(await _jobService.XemDanhSachChoDuyetAsync());

    [HttpGet("pending/stats")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ThongKeChoDuyet()
        => Ok(await _jobService.ThongKeChoDuyetAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> XemChiTiet(int id)
    {
        try
        {
            return Ok(await _jobService.XemChiTietAsync(id));
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "NhaTuyenDung")]
    public async Task<IActionResult> DangTin(DangTinRequest request)
    {
        try
        {
            var result = await _jobService.DangTinAsync(CurrentMaTK, request);
            return StatusCode(201, result);
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpGet("mine")]
    [Authorize(Roles = "NhaTuyenDung")]
    public async Task<IActionResult> Mine() => Ok(await _jobService.LayDanhSachCuaToiAsync(CurrentMaTK));

    [HttpPut("{id:int}")]
    [Authorize(Roles = "NhaTuyenDung")]
    public async Task<IActionResult> SuaTin(int id, DangTinRequest request)
    {
        try
        {
            var result = await _jobService.SuaTinAsync(CurrentMaTK, id, request);
            return Ok(result);
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpPost("{id:int}/extend")]
    [Authorize(Roles = "NhaTuyenDung")]
    public async Task<IActionResult> GiaHan(int id, GiaHanRequest request)
    {
        try
        {
            var result = await _jobService.GiaHanAsync(CurrentMaTK, id, request.HanNopMoi);
            return Ok(new { tin = result, message = "Cập nhật trạng thái tin thành công." }); // MS42
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpPost("{id:int}/close")]
    [Authorize(Roles = "NhaTuyenDung")]
    public async Task<IActionResult> Dong(int id)
    {
        try
        {
            var result = await _jobService.DongTinAsync(CurrentMaTK, id);
            return Ok(new { tin = result, message = "Cập nhật trạng thái tin thành công." }); // MS42
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpPost("{id:int}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DuyetTin(int id)
    {
        try
        {
            await _jobService.DuyetTinAsync(id);
            return Ok(new { message = "Duyệt tin thành công. Tin đã được công khai." }); // MS43
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpPost("{id:int}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> TuChoiTin(int id, TuChoiTinRequest request)
    {
        try
        {
            await _jobService.TuChoiTinAsync(id, request.LyDo);
            return Ok(new { message = "Tin đã bị từ chối." }); // MS44
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpGet("removed")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> XemDanhSachDaGo() => Ok(await _jobService.XemDanhSachDaGoAsync());

    [HttpPost("{id:int}/remove")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GoTin(int id, TuChoiTinRequest request)
    {
        try
        {
            await _jobService.GoTinAsync(id, request.LyDo);
            return Ok(new { message = "Đã gỡ tin vi phạm." }); // MS45
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    [HttpPost("{id:int}/restore-removed")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PhucHoiTinDaGo(int id)
    {
        try
        {
            await _jobService.PhucHoiTinDaGoAsync(id);
            return Ok(new { message = "Phục hồi tin thành công." }); // MS46
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    // UC29: xem danh sach ung vien binh thuong (khong loc)
    [HttpGet("{id:int}/applicants")]
    [Authorize(Roles = "NhaTuyenDung")]
    public async Task<IActionResult> XemDanhSachUngVien(int id)
    {
        try
        {
            return Ok(await _matchService.XemDanhSachUngVienAsync(id, CurrentMaTK));
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }

    // UC30/31: loc ung vien dung 3 tieu chi BM14 + diem phu hop QD14
    [HttpGet("{id:int}/applicants/filter")]
    [Authorize(Roles = "NhaTuyenDung")]
    public async Task<IActionResult> LocUngVien(int id, [FromQuery] List<int>? maKyNang, [FromQuery] int? minNamKinhNghiem, [FromQuery] List<string>? trinhDoHocVan)
    {
        try
        {
            // Tra ve mang thuan tuy (nhat quan voi cac endpoint khac); rong = MS07,
            // frontend tu hien thi thong bao khi mang rong thay vi doi shape response.
            return Ok(await _matchService.LocUngVienAsync(id, CurrentMaTK, maKyNang, minNamKinhNghiem, trinhDoHocVan));
        }
        catch (BusinessRuleException ex)
        {
            return StatusCode(ex.StatusCode, new ErrorResponse { Message = ex.Message });
        }
    }
}
