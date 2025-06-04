// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using RailwayManagementApi.Data;
// using RailwayManagementApi.Interfaces;

// namespace RailwayManagementApi.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class StationController : ControllerBase
//     {
//         private readonly RailwayContext _dbContext;
//         private readonly IStationService _stationService;

//         public StationController(RailwayContext dbContext,IStationService stationService)
//         {
//             _dbContext = dbContext;
//             _stationService=stationService;
//         }

//         // GET: api/station/id?name=Mumbai
//         [HttpGet("id")]
//         public async Task<ActionResult<int>> GetStationIdByName([FromQuery] string name)
//         {
//             try
//             {
//                 var stationId = await _stationService.GetStationIdByNameAsync(name);
//                 if (stationId == 0)
//                     return NotFound($"Station with name '{name}' not found.");

//                 return Ok(stationId);
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, $"Internal server error: {ex.Message}");
//             }
//         }

//     }

// }

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailwayManagementApi.Data;
using RailwayManagementApi.DTOs.StationDTO;
using RailwayManagementApi.Interfaces;
using RailwayManagementApi.Models;

namespace RailwayManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StationController : ControllerBase
    {
        private readonly RailwayContext _dbContext;
        private readonly IStationService _stationService;

        public StationController(RailwayContext dbContext, IStationService stationService)
        {
            _dbContext = dbContext;
            _stationService = stationService;
        }

        // GET: api/station
        
        [Authorize(Roles ="Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Station>>> GetAllStations()
        {
            return Ok(await _stationService.GetAllStationsAsync());
        }

        // GET: api/station/id?name=Mumbai
        [HttpGet("id")]
        public async Task<ActionResult<Station>> GetStationIdByName([FromQuery] string name)
        {
            try
            {
                var station = await _stationService.GetStationIdByNameAsync(name);
                if (station==null)
                    return NotFound($"Station with name '{name}' not found.");

                return Ok(station);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/station
        [Authorize(Roles ="Admin")]
        [HttpPost]
        public async Task<ActionResult> CreateStation([FromBody] AddStationDTO station)
        {
            var created = await _stationService.CreateStationAsync(station);
            return CreatedAtAction(nameof(GetAllStations), new { id = created.StationID }, created);
        } 

        // PUT: api/station/{id}
        [Authorize(Roles ="Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateStation(int id, [FromBody] UpdateStationDTO station)
        {
            var updated = await _stationService.UpdateStationAsync(id,station);
            if (!updated)
                return NotFound($"Station with ID {id} not found.");

            return NoContent();
        }

        // DELETE: api/station/{id}
        [Authorize(Roles ="Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteStation(int id)
        {
            var deleted = await _stationService.DeleteStationAsync(id);
            if (!deleted)
                return NotFound($"Station with ID {id} not found.");

            return NoContent();
        }
    }
}
