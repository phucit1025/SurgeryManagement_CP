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
        bool AddMedicalSupply(ShiftMedicalSuppliesViewModel medicalSupply);
        ICollection<MedicalSupplyInfoViewModel> GetSuppliesUsedInSurgery(int surgeryShiftId);
        ICollection<EkipMemberViewModel> GetEkipMember(int surgeryShiftId);
        bool AssignTechnicalStaff();

        ICollection<SurgeonsViewModel> GetAvailableSurgeons(int surgeryShiftId);
        bool UpdateSurgeon(UpdateSurgeonsViewModel updatedSurgeon);
<<<<<<< HEAD
=======
        bool AddSurgeon(AddSurgeonToShiftViewModel model);
        bool RemoveSurgeon(RemoveSurgeonFromShiftViewModel model);
>>>>>>> deac6e0931876be808b371add2d4645d13aece8e

        //update information emergency
        bool UpdateSurgeryProfile(EditSurgeryShiftViewModel editForm);
        EditSurgeryShiftViewModel LoadEditSurgeryProfile(int shitId);
        ICollection<SelectedSurgeryCatalogViewModel> GetSurgeryCatalogOnQuery(string searchName);
    }
}
