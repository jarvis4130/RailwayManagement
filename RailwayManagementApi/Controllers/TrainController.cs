using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RailwayManagementApi.DTOs;
using RailwayManagementApi.Models;
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

        [Authorize(Roles = "Admin")]
        [HttpGet("all-trains")]
        public async Task<ActionResult<IEnumerable<TrainDTOAdmin>>> GetAllTrains()
        {
            var trains = await _trainService.GetAllTrainsAsync();
            return Ok(trains);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("add-train")]
        public async Task<ActionResult<Train>> AddTrain([FromBody] AddTrainReq request)
        {
            var train = await _trainService.AddTrainAsync(request);
            return CreatedAtAction(nameof(GetAllTrains), new { id = train.TrainID }, train);
        }

        // DELETE: api/Train/5

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrain(int id)
        {
            var success = await _trainService.DeleteTrainAsync(id);
            if (!success)
                return NotFound();

            return NoContent(); // 204
        }

        // PUT: api/Train/5

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<Train>> UpdateTrain(int id, [FromBody] AddTrainReq request)
        {
            var updatedTrain = await _trainService.UpdateTrainAsync(id, request);
            if (updatedTrain == null)
                return NotFound();

            return Ok(updatedTrain);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("schedule-train")]
        public async Task<IActionResult> AddSchedule([FromBody] TrainScheduleDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var addedSchedule = await _trainService.AddScheduleAsync(dto);
            return Ok(new { message = "Schedule added successfully", schedule = addedSchedule });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("trains-by-date")]
        public IActionResult GetTrainIdsByDate([FromQuery] string date)
        {
            try
            {
                var trainIds = _trainService.GetTrainIdsByDate(date);
                return Ok(trainIds);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("schedule-by-train-and-date")]
        public IActionResult GetScheduleByTrainAndDate([FromQuery] int trainId, [FromQuery] string date)
        {
            if (!DateTime.TryParse(date, out var parsedDate))
                return BadRequest("Invalid date format");

            var schedules = _trainService.GetScheduleByTrainAndDate(trainId, parsedDate);
            return Ok(schedules);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update-schedule")]
        public async Task<IActionResult> UpdateTrainSchedule([FromBody] UpdateScheduleDto dto)
        {
            await _trainService.UpdateTrainScheduleAsync(dto);
            return Ok(new { message = "Schedule updated successfully" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteSchedule(int trainId, DateTime arrivalDate)
        {
            var result = await _trainService.DeleteScheduleAsync(trainId, arrivalDate);

            if (!result)
                return NotFound(new { message = "Schedule not found for the given TrainID, StationID, and ArrivalDate." });

            return Ok(new { message = "Schedule deleted successfully." });;
        }
    }
}
