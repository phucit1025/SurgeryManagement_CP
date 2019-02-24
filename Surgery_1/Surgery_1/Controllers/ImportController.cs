using Microsoft.AspNetCore.Mvc;
using Surgery_1.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surgery_1.Controllers
{
    [Route("api/Import/[action]")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly ISurgeryShiftService _surgeryShiftService;
        public ImportController(ISurgeryShiftService surgeryShiftService)
        {
            _surgeryShiftService = surgeryShiftService;
        }

        public bool ImportSurgeryShift()
        {
            return true;
        }
    }
}
