using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Surgery_1.Services.Extensions
{
    public static class TokenExtensions
    {
        public static string GetGuid(this ClaimsPrincipal user)
        {
            if (user.HasClaim(c => c.Type.Equals(new ClaimsIdentityOptions().UserIdClaimType)))
            {
                return user.Claims.FirstOrDefault(c => c.Type.Equals(new ClaimsIdentityOptions().UserIdClaimType)).Value;
            }
            return null;
        }
    }
}
