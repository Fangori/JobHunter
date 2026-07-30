namespace JobHunter.API.Services;

public interface IThamSoService
{
    Task<int> LayGiaTriIntAsync(string maThamSo);
}
