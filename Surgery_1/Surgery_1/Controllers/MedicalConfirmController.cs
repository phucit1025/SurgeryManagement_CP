using Microsoft.AspNetCore.Mvc;
using Surgery_1.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surgery_1.Controllers
{

    [Route("api/MedicalConfirm/[action]")]
    [ApiController]
    public class MedicalConfirmController: ControllerBase
    {
        private readonly IMedicalSupplyConfirmService _confirmService;
        public MedicalConfirmController(IMedicalSupplyConfirmService confirmService)
        {
            _confirmService = confirmService;
        }

        [HttpGet]
        public IActionResult GetAllMedicalSupplyRequest()
        {
            var result = _confirmService.GetAllMedicalSupplyRequest();
            return Ok(result);
        }

        [HttpGet]
        public void ConfirmMedicalRequest(int surgeryShiftId)
        {
            var result = _confirmService.ConfirmedSupply(surgeryShiftId);
        }

        [HttpGet]
        public IActionResult GetMedicalSupplyRequest(int surgeryShiftId)
        {
            var result = _confirmService.GetMedicalSupplyRequestDetail(surgeryShiftId);
            return Ok(result);
        }

        [HttpGet]
        public void ConfirmedAllSupplyRequest()
        {
            _confirmService.ConfirmedAllSupplyRequest();
        }
    }
}
