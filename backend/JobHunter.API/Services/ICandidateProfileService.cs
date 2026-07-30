using JobHunter.API.DTOs;
using Microsoft.AspNetCore.Http;

namespace JobHunter.API.Services;

public interface ICandidateProfileService
{
    Task<CandidateProfileDto> LayHoSoAsync(int maTk);
    Task<CandidateProfileDto> CapNhatHoSoAsync(int maTk, CapNhatCandidateProfileRequest request, IFormFile? anhDaiDien);
}
