using Surgery_1.Data.Context;
using Surgery_1.Data.Entities;
using Surgery_1.Data.ViewModels;
using Surgery_1.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Services.Implementations
{
    public class DrugService : IDrugService
    {
        private readonly AppDbContext _context;
        public DrugService(AppDbContext _context)
        {
            this._context = _context;
        }

        public void ImportDrug(ICollection<DrugViewModel> drugs)
        {
            foreach (var d in drugs)
            {
                var drug = new Drug()
                {
                    DateCreated = DateTime.Now,
                    IsDeleted = false,
                    DrugName = d.Name,
                    Unit = d.Unit
                };
                _context.Drugs.Add(drug);
                
            }
            _context.SaveChanges();
        }
    }
}
