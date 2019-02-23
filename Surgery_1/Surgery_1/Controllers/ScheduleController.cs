using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Surgery_1.Data.ViewModels;
using Surgery_1.Services.Interfaces;

namespace Surgery_1.Controllers
{
    [Route("api/Schedule/[action]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly ISurgeryService _surgeryService;

        public ScheduleController(ISurgeryService _surgeryService)
        {
            this._surgeryService = _surgeryService;
        }
        [HttpGet]
        public IActionResult GetRoomByMax(int dayNumber)
        {
            var result = _surgeryService.GetRoomByMax(dayNumber);
            return StatusCode(200, result);
        }

        [HttpGet]
        public IActionResult MakeScheduleList()
        {
            _surgeryService.MakeScheduleList();
            return StatusCode(200);
        }

        [HttpPost]
        public IActionResult MakeSchedule([FromBody] ScheduleViewModel scheduleViewModel)
        {
            _surgeryService.MakeSchedule(scheduleViewModel);
            return StatusCode(200);
        }
        [HttpGet]
        public IActionResult GetSurgeryShiftsNoSchedule(int dateNumber)
        {
            var result = _surgeryService.GetSurgeryShiftsNoSchedule(dateNumber);
            return StatusCode(200, result);
        }

        [HttpPost]
        public IActionResult GetSurgeryMaxTime([FromBody] ScheduleViewModel scheduleViewModel)
        {
            var result = _surgeryService.GetRoomByMaxSurgeryTime(scheduleViewModel);
            return StatusCode(200, result);
        }


        [HttpGet]
        public IActionResult GetSurgeryShiftsByRoomAndDate(int roomId, int dayNumber)
        {
            var result = _surgeryService.GetSurgeryShiftsByRoomAndDate(roomId, dayNumber);
            return StatusCode(200, result);
        }
        [HttpGet]
        public IActionResult GetSurgeryRooms()
        {
            var result = _surgeryService.GetSurgeryRooms();
            return StatusCode(200, result);
        }

        [HttpGet]
        public IActionResult GetSurgeryShiftDetail(int shiftId)
        {
            var result = _surgeryService.GetShiftDetail(shiftId);

            if(result!=null)return StatusCode(200, result);
            return StatusCode(400);
        }
    }
}