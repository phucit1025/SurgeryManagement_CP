using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Surgery_1.Services.Interfaces;

namespace Surgery_1.Controllers
{
    [Route("api/postop")]
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
    }
}