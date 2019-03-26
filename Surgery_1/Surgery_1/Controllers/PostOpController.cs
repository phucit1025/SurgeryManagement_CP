﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Microsoft.AspNetCore.Authorization;
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

        [Authorize]
        [HttpGet]
        public IActionResult GetSurgeryByStatusId(int statusId)
        {
            var result = _postOpService.GetSurgeryByStatusId(statusId);
            if (result.IsNullOrEmpty())
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
        public IActionResult ChangeSurgeryShiftToRecovery(int surgeryShiftId, string postOpRoom, string postOpBed)
        {
            var result = _postOpService.ChangeSurgeryShiftToRecovery(surgeryShiftId, postOpRoom, postOpBed);
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

        [Authorize]
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

        [Authorize]
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

        [HttpGet]
        public IActionResult SoftDeleteTreatmentReport(int id)
        {
            var result = _postOpService.SoftDeleteTreatmentReport(id);
            if (result)
            {
                return Ok(result);
            }
            return BadRequest();
        }

        [Authorize]
        [HttpGet]
        public IActionResult FindPostOpSurgeryByQuery(string query)
        {
            var result = _postOpService.FindPostOpSurgeryByQuery(query);
            if (result.IsNullOrEmpty())
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet]
        public IActionResult EditRoomBedSurgeryShift(int surgeryShiftId, string room, string bed)
        {
            var result = _postOpService.EditRoomBedSurgeryShift(surgeryShiftId, room, bed);
            if (result)
            {
                return Ok(result);
            }
            return BadRequest();
        }
       
        [HttpPost]
        public IActionResult CreateTreatmenReport(TreatmentReportViewModel treatmentReportViewModel)
        {
            var result = _postOpService.CreateTreatmenReport(treatmentReportViewModel);
            if (result)
            {
                return Ok(result);
            }
            return BadRequest();
        }

        [HttpPost]
        public IActionResult get(TreatmentReportViewModel treatmentReportViewModel)
        {
            var result = _postOpService.EditTreatmentReport(treatmentReportViewModel);
            if (result)
            {
                return Ok(result);
            }
            return BadRequest();
        }
        

       [HttpPost]
        public IActionResult CreateTreatmentReportDrugs([FromBody]ICollection<TreatmentReportDrugViewModel> treatmentReportDrugs)
        {
            var result = _postOpService.CreateTreatmentReportDrugs(treatmentReportDrugs);
            if (result)
            {
                return Ok(result);
            }
            return BadRequest();
        }

        [HttpGet]
        public IActionResult GetTreatmentReportByShiftId(int surgeryShiftId)
        {
            var result = _postOpService.GetTreatmentReportByShiftId(surgeryShiftId);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
        
        [HttpGet]
        public IActionResult GetTodayTreatmentReportByShiftId(int surgeryShiftId)
        {
            var result = _postOpService.GetTodayTreatmentReportByShiftId(surgeryShiftId);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet]
        public IActionResult GetDrugRequirementForNurse(int shiftId)
        {
            var result = _postOpService.GetDrugRequirementForNurse(shiftId);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet]
        public IActionResult GetTreatmentReportById(int id)
        {
            var result = _postOpService.GetTreatmentReportById(id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
       
        [HttpGet]
        public IActionResult AssignNurse(int shiftId, int nurseId)
        {
            var result = _postOpService.AssignNurse(shiftId, nurseId).Result;
            if (result)
            {
                return Ok(result);
            }
            return BadRequest();
        }

        [HttpGet]
        public IActionResult GetAllNurse()
        {
            var result = _postOpService.GetAllNurse().Result;
            if (!result.IsNullOrEmpty())
            {
                return Ok(result);
            }
            return NotFound();
        }

        [HttpGet]
        public IActionResult GetNurseByShiftId(int shiftId)
        {
            var result = _postOpService.GetNurseByShiftId(shiftId);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
       
        [HttpGet]
        public IActionResult CreateSurgeryPdf(int id)
        {
            string styleSheets = Path.Combine(Directory.GetCurrentDirectory(), "assets", "styles.css");
            var result = _postOpService.CreateSurgeryPdf(styleSheets, id);
            if (!result.IsNullOrEmpty())
            {
                return File(result, "application/pdf", "EmployeeReport.pdf");
            }
            return NotFound();
        }
    }
}