using Surgery_1.Data.Context;
using Surgery_1.Data.Entities;
using Surgery_1.Data.ViewModels;
using Surgery_1.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public ICollection<DrugViewModel> GetAllDrug()
        {
            var drugs = _context.Drugs;
            var resuts = new List<DrugViewModel>();
            foreach (var drug in drugs)
            {
                resuts.Add(new DrugViewModel()
                {
                    Id = drug.Id,
                    Name = drug.DrugName,
                    Unit = drug.Unit
                });
            }
            return resuts;
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

        public ICollection<DrugViewModel> SearchDrugOnQuery(string query)
        {
            var drugs = _context.Drugs.Where(a => a.DrugName.Contains(query)).OrderBy(a => a.DrugName).Take(10).ToList();
            var resuts = new List<DrugViewModel>();
            foreach (var drug in drugs)
            {
                resuts.Add(new DrugViewModel()
                {
                    Id = drug.Id,
                    Name = drug.DrugName,
                    Unit = drug.Unit
                });
            }
            return resuts;
        }
    }
}
