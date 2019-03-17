using Microsoft.AspNetCore.Mvc;
using Surgery_1.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surgery_1.Controllers
{
    [Route("api/SurgeryShift/[action]")]
    [ApiController]
    public class SurgeryShiftController : ControllerBase
    {
        private readonly ISurgeryShiftService _surgeryShiftService;
        public SurgeryShiftController(ISurgeryShiftService _surgeryShiftService)
        {            
            this._surgeryShiftService = _surgeryShiftService;
        }

        [HttpGet]
        public IActionResult GetEkipMember(int surgeryShiftId)
        {
            var result = _surgeryShiftService.GetEkipMember(surgeryShiftId);
            return StatusCode(200, result);
        }
    }
}
