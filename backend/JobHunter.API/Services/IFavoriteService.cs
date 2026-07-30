using JobHunter.API.DTOs;

namespace JobHunter.API.Services;

public interface IFavoriteService
{
    Task ThemAsync(int maTk, int maTin); // MS27
    Task GoAsync(int maTk, int maTin);
    Task<List<TinTuyenDungSummaryDto>> LayCuaToiAsync(int maTk);
}
