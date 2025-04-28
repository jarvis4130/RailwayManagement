using Microsoft.EntityFrameworkCore;
using RailwayManagementApi.Data;
using RailwayManagementApi.DTOs;

namespace RailwayManagementApi.Services
{
    public class TrainService : ITrainService
    {
        private readonly RailwayContext _dbContext;

        public TrainService(RailwayContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<TrainDTO>> SearchTrainsAsync(TrainSearchDTO searchDto)
        {
            var sourceStationId = await _dbContext.Stations
                .Where(s => s.StationName == searchDto.Source)
                .Select(s => s.StationID)
                .FirstOrDefaultAsync();

            var destinationStationId = await _dbContext.Stations
                .Where(s => s.StationName == searchDto.Destination)
                .Select(s => s.StationID)
                .FirstOrDefaultAsync();

            if (sourceStationId == 0 || destinationStationId == 0)
                throw new ArgumentException("Invalid source or destination station.");

            var trainIds = await _dbContext.TrainSchedules
                .Where(s => s.StationID == sourceStationId)
                .Join(
                    _dbContext.TrainSchedules.Where(s => s.StationID == destinationStationId),
                    src => src.TrainID,
                    dest => dest.TrainID,
                    (src, dest) => new
                    {
                        TrainID = src.TrainID,
                        SourceSequence = src.SequenceOrder,
                        DestinationSequence = dest.SequenceOrder
                    })
                .Where(x => x.SourceSequence < x.DestinationSequence)
                .Select(x => x.TrainID)
                .Distinct()
                .ToListAsync();

            if (!trainIds.Any())
                return new List<TrainDTO>(); // or throw exception if needed

            var trains = await (from train in _dbContext.Trains
                                join sourceSchedule in _dbContext.TrainSchedules on train.TrainID equals sourceSchedule.TrainID
                                join destinationSchedule in _dbContext.TrainSchedules on train.TrainID equals destinationSchedule.TrainID
                                where trainIds.Contains(train.TrainID)
                                    && sourceSchedule.StationID == sourceStationId
                                    && destinationSchedule.StationID == destinationStationId
                                select new
                                {
                                    Train = train,
                                    SourceSchedule = sourceSchedule,
                                    DestinationSchedule = destinationSchedule
                                }).ToListAsync();

            var distinctTrains = trains
                                .GroupBy(t => new { t.Train.TrainID, searchDto.JourneyDate })
                                .Select(g => g.First())
                                .ToList();
            var result = new List<TrainDTO>();

            foreach (var item in distinctTrains)
            {
                var journeyDate = searchDto.JourneyDate ?? DateTime.Today;
                var duration = (item.DestinationSchedule.ArrivalTime - item.SourceSchedule.DepartureTime).TotalMinutes;
                // var departureTime = item.SourceSchedule.DepartureTime.TimeOfDay;
                // var arrivalTime = item.DestinationSchedule.ArrivalTime.TimeOfDay;

                // double duration;
                // if (arrivalTime < departureTime)
                // {
                //     // Arrival is on next day
                //     duration = (arrivalTime + TimeSpan.FromDays(1) - departureTime).TotalMinutes;
                // }
                // else
                // {
                //     duration = (arrivalTime - departureTime).TotalMinutes;
                // }


                var classTypes = searchDto.ClassTypeId.HasValue ?
                    await _dbContext.ClassTypes.Where(c => c.ClassTypeID == searchDto.ClassTypeId.Value).ToListAsync()
                    : await _dbContext.ClassTypes.ToListAsync();

                var seatAvailabilityList = new List<SeatAvailabilityDTO>();

                foreach (var classType in classTypes)
                {
                    var availability = await _dbContext.SeatAvailabilities
                        .FirstOrDefaultAsync(sa =>
                            sa.TrainID == item.Train.TrainID &&
                            sa.Date.Date == journeyDate.Date &&
                            sa.ClassTypeID == classType.ClassTypeID);

                    int remainingSeats = availability?.RemainingSeats ?? 0;

                    int waitingCount = await _dbContext.WaitingLists
                        .Where(w =>
                            w.TrainID == item.Train.TrainID &&
                            w.ClassTypeID == classType.ClassTypeID &&
                            w.RequestDate.Date == journeyDate.Date)
                        .CountAsync();

                    seatAvailabilityList.Add(new SeatAvailabilityDTO
                    {
                        ClassType = classType.ClassName,
                        RemainingSeats = remainingSeats,
                        WaitingListCount = waitingCount
                    });
                }

                result.Add(new TrainDTO
                {
                    TrainId = item.Train.TrainID,
                    TrainName = item.Train.TrainName,
                    Source = searchDto.Source,
                    Destination = searchDto.Destination,
                    DepartureTime = item.SourceSchedule.DepartureTime.TimeOfDay,
                    ArrivalTime = item.DestinationSchedule.ArrivalTime.TimeOfDay,
                    DurationMinutes = (int)duration,
                    JourneyDate = journeyDate,
                    SeatAvailability = seatAvailabilityList
                });
            }

            return result;
        }

        public async Task<List<TrainAvailabilityDayDTO>> GetTrainAvailabilityForNext7Days(int trainId, string source, string destination)
        {
            var sourceStationId = await _dbContext.Stations
                .Where(s => s.StationName == source)
                .Select(s => s.StationID)
                .FirstOrDefaultAsync();

            var destinationStationId = await _dbContext.Stations
                .Where(s => s.StationName == destination)
                .Select(s => s.StationID)
                .FirstOrDefaultAsync();

            if (sourceStationId == 0 || destinationStationId == 0)
                throw new ArgumentException("Invalid source or destination.");

            var schedule = await _dbContext.TrainSchedules
                .Where(s => s.TrainID == trainId && (s.StationID == sourceStationId || s.StationID == destinationStationId))
                .ToListAsync();

            var sourceSchedule = schedule.FirstOrDefault(s => s.StationID == sourceStationId);
            var destinationSchedule = schedule.FirstOrDefault(s => s.StationID == destinationStationId);

            if (sourceSchedule == null || destinationSchedule == null || sourceSchedule.SequenceOrder >= destinationSchedule.SequenceOrder)
                throw new ArgumentException("Train does not run between the given stations in correct order.");

            var result = new List<TrainAvailabilityDayDTO>();
            var classTypes = await _dbContext.ClassTypes.ToListAsync();

            for (int i = 0; i < 7; i++)
            {
                var fixedDate = new DateTime(2025, 05, 01);
                var date = fixedDate.AddDays(i);
                var seatList = new List<SeatAvailabilityDTO>();

                foreach (var classType in classTypes)
                {
                    var remainingSeats = await _dbContext.SeatAvailabilities
                        .Where(s => s.TrainID == trainId && s.ClassTypeID == classType.ClassTypeID && s.Date == date)
                        .Select(s => s.RemainingSeats)
                        .FirstOrDefaultAsync();

                    var waitingCount = await _dbContext.WaitingLists
                        .Where(w => w.TrainID == trainId && w.ClassTypeID == classType.ClassTypeID && w.RequestDate.Date == date)
                        .CountAsync();

                    seatList.Add(new SeatAvailabilityDTO
                    {
                        ClassType = classType.ClassName,
                        RemainingSeats = remainingSeats,
                        WaitingListCount = waitingCount
                    });
                }

                result.Add(new TrainAvailabilityDayDTO
                {
                    JourneyDate = date,
                    DepartureTime = sourceSchedule.DepartureTime.TimeOfDay,
                    ArrivalTime = destinationSchedule.ArrivalTime.TimeOfDay,
                    SeatAvailability = seatList
                });
            }

            return result;
        }
    }
}
