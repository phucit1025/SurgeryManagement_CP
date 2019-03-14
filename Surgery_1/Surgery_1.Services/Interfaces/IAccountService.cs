using Surgery_1.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surgery_1.Services.Interfaces
{
    public interface IAccountService
    {
        string Login(LoginViewModel loginViewModel);
        Task<string> Authenticate(string email, string password);
        Task<string> GetRoleName(string email);
    }
}
