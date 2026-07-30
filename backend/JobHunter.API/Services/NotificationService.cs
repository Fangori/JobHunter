using JobHunter.API.Data;
using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Models;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.API.Services;

public class NotificationService : INotificationService
{
    private readonly JobHunterDbContext _db;

    public NotificationService(JobHunterDbContext db)
    {
        _db = db;
    }

    public async Task TaoThongBaoAsync(int maTk, string noiDung, string loaiThongBao, string? lienKet = null)
    {
        _db.ThongBaos.Add(new ThongBao
        {
            MaTK = maTk,
            NoiDung = noiDung,
            LoaiThongBao = loaiThongBao,
            DaDoc = false,
            NgayTao = DateTime.UtcNow,
            LienKet = lienKet,
        });
        await _db.SaveChangesAsync();
    }

    public async Task<List<NotificationDto>> LayCuaToiAsync(int maTk)
    {
        return await _db.ThongBaos.AsNoTracking()
            .Where(x => x.MaTK == maTk)
            .OrderByDescending(x => x.NgayTao)
            .Select(x => new NotificationDto
            {
                MaThongBao = x.MaThongBao,
                NoiDung = x.NoiDung,
                LoaiThongBao = x.LoaiThongBao,
                DaDoc = x.DaDoc,
                NgayTao = x.NgayTao,
                LienKet = x.LienKet,
            })
            .ToListAsync();
    }

    public async Task DanhDauDaDocAsync(int maTk, int maThongBao)
    {
        var thongBao = await _db.ThongBaos.FirstOrDefaultAsync(x => x.MaThongBao == maThongBao);
        if (thongBao is null)
            throw new BusinessRuleException(404, "Không tìm thấy thông báo.");
        if (thongBao.MaTK != maTk)
            throw new BusinessRuleException(403, "Bạn không có quyền truy cập thông báo này.");

        thongBao.DaDoc = true;
        await _db.SaveChangesAsync();
    }
}
