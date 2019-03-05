using Surgery_1.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Services.Interfaces
{
    public interface IDrugService
    {
        void ImportDrug(ICollection<DrugViewModel> surgeryShift);
        ICollection<DrugViewModel> GetAllDrug();
    }
}
