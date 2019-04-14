using Surgery_1.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Surgery_1.Data.ViewModels.PostOpSurgeryShiftViewModel;

namespace Surgery_1.Services.Interfaces
{
    public interface ISurgeryShiftService
    {
        bool ImportSurgeryShift(ICollection<ImportSurgeryShiftViewModel> surgeryShifts);
        bool ImportSurgeryShiftMedicalSupply(ICollection<ImportMedicalSupplyViewModel> medicalSupply);
        ICollection<SurgeryCatalogNamesViewModel> GetSurgeryName(ICollection<SurgeryCatalogIDsViewModel> id);
        bool AddMedicalSupply(ShiftMedicalSuppliesViewModel medicalSupply);
        ICollection<MedicalSupplyInfoViewModel> GetSuppliesUsedInSurgery(int surgeryShiftId);
        ICollection<EkipMemberViewModel> GetEkipMember(int surgeryShiftId);
        bool UpdateMedicalSupply(ICollection<ShiftMedicalSupplyViewModel> medicalSupply);
        bool AssignTechnicalStaff();

        //update information emergency
        bool UpdateSurgeryProfile(EditSurgeryShiftViewModel editForm);
        EditSurgeryShiftViewModel LoadEditSurgeryProfile(int shitId);
    }
}
