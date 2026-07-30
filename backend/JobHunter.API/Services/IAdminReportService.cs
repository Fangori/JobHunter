using JobHunter.API.DTOs;

namespace JobHunter.API.Services;

public interface IAdminReportService
{
    Task<BaoCaoThangDto> LayBaoCaoThangAsync(int thang, int nam); // UC43/BR23/QD17
}
