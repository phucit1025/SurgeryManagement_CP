using Surgery_1.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using static Surgery_1.Data.ViewModels.PostOpSurgeryShiftViewModel;

namespace Surgery_1.Services.Interfaces
{
    public interface ISurgeryShiftService
    {
        void ImportSurgeryShift(ICollection<ImportSurgeryShiftViewModel> surgeryShift);
        void ImportSurgeryShiftMedicalSupply(ICollection<ImportMedicalSupplyViewModel> medicalSupply);
        ICollection<SurgeryCatalogNamesViewModel> GetSurgeryName(ICollection<SurgeryCatalogIDsViewModel> id);
        void AddMedicalSupply(ICollection<AddMedicalSupplyViewModel> medicalSupply);
        ICollection<MedicalSupplyInfoViewModel> GetSuppliesUsedInSurgery(int surgeryShiftId);
    }
}
