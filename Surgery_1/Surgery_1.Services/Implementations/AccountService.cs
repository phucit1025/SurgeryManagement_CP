using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Surgery_1.Data.Context;
using Surgery_1.Data.Entities;
using Surgery_1.Data.ViewModels;
using Surgery_1.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Surgery_1.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;

        public AccountService(AppDbContext _context, UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            this._context = _context;
            _userManager = userManager;
            _configuration = configuration;
        }
        public string Login(LoginViewModel loginViewModel)
        {

            return loginViewModel.Username;
        }

        public async Task<string> Authenticate(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var hasRightPassword = await _userManager.CheckPasswordAsync(user, password);

                if (hasRightPassword)
                {
                    var roleList = await _userManager.GetRolesAsync(user);
                    var userRole = roleList.FirstOrDefault();

                    #region Set Claims
                    var claims = new List<Claim>() {
                        new Claim(JwtRegisteredClaimNames.Email, user.Email),
                        new Claim(new ClaimsIdentityOptions().UserIdClaimType, user.Id),
                    };
                    claims.Add(new Claim(ClaimTypes.Role, userRole));
                    claims.Add(new Claim(new ClaimsIdentityOptions().SecurityStampClaimType, await _userManager.GetSecurityStampAsync(user)));
                    claims.Add(new Claim("IsEmailConfirmed", user.EmailConfirmed.ToString()));
                    #endregion

                    return BuildJwtToken(claims);
                }
            }

            return null;
        }

        public async Task<string> GetRoleName(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var roles = await _userManager.GetRolesAsync(user);
            return roles.FirstOrDefault();
        }

        private string BuildJwtToken(List<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
              _configuration["Jwt:Issuer"],
              claims,
              expires: DateTime.Now.AddMinutes(43200),
              signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
