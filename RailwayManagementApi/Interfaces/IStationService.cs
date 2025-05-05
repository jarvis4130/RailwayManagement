
namespace RailwayManagementApi.Interfaces
{
    public interface IStationService
    {
        Task<int> GetStationIdByNameAsync(string name);
    }
}