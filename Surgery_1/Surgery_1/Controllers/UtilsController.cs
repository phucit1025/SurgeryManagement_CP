using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Surgery_1.Services.Interfaces;
using Surgery_1.Data.ViewModels;

namespace Surgery_1.Controllers
{
    [Route("api/Utils/[action]")]
    [ApiController]
    public class UtilsController : ControllerBase
    {
        private readonly IUtilsService _utilsService;

        public UtilsController(IUtilsService _utilsService)
        {
            this._utilsService = _utilsService;
        }

        [HttpGet]
        public IActionResult GetMedicalSupplies()
        {
            var result = _utilsService.GetMedicalSupply();
            return StatusCode(200, result);
        }
    }
}
