using Microsoft.EntityFrameworkCore;
using RailwayManagementApi.Data;
using RailwayManagementApi.DTOs;
using RailwayManagementApi.Models;
using RailwayManagementApi.Helper;
using RailwayManagementApi.Interfaces;

namespace RailwayManagementApi.Services
{
    public class TrainService : ITrainService
    {
        private readonly RailwayContext _dbContext;
        private readonly TicketHelper _ticketHelper;
        private readonly INotificationService _notificationService;

        public TrainService(RailwayContext dbContext, TicketHelper ticketHelper, INotificationService notificationService)
        {
            _dbContext = dbContext;
            _ticketHelper = ticketHelper;
            _notificationService = notificationService;
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

                // var seatAvailabilityList = new List<SeatAvailabilityDTO>();

                // foreach (var classType in classTypes)
                // {
                //     var availability = await _dbContext.SeatAvailabilities
                //         .FirstOrDefaultAsync(sa =>
                //             sa.TrainID == item.Train.TrainID &&
                //             sa.Date.Date == journeyDate.Date &&
                //             sa.ClassTypeID == classType.ClassTypeID);

                //     int remainingSeats = availability?.RemainingSeats ?? 0;

                //     int waitingCount = await _dbContext.WaitingLists
                //         .Where(w =>
                //             w.TrainID == item.Train.TrainID &&
                //             w.ClassTypeID == classType.ClassTypeID &&
                //             w.RequestDate.Date == journeyDate.Date)
                //         .CountAsync();

                //     seatAvailabilityList.Add(new SeatAvailabilityDTO
                //     {
                //         ClassType = classType.ClassName,
                //         RemainingSeats = remainingSeats,
                //         WaitingListCount = waitingCount
                //     });
                // }

                // result.Add(new TrainDTO
                // {
                //     TrainId = item.Train.TrainID,
                //     TrainName = item.Train.TrainName,
                //     Source = searchDto.Source,
                //     Destination = searchDto.Destination,
                //     DepartureTime = item.SourceSchedule.DepartureTime.TimeOfDay,
                //     ArrivalTime = item.DestinationSchedule.ArrivalTime.TimeOfDay,
                //     DurationMinutes = (int)duration,
                //     JourneyDate = journeyDate,
                //     SeatAvailability = seatAvailabilityList
                // });
                var seatAvailabilityList = new List<SeatAvailabilityDTO>();
                bool hasAnyAvailability = false;

                foreach (var classType in classTypes)
                {
                    var availability = await _dbContext.SeatAvailabilities
                        .FirstOrDefaultAsync(sa =>
                            sa.TrainID == item.Train.TrainID &&
                            sa.Date.Date == journeyDate.Date &&
                            sa.ClassTypeID == classType.ClassTypeID);

                    if (availability == null)
                        continue;

                    hasAnyAvailability = true;

                    int waitingCount = await _dbContext.WaitingLists
                        .Where(w =>
                            w.TrainID == item.Train.TrainID &&
                            w.ClassTypeID == classType.ClassTypeID &&
                            w.RequestDate.Date == journeyDate.Date)
                        .CountAsync();

                    seatAvailabilityList.Add(new SeatAvailabilityDTO
                    {
                        ClassType = classType.ClassName,
                        RemainingSeats = availability.RemainingSeats,
                        WaitingListCount = waitingCount
                    });
                }

                if (!hasAnyAvailability)
                    continue;

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

        public async Task<IEnumerable<TrainDTOAdmin>> GetAllTrainsAsync()
        {
            return await _dbContext.Trains
         .Select(t => new TrainDTOAdmin
         {
             TrainID = t.TrainID,
             TrainName = t.TrainName,
             TrainType = t.TrainType,
             TotalSeats = t.TotalSeats,
             RunningDays = t.RunningDays
         })
         .ToListAsync();
        }

        public async Task<Train> AddTrainAsync(AddTrainReq request)
        {
            var newTrain = new Train
            {
                TrainName = request.TrainName,
                TrainType = request.TrainType,
                TotalSeats = request.TotalSeats,
                RunningDays = "" // Optional: can be set later
            };

            _dbContext.Trains.Add(newTrain);
            await _dbContext.SaveChangesAsync();
            return newTrain;
        }

        public async Task<bool> DeleteTrainAsync(int trainId)
        {
            var train = await _dbContext.Trains.FindAsync(trainId);
            if (train == null)
                return false;

            _dbContext.Trains.Remove(train);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<Train?> UpdateTrainAsync(int trainId, AddTrainReq request)
        {
            var train = await _dbContext.Trains.FindAsync(trainId);
            if (train == null)
                return null;

            if (!string.IsNullOrWhiteSpace(request.TrainName))
                train.TrainName = request.TrainName;

            if (!string.IsNullOrWhiteSpace(request.TrainType))
            {
                train.TrainType = request.TrainType;
                train.TotalSeats = request.TotalSeats;
            }
            await _dbContext.SaveChangesAsync();
            return train;
        }

        // public async Task<TrainSchedule> AddScheduleAsync(TrainScheduleDTO dto)
        // {
        //     var schedule = new TrainSchedule
        //     {
        //         TrainID = dto.TrainID,
        //         StationID = dto.StationID,
        //         ArrivalTime = dto.ArrivalTime,
        //         DepartureTime = dto.DepartureTime,
        //         SequenceOrder = dto.SequenceOrder,
        //         Fair = dto.Fair,
        //         DistanceFromSource = dto.DistanceFromSource
        //     };

        //     _dbContext.TrainSchedules.Add(schedule);
        //     await _dbContext.SaveChangesAsync();
        //     return schedule;
        // }

        public async Task<TrainSchedule> AddScheduleAsync(TrainScheduleDTO dto)
        {
            var schedule = new TrainSchedule
            {
                TrainID = dto.TrainID,
                StationID = dto.StationID,
                ArrivalTime = dto.ArrivalTime,
                DepartureTime = dto.DepartureTime,
                SequenceOrder = dto.SequenceOrder,
                Fair = dto.Fair,
                DistanceFromSource = dto.DistanceFromSource
            };

            _dbContext.TrainSchedules.Add(schedule);
            await _dbContext.SaveChangesAsync();

            // 🔥 Add seat availability for that train and date (only if it's the first station)
            if (dto.SequenceOrder == 1)
            {
                var travelDate = dto.ArrivalTime.Date;

                // Check if seat availability already exists for this train and date
                bool availabilityExists = _dbContext.SeatAvailabilities
                    .Any(sa => sa.TrainID == dto.TrainID && sa.Date == travelDate);

                if (!availabilityExists)
                {
                    for (int classTypeId = 1; classTypeId <= 4; classTypeId++)
                    {
                        int totalSeats = _ticketHelper.GetTotalSeatsForClass(dto.TrainID, classTypeId);
                        if (totalSeats == 0) continue;

                        var seatAvailability = new SeatAvailability
                        {
                            TrainID = dto.TrainID,
                            Date = travelDate,
                            ClassTypeID = classTypeId,
                            RemainingSeats = totalSeats
                        };

                        _dbContext.SeatAvailabilities.Add(seatAvailability);
                    }

                    await _dbContext.SaveChangesAsync();
                }
            }

            return schedule;
        }


        public List<int> GetTrainIdsByDate(string date)
        {
            if (!DateTime.TryParse(date, out var parsedDate))
                throw new ArgumentException("Invalid date format");

            return _dbContext.TrainSchedules
                .Where(s => s.ArrivalTime.Date == parsedDate.Date || s.DepartureTime.Date == parsedDate.Date)
                .Select(s => s.TrainID)
                .Distinct()
                .ToList();
        }

        public List<TrainScheduleDTO> GetScheduleByTrainAndDate(int trainId, DateTime date)
        {
            return _dbContext.TrainSchedules
                .Where(s => s.TrainID == trainId && s.ArrivalTime.Date == date.Date)
                .OrderBy(s => s.SequenceOrder)
                .Select(s => new TrainScheduleDTO
                {
                    StationID = s.StationID,
                    ArrivalTime = s.ArrivalTime,
                    DepartureTime = s.DepartureTime,
                    SequenceOrder = s.SequenceOrder,
                    Fair = s.Fair,
                    DistanceFromSource = s.DistanceFromSource
                })
                .ToList();
        }

        // public async Task UpdateTrainScheduleAsync(UpdateScheduleDto dto)
        // {
        //     foreach (var stop in dto.Schedules)
        //     {
        //         var existing = await _dbContext.TrainSchedules.FirstOrDefaultAsync(s =>
        //             s.TrainID == dto.TrainID &&
        //             s.StationID == stop.StationID &&
        //             s.ArrivalTime.Date == dto.Date.Date);

        //         if (existing != null)
        //         {
        //             existing.ArrivalTime = stop.ArrivalTime;
        //             existing.DepartureTime = stop.DepartureTime;
        //             existing.SequenceOrder = stop.SequenceOrder;
        //             existing.Fair = stop.Fare;
        //             existing.DistanceFromSource = stop.DistanceFromSource;
        //         }
        //     }

        //     await _dbContext.SaveChangesAsync();
        // }
        public async Task UpdateTrainScheduleAsync(UpdateScheduleDto dto)
        {
            bool scheduleChanged = false;

            foreach (var stop in dto.Schedules)
            {
                var existing = await _dbContext.TrainSchedules.FirstOrDefaultAsync(s =>
                    s.TrainID == dto.TrainID &&
                    s.StationID == stop.StationID &&
                    s.ArrivalTime.Date == dto.Date.Date);

                if (existing != null)
                {
                    // Check if any changes were made
                    if (existing.ArrivalTime != stop.ArrivalTime ||
                        existing.DepartureTime != stop.DepartureTime ||
                        existing.SequenceOrder != stop.SequenceOrder ||
                        existing.Fair != stop.Fare ||
                        existing.DistanceFromSource != stop.DistanceFromSource)
                    {
                        existing.ArrivalTime = stop.ArrivalTime;
                        existing.DepartureTime = stop.DepartureTime;
                        existing.SequenceOrder = stop.SequenceOrder;
                        existing.Fair = stop.Fare;
                        existing.DistanceFromSource = stop.DistanceFromSource;
                        scheduleChanged = true;
                    }
                }
            }

            if (scheduleChanged)
            {
                await _dbContext.SaveChangesAsync();

                // Notify users whose tickets are for the affected train and date
                var usersToNotify = await _dbContext.Tickets
                    .Where(t => t.TrainID == dto.TrainID && t.JourneyDate.Date == dto.Date.Date)
                    .Select(t => new
                    {
                        t.User.Email,
                        t.User.UserName,
                        t.Train.TrainName,
                        t.JourneyDate
                    })
                    .Distinct()
                    .ToListAsync();

                foreach (var user in usersToNotify)
                {
                    string subject = $"Train Schedule Updated - {user.TrainName}";
                    string body = $@"
                <p>Dear {user.UserName},</p>
                <p>Please be informed that the schedule for your booked train <strong>{user.TrainName}</strong> on <strong>{user.JourneyDate:dd MMM yyyy}</strong> has been updated.</p>
                <p>We recommend you to review the updated timings before your journey.</p>
                <p>Regards,<br/>Railway Management Team</p>";

                    await _notificationService.SendEmailAsync(user.Email, subject, body);
                }
            }
        }

        public async Task<bool> DeleteScheduleAsync(int trainId, DateTime arrivalDate)
        {
            var targetDate = arrivalDate.Date;

            // Get all schedules for that train on that specific date
            var schedules = await _dbContext.TrainSchedules
                .Where(s => s.TrainID == trainId && s.ArrivalTime.Date == targetDate)
                .ToListAsync();

            if (!schedules.Any())
                return false;

            // Delete schedules
            _dbContext.TrainSchedules.RemoveRange(schedules);

            // Delete seat availability for that train on that specific date
            var seatAvailabilities = await _dbContext.SeatAvailabilities
                .Where(sa => sa.TrainID == trainId && sa.Date == targetDate)
                .ToListAsync();

            if (seatAvailabilities.Any())
            {
                _dbContext.SeatAvailabilities.RemoveRange(seatAvailabilities);
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

    }
}
