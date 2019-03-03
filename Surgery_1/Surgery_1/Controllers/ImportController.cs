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
        public bool ImportSurgeryShift([FromBody]ICollection<ImportSurgeryShiftViewModel> surgeryShift)
        {
            _surgeryShiftService.ImportSurgeryShift(surgeryShift);
            return true;
        }

        [HttpPost]
        public bool ImportSurgeryShiftMedicalSupply([FromBody]List<ImportMedicalSupplyViewModel> surgeryShiftSupply)
        {
            _surgeryShiftService.ImportSurgeryShiftMedicalSupply(surgeryShiftSupply);
            return true;
        }

        [HttpPost]
        public IActionResult getSurgeryNameById([FromBody]ICollection<SurgeryCatalogIDsViewModel> SurgeryId)
        {
            var result = _surgeryShiftService.GetSurgeryName(SurgeryId);
            return Ok(result);
        }
    }
}
