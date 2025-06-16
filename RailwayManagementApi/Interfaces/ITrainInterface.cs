
using RailwayManagementApi.DTOs;
using RailwayManagementApi.DTOs;
using RailwayManagementApi.Models;

namespace RailwayManagementApi.Services
{
    public interface ITrainService
    {
        Task<List<TrainDTO>> SearchTrainsAsync(TrainSearchDTO searchDto);
        Task<List<TrainAvailabilityDayDTO>> GetTrainAvailabilityForNext7Days(int trainId, string source, string destination);
        Task<IEnumerable<TrainDTOAdmin>> GetAllTrainsAsync();
        Task<Train> AddTrainAsync(AddTrainReq request);
        Task<bool> DeleteTrainAsync(int trainId);
        Task<Train?> UpdateTrainAsync(int trainId, AddTrainReq request);
        Task<TrainSchedule> AddScheduleAsync(TrainScheduleDTO dto);

        List<int> GetTrainIdsByDate(string date);

        List<TrainScheduleDTO> GetScheduleByTrainAndDate(int trainId, DateTime date);

        Task UpdateTrainScheduleAsync(UpdateScheduleDto dto);

        Task<bool> DeleteScheduleAsync(int trainId, DateTime arrivalDate);
    }
}
