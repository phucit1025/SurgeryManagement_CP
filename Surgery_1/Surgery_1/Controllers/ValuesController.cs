using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Surgery_1.Data.ViewModels;
using Surgery_1.Services.Interfaces;

namespace Surgery_1.Controllers
{
    [Route("api/Account/[action]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public ValuesController(IAccountService _accountService)
        {
            this._accountService = _accountService;
        }
        
        [HttpGet]

        public IActionResult Login([FromBody] LoginViewModel loginViewModel)
        {
            var result= _accountService.Login(loginViewModel);
            return StatusCode(200, result);
        }

        
    }
}
