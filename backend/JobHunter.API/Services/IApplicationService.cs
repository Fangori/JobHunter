using JobHunter.API.DTOs;

namespace JobHunter.API.Services;

public interface IApplicationService
{
    Task<DonUngTuyenResponse> UngTuyenAsync(int maTkUv, UngTuyenRequest request);
    Task HuyDonAsync(int maTkUv, int maDon); // MS33/MS34, BR10
    Task<List<DonUngTuyenMineDto>> LayCuaToiAsync(int maTkUv);
}
