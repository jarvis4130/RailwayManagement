using RailwayManagementApi.DTOs.StationDTO;
using RailwayManagementApi.Models;

namespace RailwayManagementApi.Interfaces
{
    public interface IStationService
    {
        Task<Station> GetStationIdByNameAsync(string name);
        Task<IEnumerable<Station>> GetAllStationsAsync();
        Task<Station> CreateStationAsync(AddStationDTO newStation);
        Task<bool> UpdateStationAsync(int id,UpdateStationDTO station);
        Task<bool> DeleteStationAsync(int id);
    }
}
