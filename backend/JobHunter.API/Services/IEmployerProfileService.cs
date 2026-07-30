using JobHunter.API.DTOs;
using Microsoft.AspNetCore.Http;

namespace JobHunter.API.Services;

public interface IEmployerProfileService
{
    Task<EmployerProfileDto> LayHoSoAsync(int maTk);
    Task<EmployerProfileDto> CapNhatHoSoAsync(int maTk, CapNhatEmployerProfileRequest request, IFormFile? logo);
    Task<EmployerPublicProfileDto> LayHoSoCongKhaiAsync(int maTk);
}
