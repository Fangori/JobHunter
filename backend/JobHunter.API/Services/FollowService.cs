using JobHunter.API.Data;
using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Models;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.API.Services;

public class FollowService : IFollowService
{
    private readonly JobHunterDbContext _db;

    public FollowService(JobHunterDbContext db)
    {
        _db = db;
    }

    public async Task ThemAsync(int maTkUv, int maTkNtd)
    {
        var ntdTonTai = await _db.NhaTuyenDungs.AnyAsync(x => x.MaTK == maTkNtd);
        if (!ntdTonTai)
            throw new BusinessRuleException(404, "Không tìm thấy công ty.");

        var daTheoDoi = await _db.TheoDoiCongTys.AnyAsync(x => x.MaTK_UngVien == maTkUv && x.MaTK_NTD == maTkNtd);
        if (daTheoDoi) return;

        _db.TheoDoiCongTys.Add(new TheoDoiCongTy { MaTK_UngVien = maTkUv, MaTK_NTD = maTkNtd, NgayTheoDoi = DateTime.UtcNow });
        await _db.SaveChangesAsync();
    }

    public async Task GoAsync(int maTkUv, int maTkNtd)
    {
        var theoDoi = await _db.TheoDoiCongTys.FirstOrDefaultAsync(x => x.MaTK_UngVien == maTkUv && x.MaTK_NTD == maTkNtd);
        if (theoDoi is null) return;

        _db.TheoDoiCongTys.Remove(theoDoi);
        await _db.SaveChangesAsync();
    }

    public async Task<List<FollowedCompanyDto>> LayCuaToiAsync(int maTkUv)
    {
        return await _db.TheoDoiCongTys.AsNoTracking()
            .Where(x => x.MaTK_UngVien == maTkUv)
            .OrderByDescending(x => x.NgayTheoDoi)
            .Join(_db.NhaTuyenDungs, td => td.MaTK_NTD, ntd => ntd.MaTK, (td, ntd) => ntd)
            .Select(ntd => new FollowedCompanyDto { MaTk = ntd.MaTK, TenCongTy = ntd.TenCongTy, Logo = ntd.Logo })
            .ToListAsync();
    }
}
