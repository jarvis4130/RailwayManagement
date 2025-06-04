using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RailwayManagementApi.Data;
using RailwayManagementApi.DTOs.StationDTO;
using RailwayManagementApi.Interfaces;
using RailwayManagementApi.Models;

namespace RailwayManagementApi.Services
{
    public class StationService : IStationService
    {
        private readonly RailwayContext _context;

        public StationService(RailwayContext context)
        {
            _context = context;
        }

        public async Task<Station> GetStationIdByNameAsync(string name)
        {
            var station = await _context.Stations
                .FirstOrDefaultAsync(s => s.StationName.ToLower() == name.ToLower());

            return station;
        }

        public async Task<IEnumerable<Station>> GetAllStationsAsync()
        {
            return await _context.Stations.ToListAsync();
        }

        public async Task<Station> CreateStationAsync([FromBody] AddStationDTO newStation)
        {
            var station = new Station
            {
                StationName = newStation.StationName,
                Location = newStation.Location
            };

            _context.Stations.Add(station);
            await _context.SaveChangesAsync();
            return station;
        }

        public async Task<bool> UpdateStationAsync(int id, UpdateStationDTO station)
        {
            var existing = await _context.Stations.FindAsync(id);
            if (existing == null)
                return false;

            if (!string.IsNullOrWhiteSpace(station.StationName))
                existing.StationName = station.StationName;

            if (!string.IsNullOrWhiteSpace(station.Location))
                existing.Location = station.Location;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteStationAsync(int id)
        {
            var station = await _context.Stations.FindAsync(id);
            if (station == null)
                return false;

            _context.Stations.Remove(station);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
