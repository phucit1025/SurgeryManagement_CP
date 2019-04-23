using Castle.Core.Internal;
using Microsoft.AspNetCore.Mvc;
using Surgery_1.Data.ViewModels;
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

        [HttpPost]
        public IActionResult UpdateSurgeryProfile([FromBody]EditSurgeryShiftViewModel editForm)
        {
            var result = _surgeryShiftService.UpdateSurgeryProfile(editForm);
            return StatusCode(200, result);
        }
        [HttpGet]
        public IActionResult LoadEditSurgeryProfile(int shiftId)
        {
            var result = _surgeryShiftService.LoadEditSurgeryProfile(shiftId);
            return StatusCode(200, result);
        }

        [HttpGet]
        public IActionResult GetSurgeryCatalogOnQuery(string searchName)
        {
            var result = _surgeryShiftService.GetSurgeryCatalogOnQuery(searchName);
            return Ok(result);
        }

        [HttpGet]
        public IActionResult GetAvailableSurgeons(int surgeryShiftId)
        {
            var result = _surgeryShiftService.GetAvailableSurgeons(surgeryShiftId);
            return StatusCode(200, result);
        }

        [HttpGet]
        public IActionResult GetShiftSurgeons(int surgeryShiftId)
        {
            var results = _surgeryShiftService.GetShiftSurgeons(surgeryShiftId);
            return StatusCode(200, results);
        }

        [HttpPost]
        public IActionResult UpdateSurgeon([FromBody]UpdateSurgeonsViewModel model)
        {
            var result = _surgeryShiftService.UpdateSurgeon(model);
            if (result)
            {
                return StatusCode(200, result);
            }
            else
            {
                return StatusCode(400);
            }
        }

        [HttpPost]
        public IActionResult AddSurgeon([FromBody]AddSurgeonToShiftViewModel model)
        {
            var result = _surgeryShiftService.AddSurgeon(model);
            if (result)
            {
                return StatusCode(200);
            }
            else
            {
                return StatusCode(400);
            }
        }

        [HttpDelete]
        public IActionResult RemoveSurgeon(int surgeryShiftId, int surgeonId)
        {
            var result = _surgeryShiftService.RemoveSurgeon(new RemoveSurgeonFromShiftViewModel()
            {
                SurgeonId = surgeonId,
                SurgeryShiftId = surgeryShiftId
            });
            if (result)
            {
                return StatusCode(200);
            }
            else
            {
                return StatusCode(400);
            }
        }
    }
}
