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
        private readonly IRoomService _roomService;
        private readonly ISurgeryShiftService _surgeryShiftService;

        public ScheduleController(ISurgeryService _surgeryService, IRoomService _roomService,
            ISurgeryShiftService _surgeryShiftService)
        {
            this._surgeryService = _surgeryService;
            this._roomService = _roomService;
            this._surgeryShiftService = _surgeryShiftService;
        }

        [HttpPost]
        public void AddUsedMedicalSupply([FromBody]ShiftMedicalSuppliesViewModel medicalSupplyAddList)
        {
            _surgeryShiftService.AddMedicalSupply(medicalSupplyAddList);
        }

        [HttpGet]
        public IActionResult GetUsedSupply(int surgeryShiftId)
        {
            var result = _surgeryShiftService.GetSuppliesUsedInSurgery(surgeryShiftId);
            return StatusCode(200, result);
        }

        [HttpGet]
        public IActionResult GetSlotRooms()
        {
            var result = _surgeryService.GetSlotRooms();
            return StatusCode(200, result);
        }

        [HttpGet]
        public IActionResult GetServerTime()
        {
            String dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            return StatusCode(200, dateTime);
        }

        [HttpGet]
        public IActionResult CheckStatusPreviousSurgeryShift(int shiftId)
        {
            var result = _surgeryService.CheckStatusPreviousSurgeryShift(shiftId);
            return StatusCode(200, result);
        }

        #region Make Schedule

        [HttpPost]
        public IActionResult RefreshSurgeryShift(int shiftId)
        {
            var result = _surgeryService.RefreshSurgeryShift(shiftId);
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

        [HttpPost]
        public IActionResult GetAvailableRoomForProposedTime([FromBody] EmerSurgeryShift emerShift)
        {
            var result = _surgeryService.GetAvailableRoomForProposedTime(emerShift);
            return StatusCode(200, result);
        }
        [HttpPost]
        public IActionResult AddEmergencyShift([FromBody] EmerSurgeryShift emerShift)
        {
            if (_surgeryService.AddEmergencyShift(emerShift))
            {
                return StatusCode(200, true);
            }
            return StatusCode(200, false);
        }

        [HttpGet]
        public IActionResult GetSurgeryShiftsNoSchedule()
        {
            var result = _surgeryService.GetSurgeryShiftsNoSchedule();
            return StatusCode(200, result);
        }

        [HttpGet]
        public IActionResult GetSurgeryShiftNoScheduleByProposedTime()
        {
            var result = _surgeryService.GetSurgeryShiftNoScheduleByProposedTime();
            return StatusCode(200, result);
        }
        #endregion

        #region Load Schedule
        [HttpGet]
        public IActionResult GetSurgeryShiftsByRoomAndDate(int slotRoomId, int dayNumber)
        {
            var result = _surgeryService.GetSurgeryShiftsByRoomAndDate(slotRoomId, dayNumber);
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
        #endregion


        [HttpPost]
        public Boolean SaveSurgeryProcedure([FromBody]SurgeryProcedureViewModel SurgeryProcedure)
        {
            _surgeryService.SaveSurgeryProcedure(SurgeryProcedure);
            return true;
        }

        #region Change Schedules
        [HttpGet]
        public IActionResult GetRoomInfo(int id)
        {
            return StatusCode(200, _roomService.GetRoom(id));
        }


        [HttpPost]
        public IActionResult GetAvailableRoom([FromBody]AvailableRoomParamViewModel param)
        {
            var results = _surgeryService.GetAvailableRoom(param.StartDate, param.EndDate, param.ForcedChange);
            if (results != null)
            {
                return StatusCode(200, results);
            }
            else
            {
                if (param.ForcedChange)
                {
                    return StatusCode(400, "There was an error during getting room.");
                }
                else
                {
                    return StatusCode(400, "Start Time cannot beyond 17:00.");
                }
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

        [HttpPost]
        public IActionResult SwapShiftToRoom([FromBody] SwapShiftToRoomViewModel shift)
        {
            var result = _surgeryService.SwapShiftToRoom(shift.ShiftId, shift.RoomId, shift.ForcedSwap);
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