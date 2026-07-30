using JobHunter.API.DTOs;

namespace JobHunter.API.Services;

public interface ICandidateMatchService
{
    Task<double> TinhDiemPhuHopAsync(int maCv, int maTin);
    Task<List<UngVienPhuHopDto>> XemDanhSachUngVienAsync(int maTin, int maTkNtd);
    Task<List<UngVienPhuHopDto>> LocUngVienAsync(int maTin, int maTkNtd, List<int>? maKyNang, int? minNamKinhNghiem, List<string>? trinhDoHocVan);
    Task<List<ViecLamGoiYDto>> GoiYViecLamAsync(int maTkUv); // UC14
}
