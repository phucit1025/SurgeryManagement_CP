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
    [Route("api/Drug/[action]")]
    [ApiController]
    public class DrugController : ControllerBase
    {
        private readonly IDrugService _drugService;
        public DrugController(IDrugService drugService)
        {
            _drugService = drugService;
        }

        [HttpPost]
        public bool ImportDrug([FromBody]ICollection<DrugViewModel> drugs)
        {
            _drugService.ImportDrug(drugs);
            return true;
        }

        [HttpGet]
        public IActionResult GetAllDrugs()
        {
            var result = _drugService.GetAllDrug();
            if (!result.Any())
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet]
        public IActionResult SearchDrugOnQuery(string q)
        {
            var result = _drugService.SearchDrugOnQuery(q);
            if (!result.Any())
            {
                return NotFound();
            }
            return Ok(result);
        }
    }
}