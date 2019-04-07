using Microsoft.AspNetCore.Mvc;
using Surgery_1.Data.ViewModels;
using Surgery_1.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surgery_1.Controllers
{
    [Route("api/Specialties/[action]")]
    [ApiController]
    public class SpecialtyController: ControllerBase
    {
        private readonly ISpecialtyService _specialtyService;
        public SpecialtyController(ISpecialtyService specialtyService)
        {
            _specialtyService = specialtyService;
        }

        [HttpGet]
        public void AddSpecialtyGroup(String specialityGroupName)
        {
            _specialtyService.AddSpecialtyGroup(specialityGroupName);
        }

        [HttpPost]
        public void SpecialtiesSpecialtyGroup([FromBody]SpecialtySpecialtyGroupViewModel group)
        {
            _specialtyService.SpecialtiesSpecialtyGroup(group);
        }

        [HttpGet]
        public IActionResult GetSpecialtyGroups()
        {
            var result = _specialtyService.GetSpecialtyGroups();
            return Ok(result);
        }

        [HttpPost]
        public void SurgeryRoomSpecialtyGroup([FromBody]SurgeryRoomSpecialtyGroupViewModel groupRoom)
        {
            _specialtyService.SurgeryRoomSpecialtyGroup(groupRoom);
        }
    }
}
