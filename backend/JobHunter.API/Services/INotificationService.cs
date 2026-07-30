using JobHunter.API.DTOs;

namespace JobHunter.API.Services;

public interface INotificationService
{
    Task TaoThongBaoAsync(int maTk, string noiDung, string loaiThongBao, string? lienKet = null);
    Task<List<NotificationDto>> LayCuaToiAsync(int maTk);
    Task DanhDauDaDocAsync(int maTk, int maThongBao);
}
