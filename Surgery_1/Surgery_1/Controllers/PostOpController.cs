using System;
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
        public IActionResult EditTreatmentReport(TreatmentReportViewModel treatmentReportViewModel)
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
        public IActionResult CreateSurgeryPdf(int id, int type)
        {
            string styleSheets = Path.Combine(Directory.GetCurrentDirectory(), "assets", "styles.css");
            string path = Path.Combine(Directory.GetCurrentDirectory(), "assets", "ebsmslogo.jpg");
            var result = _postOpService.CreateSurgeryPdf(path, id, type);
            if (!result.IsNullOrEmpty())
            {
                return File(result, "application/pdf", "SurgeryReport.pdf");
            }
            return NotFound();
        }

        [HttpGet]
        public IActionResult GetDrugTimelineByShiftID(int surgeryShiftId)
        {
            var result = _postOpService.GetDrugTimelineByShiftIdAndDate(surgeryShiftId);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [Authorize]
        [HttpGet]
        public IActionResult ConfirmTakeMedicine(int treatmentReportDrugId, string time)
        {
            var result = _postOpService.ConfirmTakeMedicine(treatmentReportDrugId, time);
            if (result)
            {
                return Ok(result);
            }
            return BadRequest();
        }

        [HttpGet]
        public IActionResult GetHealthcareSurgeryShift()
        {
            var result = _postOpService.GetHealthcareSurgeryShifts();
            if (!result.IsNullOrEmpty())
            {
                return Ok(result);
            }
            return NotFound();
        }
        #region Statistical
        [HttpGet]
        public IActionResult GetPostOpSurgeryShift(DateTime actualEnd, int speacialtyId, int surgeryId, int doctorId, int? status, int pageSize = 10, int pageIndex = 0)
        {
            if (pageIndex != 0)
            {
                pageIndex -= 1;
            }
            try
            {
                var results = _postOpService.GetPostOpSurgeryShift(actualEnd, speacialtyId, surgeryId, doctorId, status);
                var totalPage = Math.Ceiling((double)results.Count / pageSize);
                var total = results.Count;
                var totalPreop = results.Where(r => r.StatusName.Contains("Preoperative", StringComparison.CurrentCultureIgnoreCase)).Count();
                var totalPostop = results.Where(r => r.StatusName.Contains("Postoperative", StringComparison.CurrentCultureIgnoreCase)).Count();
                var totalRecovery = results.Where(r => r.StatusName.Contains("Recovery", StringComparison.CurrentCultureIgnoreCase)).Count();
                var totalIntra = results.Where(r => r.StatusName.Contains("Intraoperative", StringComparison.CurrentCultureIgnoreCase)).Count();
                var totalFinished = results.Where(r => r.StatusName.Contains("Finished", StringComparison.CurrentCultureIgnoreCase)).Count();
                results = results.Skip(pageSize * (pageIndex)).Take(pageSize).ToList();

                return StatusCode(200,
                    new
                    {
                        results = results,
                        totalPage = totalPage,
                        total,
                        totalPreop,
                        totalPostop,
                        totalRecovery,
                        totalIntra,
                        totalFinished
                    });
            }
            catch (Exception)
            {
                return StatusCode(400);
            }
        }

        [HttpGet]
        public IActionResult GetDoctors(string name, int pageSize = 10, int pageIndex = 0)
        {
            var results = _postOpService.GetDoctors(name);

            var totalPage = Math.Ceiling((double)results.Count / pageSize);
            results = results.Skip(pageSize * (pageIndex)).Take(pageSize).ToList();

            return StatusCode(200, new { doctors = results, totalPage });
        }

        [HttpGet]
        public IActionResult GetSpecialties(string name, int pageSize = 10, int pageIndex = 0)
        {
            var results = _postOpService.GetSpecialties(name);

            var totalPage = Math.Ceiling((double)(results.Count / pageSize));
            results = results.Skip(pageSize * (pageIndex)).Take(pageSize).ToList();

            return StatusCode(200, new { specialties = results, totalPage });
        }

        [HttpGet]
        public IActionResult GetCatalogs(string name, int specialtyId = 0, int pageSize = 10, int pageIndex = 0)
        {
            var results = _postOpService.GetSurgeryCatalogs(name, specialtyId);

            var totalPage = Math.Ceiling((double)(results.Count / pageSize));
            results = results.Skip(pageSize * (pageIndex)).Take(pageSize).ToList();

            return StatusCode(200, new { catalogs = results, totalPage });
        }
        #endregion

    }
}