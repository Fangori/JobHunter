using JobHunter.API.DTOs;

namespace JobHunter.API.Services;

public interface IApplicationService
{
    Task<DonUngTuyenResponse> UngTuyenAsync(int maTkUv, UngTuyenRequest request);
}
