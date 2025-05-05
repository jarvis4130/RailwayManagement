using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RailwayManagementApi.Data;
using RailwayManagementApi.Interfaces;

namespace RailwayManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StationController : ControllerBase
    {
        private readonly RailwayContext _dbContext;
        private readonly IStationService _stationService;

        public StationController(RailwayContext dbContext,IStationService stationService)
        {
            _dbContext = dbContext;
            _stationService=stationService;
        }

        // GET: api/station/id?name=Mumbai
        [HttpGet("id")]
        public async Task<ActionResult<int>> GetStationIdByName([FromQuery] string name)
        {
            try
            {
                var stationId = await _stationService.GetStationIdByNameAsync(name);
                if (stationId == 0)
                    return NotFound($"Station with name '{name}' not found.");

                return Ok(stationId);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

    }

}