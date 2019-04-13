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
    public class SpecialtyController : ControllerBase
    {
        private readonly ISpecialtyService _SpecialtyService;
        public SpecialtyController(ISpecialtyService SpecialtyService)
        {
            _SpecialtyService = SpecialtyService;
        }

        [HttpPost]
        public IActionResult SetCatalogToSpecialty([FromBody] CatalogToSpecialtyViewModel model)
        {
            var result = _SpecialtyService.SetCatalogToSpecialty(model);
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
        public IActionResult SetSpecialtyToGroup([FromBody]SpecialtySpecialtyGroupViewModel group)
        {
            var result = _SpecialtyService.AddSpecialtyToGroup(group);
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
        public IActionResult SetSpecialtyGroupToRoom([FromBody]SurgeryRoomSpecialtyGroupCreateViewModel groupRoom)
        {
            var result = _SpecialtyService.SetSpecialtyToRoom(groupRoom);
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
        public IActionResult GetSpecialtyGroups()
        {
            var results = _SpecialtyService.GetSpecialtyGroups();
            return StatusCode(200, results);
        }

        [HttpGet]
        public IActionResult GetSpecialties()
        {
            var results = _SpecialtyService.GetSpecialties();
            return StatusCode(200, results);
        }

        [HttpGet]
        public IActionResult GetSpecialtiesInGroup(int groupId)
        {
            var results = _SpecialtyService.GetSpecialties(groupId);
            return StatusCode(200, results);
        }

        [HttpGet]
        public IActionResult GetSurgeryCatalogs()
        {
            var results = _SpecialtyService.GetCatalogs().Take(100).ToList();
            return StatusCode(200, results);
        }

        [HttpGet]
        public IActionResult GetRooms()
        {
            var results = _SpecialtyService.GetRooms();
            return StatusCode(200, results);
        }

        [HttpPost]
        public IActionResult CreateSpecialty([FromBody] SpecialtyCreateViewModel model)
        {
            var result = _SpecialtyService.CreateSpecialty(model.Name);
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
        public IActionResult CreateSpecialtyGroup([FromBody] SpecialtyGroupCreateViewModel model)
        {
            var result = _SpecialtyService.CreateSpecialtyGroup(model.Name);
            if (result != 0)
            {
                return StatusCode(200, result);
            }
            else
            {
                return StatusCode(400, new { message = "This Name Is Being Used !" });
            }
        }
    }
}
