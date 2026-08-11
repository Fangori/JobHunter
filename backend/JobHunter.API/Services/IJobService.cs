using JobHunter.API.DTOs;

namespace JobHunter.API.Services;

public interface IJobService
{
    Task<TinTuyenDungSummaryDto> DangTinAsync(int maTkNtd, DangTinRequest request);
    Task<List<TinTuyenDungSummaryDto>> XemDanhSachCongKhaiAsync(string? keyword, string? diaDiem, int? maNganhNghe = null);
    Task<DanhSachViecLamPhanTrangDto> TimKiemVaLocAsync(TimKiemVaLocRequest request); // UC10 loc nang cao (LAB4)
    Task<List<TinTuyenDungSummaryDto>> XemNoiBatAsync(int top);
    Task<TinTuyenDungDetailDto> XemChiTietAsync(int maTin);
    Task<List<TinTuyenDungSummaryDto>> XemDanhSachChoDuyetAsync();
    Task<PendingStatsResponse> ThongKeChoDuyetAsync();
    Task DuyetTinAsync(int maTin);
    Task TuChoiTinAsync(int maTin, string lyDo);
    Task<List<TinTuyenDungSummaryDto>> LayDanhSachCuaToiAsync(int maTkNtd); // UC28
    Task<SuaTinResponse> SuaTinAsync(int maTkNtd, int maTin, DangTinRequest request); // UC26, BR15
    Task<TinTuyenDungSummaryDto> GiaHanAsync(int maTkNtd, int maTin, DateOnly hanNopMoi); // UC27, BR24
    Task<TinTuyenDungSummaryDto> DongTinAsync(int maTkNtd, int maTin); // UC27
    Task GoTinAsync(int maTin, string lyDo); // UC35/BR17, MS45/MS54
    Task PhucHoiTinDaGoAsync(int maTin); // UC36, MS46
    Task<List<TinTuyenDungSummaryDto>> XemDanhSachDaGoAsync(); // UC35/36 danh sach quan tri
}
