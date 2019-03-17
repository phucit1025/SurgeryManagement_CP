using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Surgery_1.Services.Interfaces;

namespace Surgery_1.Controllers
{
    [Route("api/Status/[action]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly IStatusService _statusService;
        public StatusController(IStatusService _statusService)
        {
            this._statusService = _statusService;
        }

        [HttpPost]
        public IActionResult SetIntraoperativeStatus(int shiftId, string actualStartDateTime)
        {
            var result = _statusService.SetIntraoperativeStatus(shiftId, actualStartDateTime);

            return StatusCode(200, result);
        }

        [HttpPost]
        public IActionResult SetPostoperativeStatus(int shiftId, string roomPost, string bedPost, string actualEndDateTime)
        {
            var result = _statusService.SetPostoperativeStatus(shiftId, roomPost, bedPost, actualEndDateTime);

            return StatusCode(200, result);
        }

        [HttpPost]
        public IActionResult SetFinishedStatus(int shiftId)
        {
            var result = _statusService.SetFinishedStatus(shiftId);

            return StatusCode(200, result);
        }

        [HttpGet]
        public IActionResult GetStatusByShiftId(int shiftId)
        {
            var result = _statusService.GetStatusByShiftId(shiftId);

            return StatusCode(200, result);
        }
    }
}