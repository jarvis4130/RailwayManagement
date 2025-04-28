using RailwayManagementApi.DTOs;

namespace RailwayManagementApi.Services
{
    public interface ITrainService
    {
        Task<List<TrainDTO>> SearchTrainsAsync(TrainSearchDTO searchDto);
        Task<List<TrainAvailabilityDayDTO>> GetTrainAvailabilityForNext7Days(int trainId, string source, string destination);
    }
}
