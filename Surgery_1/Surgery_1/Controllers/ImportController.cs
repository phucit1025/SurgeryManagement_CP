using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Surgery_1.Data.ViewModels;
using Surgery_1.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Surgery_1.Data.ViewModels.PostOpSurgeryShiftViewModel;

namespace Surgery_1.Controllers
{
    [EnableCors("AllowAll")]
    [Route("api/Import/[action]")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly ISurgeryShiftService _surgeryShiftService;
        public ImportController(ISurgeryShiftService surgeryShiftService)
        {
            _surgeryShiftService = surgeryShiftService;
        }

        [HttpPost]
        public IActionResult ImportSurgeryShift([FromBody]ImportViewModel importViewModel)
        {
            var result = _surgeryShiftService.ImportSurgeryShift(importViewModel.surgeryShifts);
            if (result)
            {
                return StatusCode(200, result);
            }
            return StatusCode(500);
        }
    }
}
