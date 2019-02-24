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
    [Route("api/PostOp/[action]")]
    [ApiController]
    public class PostOpController : ControllerBase
    {
        private readonly IPostOpService _postOpService;
        public PostOpController(IPostOpService postOpService)
        {
            this._postOpService = postOpService;
        }

        [HttpGet]
        public IActionResult GetSurgeryByStatusId(int statusId)
        {
            var result = _postOpService.GetSurgeryByStatusId(statusId);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet]
        public IActionResult GetHealthCareReportBySurgeryShiftId(int surgeryShiftId)
        {
            var result = _postOpService.GetHealthCareRerportBySurgeryShiftId(surgeryShiftId);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet]
        public IActionResult ChangeSurgeryShiftToRecovery(int surgeryShiftId)
        {
            var result = _postOpService.ChangeSurgeryShiftToRecovery(surgeryShiftId);
            if (result)
            {
                return Ok(result);
            }
            return BadRequest();
        }

        [HttpGet]
        public IActionResult GetRecoverySurgeryShiftById(int surgeryShiftId)
        {
            var result = _postOpService.GetRecoverySurgeryShiftById(surgeryShiftId);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPost]
        public IActionResult CreateHealthCareReport(HealthCareReportViewModel healthCareReportViewModel)
        {
            var result = _postOpService.CreateHealthCareReport(healthCareReportViewModel);
            if (result)
            {
                return Ok(result);
            }
            return BadRequest();
        }

        [HttpPost]
        public IActionResult UpdateHealthCareReport(HealthCareReportViewModel healthCareReportViewModel)
        {
            var result = _postOpService.UpdateHealthCareReport(healthCareReportViewModel);
            if (result)
            {
                return Ok(result);
            }
            return BadRequest();
        }

        [HttpGet]
        public IActionResult SoftDeleteHealthCareReport(int healthCareReportId)
        {
            var result = _postOpService.SoftDeleteHealthCareReport(healthCareReportId);
            if (result)
            {
                return Ok(result);
            }
            return BadRequest();
        }
    }
}