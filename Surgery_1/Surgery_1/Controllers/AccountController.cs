using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Microsoft.AspNetCore.Mvc;
using Surgery_1.Data.ViewModels;
using Surgery_1.Services.Interfaces;

namespace Surgery_1.Controllers
{
    [Route("api/Account/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService _accountService)
        {
            this._accountService = _accountService;
        }

        [HttpPost]
        public IActionResult Login([FromBody] LoginViewModel loginViewModel)
        {
            var result = _accountService.Authenticate(loginViewModel.Username, loginViewModel.Password).Result;
            if (!result.IsNullOrEmpty())
            {
                var role = _accountService.GetRoleName(loginViewModel.Username).Result;
                return StatusCode(200, new { token = result, role = role });
            }
            return StatusCode(400);
        }
    }
}
