using JobHunter.API.Data;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.API.Services;

public class ThamSoService : IThamSoService
{
    private readonly JobHunterDbContext _db;

    public ThamSoService(JobHunterDbContext db)
    {
        _db = db;
    }

    public async Task<int> LayGiaTriIntAsync(string maThamSo)
    {
        var thamSo = await _db.ThamSos.AsNoTracking().FirstOrDefaultAsync(x => x.MaThamSo == maThamSo);
        if (thamSo is null)
            throw new InvalidOperationException($"Thieu tham so {maThamSo} trong bang THAM_SO");
        return int.Parse(thamSo.GiaTri);
    }
}
