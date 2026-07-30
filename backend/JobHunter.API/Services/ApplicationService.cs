using JobHunter.API.Data;
using JobHunter.API.DTOs;
using JobHunter.API.Exceptions;
using JobHunter.API.Models;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.API.Services;

public class ApplicationService : IApplicationService
{
    private readonly JobHunterDbContext _db;

    public ApplicationService(JobHunterDbContext db)
    {
        _db = db;
    }

    public async Task<DonUngTuyenResponse> UngTuyenAsync(int maTkUv, UngTuyenRequest request)
    {
        var tin = await _db.TinTuyenDungs.FindAsync(request.MaTin);
        if (tin is null)
            throw new BusinessRuleException(404, "Không tìm thấy tin tuyển dụng.");

        if (tin.HanNopHoSo < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new BusinessRuleException(400, "Tin tuyển dụng đã hết hạn nộp hồ sơ."); // MS32

        var cv = await _db.Cvs.FindAsync(request.MaCv);
        if (cv is null || cv.MaTK != maTkUv)
            throw new BusinessRuleException(404, "Không tìm thấy CV.");

        // QD10/TS8: "dang hoat dong" = tat ca trang thai TRU TuChoi va DaHuy
        var coDonActive = await _db.DonUngTuyens
            .Include(x => x.Cv)
            .AnyAsync(x => x.MaTin == request.MaTin
                        && x.Cv.MaTK == maTkUv
                        && x.TrangThai != "TuChoi"
                        && x.TrangThai != "DaHuy");
        if (coDonActive)
            throw new BusinessRuleException(409, "Bạn đã ứng tuyển vào tin này rồi."); // MS31

        var don = new DonUngTuyen
        {
            MaTin = request.MaTin,
            MaCV = request.MaCv,
            ThuGioiThieu = request.ThuGioiThieu,
            NgayNop = DateTime.UtcNow,
            TrangThai = "DaNop",
            DaXem = false,
        };
        _db.DonUngTuyens.Add(don);
        tin.SoDonUngTuyen++;

        await _db.SaveChangesAsync();
        return new DonUngTuyenResponse { MaDon = don.MaDon, TrangThai = don.TrangThai };
    }
}
