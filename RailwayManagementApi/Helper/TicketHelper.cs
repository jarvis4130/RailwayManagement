
using Microsoft.EntityFrameworkCore;
using RailwayManagementApi.Data;
using RailwayManagementApi.DTOs;
using RailwayManagementApi.Models;

namespace RailwayManagementApi.Helper
{
    public class TicketHelper
    {
        private readonly RailwayContext _dbContext;

        public TicketHelper(RailwayContext dbContext)
        {
            _dbContext = dbContext;
        }

        public string GenerateSeatNumber(int classTypeId, int trainId, int seatIndex)
        {
            string prefix;
            int seatsPerCoach;
            int coachCount = GetCoachCountForClass(trainId, classTypeId);

            switch (classTypeId)
            {
                case 1: prefix = "S"; seatsPerCoach = 72; break; // Sleeper
                case 2: prefix = "A"; seatsPerCoach = 64; break; // AC 3 Tier
                case 3: prefix = "B"; seatsPerCoach = 52; break; // AC 2 Tier
                case 4: prefix = "F"; seatsPerCoach = 24; break; // First Class
                default: prefix = "X"; seatsPerCoach = 50; break;
            }

            int coach = (seatIndex / seatsPerCoach) % coachCount + 1;
            int seatNumber = (seatIndex % seatsPerCoach) + 1;
            return $"{prefix}{coach}-{seatNumber:D2}";
        }

        public int GetTotalSeatsForClass(int trainId, int classTypeId)
        {
            int coachCount = GetCoachCountForClass(trainId, classTypeId);

            return classTypeId switch
            {
                1 => coachCount * 72,
                2 => coachCount * 64,
                3 => coachCount * 52,
                4 => coachCount * 24,
                _ => coachCount * 50
            };
        }

        public int GetCoachCountForClass(int trainId, int classTypeId)
        {
            var train = _dbContext.Trains.Find(trainId);
            if (train == null) return 0;

            // Adjust this based on your TrainType table or column
            switch (train.TrainType)
            {
                case "Express":
                    return classTypeId switch
                    {
                        1 => 8,
                        2 => 3,
                        3 => 3,
                        4 => 1,
                        _ => 1
                    };
                case "Superfast":
                    return classTypeId switch
                    {
                        1 => 6,
                        2 => 4,
                        3 => 2,
                        4 => 2,
                        _ => 1
                    };
                case "Local":
                    return classTypeId switch
                    {
                        1 => 4,
                        _ => 0
                    };
                default:
                    return 1;
            }
        }
        public async Task<decimal> CalculateFare(TicketBookingRequestDTO dto, TrainSchedule sourceSchedule, TrainSchedule destinationSchedule, int totalStops)
        {
            var baseFare = await _dbContext.Tickets
                .Where(t => t.TrainID == dto.TrainID
                         && t.SourceID == dto.SourceID
                         && t.DestinationID == dto.DestinationID)
                .Select(t => t.Fare)
                .FirstOrDefaultAsync();

            if (baseFare == 0)
                throw new Exception("Fare not available for the selected route.");

            decimal multiplier = dto.ClassTypeID switch
            {
                4 => 2.5m,
                3 => 1.75m,
                2 => 1.5m,
                _ => 1.0m
            };

            decimal fare = baseFare * multiplier;

            if (dto.HasInsurance)
                fare += dto.Passengers.Count * 20;

            return fare;
        }


    }
}