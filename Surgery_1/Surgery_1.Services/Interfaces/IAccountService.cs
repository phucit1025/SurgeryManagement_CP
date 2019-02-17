using Surgery_1.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Services.Interfaces
{
    public interface IAccountService
    {
        string Login(LoginViewModel loginViewModel);
    }
}
