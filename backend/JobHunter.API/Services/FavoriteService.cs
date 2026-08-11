using JobHunter.API.Data;
using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Models;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.API.Services;

public class FavoriteService : IFavoriteService
{
    private readonly JobHunterDbContext _db;

    public FavoriteService(JobHunterDbContext db)
    {
        _db = db;
    }

    public async Task ThemAsync(int maTk, int maTin)
    {
        var tinTonTai = await _db.TinTuyenDungs.AnyAsync(x => x.MaTin == maTin);
        if (!tinTonTai)
            throw new BusinessRuleException(404, "Không tìm thấy tin tuyển dụng.");

        var daLuu = await _db.TinYeuThichs.AnyAsync(x => x.MaTK == maTk && x.MaTin == maTin);
        if (daLuu) return; // idempotent, khong loi neu bam lai

        _db.TinYeuThichs.Add(new TinYeuThich { MaTK = maTk, MaTin = maTin, NgayLuu = DateTime.UtcNow });
        await _db.SaveChangesAsync();
    }

    public async Task GoAsync(int maTk, int maTin)
    {
        var yeuThich = await _db.TinYeuThichs.FirstOrDefaultAsync(x => x.MaTK == maTk && x.MaTin == maTin);
        if (yeuThich is null) return; // idempotent

        _db.TinYeuThichs.Remove(yeuThich);
        await _db.SaveChangesAsync();
    }

    public async Task<List<TinTuyenDungSummaryDto>> LayCuaToiAsync(int maTk)
    {
        return await _db.TinYeuThichs.AsNoTracking()
            .Where(x => x.MaTK == maTk)
            .OrderByDescending(x => x.NgayLuu)
            .Join(_db.TinTuyenDungs.Include(t => t.NhaTuyenDung), yt => yt.MaTin, t => t.MaTin, (yt, t) => t)
            .Select(t => new TinTuyenDungSummaryDto
            {
                MaTin = t.MaTin,
                TieuDe = t.TieuDe,
                MaTkNtd = t.MaTK,
                TenCongTy = t.NhaTuyenDung.TenCongTy,
                Logo = t.NhaTuyenDung.Logo,
                DiaDiem = t.DiaDiem,
                MucLuong = t.MucLuong,
                HinhThucLamViec = t.HinhThucLamViec,
                NgayDang = t.NgayDang,
                HanNopHoSo = t.HanNopHoSo,
                TrangThai = t.TrangThai,
            })
            .ToListAsync();
    }
}
