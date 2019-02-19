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
        public IActionResult GetSurgeryMaxTime([FromBody] ScheduleViewModel scheduleViewModel)
        {
            var result = _surgeryService.GetRoomByMaxSurgeryTime(scheduleViewModel);
            return StatusCode(200, result);
        }
    }
}