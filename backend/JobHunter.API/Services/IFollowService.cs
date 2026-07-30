using JobHunter.API.DTOs;

namespace JobHunter.API.Services;

public interface IFollowService
{
    Task ThemAsync(int maTkUv, int maTkNtd); // MS29
    Task GoAsync(int maTkUv, int maTkNtd);
    Task<List<FollowedCompanyDto>> LayCuaToiAsync(int maTkUv);
}
