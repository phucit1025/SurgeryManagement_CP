using Surgery_1.Data.Context;
using Surgery_1.Data.Entities;
using Surgery_1.Data.ViewModels;
using Surgery_1.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Services.Implementations
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _context;
        public AccountService(AppDbContext _context)
        {
            this._context = _context;
        }
        public string Login(LoginViewModel loginViewModel)
        {
            return loginViewModel.Username;
        }
    }
}
