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
        
        [HttpPost]
        public IActionResult SetIntraoperativeStatus(int shiftId)
        {
            var result = _surgeryService.SetIntraoperativeStatus(shiftId);

            return StatusCode(200, result);
        }
        [HttpPost]
        public IActionResult SetPostoperativeStatus(int shiftId, string roomPost, string bedPost)
        {
            var result = _surgeryService.SetPostoperativeStatus(shiftId, roomPost, bedPost);

            return StatusCode(200, result);
        }
        [HttpGet]
        public IActionResult CheckPostStatus(int shiftId)
        {
            var result = _surgeryService.CheckPostStatus(shiftId);
            return StatusCode(200, result);
        }

        [HttpGet]
        public IActionResult GetSurgeryShiftNoScheduleByProposedTime()
        {
            var result = _surgeryService.GetSurgeryShiftNoScheduleByProposedTime();
            return StatusCode(200, result);
        }

        [HttpGet]
        public IActionResult MakeScheduleList()
        {
            var result = _surgeryService.MakeScheduleList();
            return StatusCode(200, result);
        }

        [HttpPost]
        public IActionResult GetAvailableSlotRoom(int dateNumber)
        {
            var result = _surgeryService.GetAvailableSlotRoom(dateNumber);
            return StatusCode(200, result);
        }

        [HttpGet]
        public IActionResult GetSurgeryShiftsNoSchedule()
        {
            var result = _surgeryService.GetSurgeryShiftsNoSchedule();
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

            return StatusCode(200, result);
        }

        #region Change Schedules
        [HttpPost]
        public IActionResult GetAvailableRoom([FromBody]AvailableRoomParamViewModel param)
        {
            var results = _surgeryService.GetAvailableRoom(param.StartDate, param.EndDate);
            if (results != null)
            {
                return StatusCode(200, results);
            }
            else
            {
                return StatusCode(400, "Time is not valid");
            }
        }

        [HttpGet]
        public IActionResult GetAvailableRoomForDuration(int hour, int minute)
        {
            var results = _surgeryService.GetAvailableRoom(hour, minute);
            return StatusCode(200, results);
        }

        [HttpPost]
        public IActionResult ChangeSchedule([FromBody] ShiftScheduleChangeViewModel newShift)
        {
            var result = _surgeryService.ChangeSchedule(newShift);
            if (result) return StatusCode(200);
            return StatusCode(400);
        }

        [HttpPost]
        public IActionResult ChangeScheduleForDuration([FromBody] ShiftScheduleChangeViewModel newShift)
        {
            var result = _surgeryService.ChangeSchedule(newShift);
            if (result) return StatusCode(200);
            return StatusCode(400);
        }

        [HttpPost]
        public IActionResult ChangeShiftPriority([FromBody] ShiftChangeViewModel newPriority)
        {
            if (_surgeryService.ChangeFirstPriority(newPriority)) return StatusCode(200);
            return StatusCode(400);

        }

        [HttpPost]
        public IActionResult ChangeShiftStatus([FromBody] ShiftStatusChangeViewModel newStatus)
        {
            if (_surgeryService.ChangeShiftStatus(newStatus))
            {
                return StatusCode(200);
            }
            return StatusCode(400);
        }

        [HttpPost]
        public IActionResult SwapShifts([FromBody] SwapShiftViewModel shifts)
        {
            var result = _surgeryService.SwapShift(shifts.FirstShiftId, shifts.SecondShiftId);
            if (result.Succeed)
            {
                return StatusCode(200, result.AffectedShifts);
            }
            else
            {
                return StatusCode(400);
            }
        }

        [HttpGet]
        public IActionResult GetSwapableShifts()
        {
            var results = _surgeryService.GetSwapableShiftIds();
            return StatusCode(200, results);
        }
        #endregion
    }
}