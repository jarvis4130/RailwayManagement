using Microsoft.AspNetCore.Mvc;
using RailwayManagementApi.DTOs;
using RailwayManagementApi.Services;

namespace RailwayManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrainController : ControllerBase
    {
        private readonly ITrainService _trainService;

        public TrainController(ITrainService trainService)
        {
            _trainService = trainService;
        }

        // {
        //   "source": "Mumbai",
        //   "destination": "Pune",
        //   "journeyDate": "2025-05-01"
        // }
        [HttpPost("search")]
        public async Task<ActionResult<List<TrainDTO>>> SearchTrainsAsync([FromBody] TrainSearchDTO searchDto)
        {
            try
            {
                var trains = await _trainService.SearchTrainsAsync(searchDto);
                return Ok(trains);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // 1 mumbai pune
        [HttpGet("{trainId}/availability")]
        public async Task<ActionResult<List<TrainAvailabilityDayDTO>>> GetTrainAvailabilityForNext7Days(int trainId, [FromQuery] string source, [FromQuery] string destination)
        {
            try
            {
                var availability = await _trainService.GetTrainAvailabilityForNext7Days(trainId, source, destination);
                return Ok(availability);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
