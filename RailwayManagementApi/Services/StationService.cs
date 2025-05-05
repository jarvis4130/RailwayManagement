
using Microsoft.EntityFrameworkCore;
using RailwayManagementApi.Data;
using RailwayManagementApi.Interfaces;

namespace RailwayManagementApi.Services
{
    public class StationService : IStationService
    {
        private readonly RailwayContext _dbContext;

        public StationService(RailwayContext dbContext){
            _dbContext=dbContext;
        }
        public async Task<int> GetStationIdByNameAsync(string name)
        {
            var station = await _dbContext.Stations
                .FirstOrDefaultAsync(s => s.StationName.ToLower() == name.ToLower());

            return station?.StationID ?? 0;
        }


    }
}