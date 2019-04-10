using Microsoft.AspNetCore.Mvc;
using Surgery_1.Data.ViewModels;
using Surgery_1.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surgery_1.Controllers
{
    [Route("api/Specialities/[action]")]
    [ApiController]
    public class SpecialityController : ControllerBase
    {
        private readonly ISpecialityService _specialityService;
        public SpecialityController(ISpecialityService specialityService)
        {
            _specialityService = specialityService;
        }

        [HttpPost]
        public IActionResult SetCatalogToSpeciality([FromBody] CatalogToSpecialityViewModel model)
        {
            var result = _specialityService.SetCatalogToSpeciality(model);
            if (result)
            {
                return StatusCode(200);
            }
            else
            {
                return StatusCode(400);
            }
        }

        [HttpPost]
        public IActionResult SetSpecialityToGroup([FromBody]SpecialitySpecialityGroupViewModel group)
        {
            var result = _specialityService.AddSpecialityToGroup(group);
            if (result)
            {
                return StatusCode(200);
            }
            else
            {
                return StatusCode(400);
            }
        }

        [HttpPost]
        public IActionResult SetSpecialityGroupToRoom([FromBody]SurgeryRoomSpecialityGroupCreateViewModel groupRoom)
        {
            var result = _specialityService.SetSpecialityToRoom(groupRoom);
            if (result)
            {
                return StatusCode(200);
            }
            else
            {
                return StatusCode(400);
            }
        }

        [HttpGet]
        public IActionResult GetSpecialityGroups()
        {
            var results = _specialityService.GetSpecialityGroups();
            return StatusCode(200, results);
        }

        [HttpGet]
        public IActionResult GetSpecialities()
        {
            var results = _specialityService.GetSpecialities();
            return StatusCode(200, results);
        }

        [HttpGet]
        public IActionResult GetSpecialitiesInGroup(int groupId)
        {
            var results = _specialityService.GetSpecialities(groupId);
            return StatusCode(200, results);
        }

        [HttpGet]
        public IActionResult GetSurgeryCatalogs()
        {
            var results = _specialityService.GetCatalogs().Take(100).ToList();
            return StatusCode(200, results);
        }

        [HttpGet]
        public IActionResult GetRooms()
        {
            var results = _specialityService.GetRooms();
            return StatusCode(200, results);
        }

        [HttpPost]
        public IActionResult CreateSpeciality([FromBody] SpecialityCreateViewModel model)
        {
            var result = _specialityService.CreateSpeciality(model.Name);
            if (result != 0)
            {
                return StatusCode(200, result);
            }
            else
            {
                return StatusCode(400);
            }
        }

        [HttpPost]
        public IActionResult CreateSpecialityGroup([FromBody] SpecialityGroupCreateViewModel model)
        {
            var result = _specialityService.CreateSpecialityGroup(model.Name);
            if (result != 0)
            {
                return StatusCode(200, result);
            }
            else
            {
                return StatusCode(400);
            }
        }
    }
}
