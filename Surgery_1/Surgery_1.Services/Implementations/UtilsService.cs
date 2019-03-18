using Surgery_1.Data.Context;
using Surgery_1.Data.ViewModels;
using Surgery_1.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Surgery_1.Services.Implementations
{
    public class UtilsService : IUtilsService
    {
        private static List<MedicalSupplyInfoViewModel> list = null;

        private readonly AppDbContext _context;

        public UtilsService(AppDbContext _context)
        {
            this._context = _context;
        }

        public ICollection<MedicalSupplyInfoViewModel> GetMedicalSupply()
        {
            if (list == null)
            {
                list = new List<MedicalSupplyInfoViewModel>();
                var supplies = _context.MedicalSupplies.ToList();

                foreach (var supply in supplies)
                {
                    var ms = new MedicalSupplyInfoViewModel();
                    ms.medicalSupplyId = supply.Id;
                    ms.medicalSupplyName = supply.Name;
                    list.Add(ms);
                }
            }
            return list;
        }
    }
}
