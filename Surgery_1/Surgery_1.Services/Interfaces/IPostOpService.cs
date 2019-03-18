using Surgery_1.Data.Entities;
using Surgery_1.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surgery_1.Services.Interfaces
{
    public interface IPostOpService
    {
        ICollection<PostOpSurgeryShiftViewModel> GetSurgeryByStatusId(int statusId);
        RecoverySurgeryShiftViewModel GetRecoverySurgeryShiftById(int id);
        ICollection<HealthCareReportViewModel> GetHealthCareRerportBySurgeryShiftId(int surgeryShiftId);
        bool ChangeSurgeryShiftToRecovery(int surgeryShiftId, string postOpRoom, string postOpBed);
        bool CreateHealthCareReport(HealthCareReportViewModel healthCareRerpotViewModel);
        bool UpdateHealthCareReport(HealthCareReportViewModel healthCareRerpotViewModel);
        bool SoftDeleteHealthCareReport(int healthCareReportId);
        ICollection<PostOpSurgeryShiftViewModel> FindPostOpSurgeryByQuery(string name);
        bool EditRoomBedSurgeryShift(int surgeryShiftId, string room, string bed);
        bool CreateTreatmenReport(TreatmentReportViewModel treatmentReportViewModel);
        ICollection<TreatmentReportViewModel> GetTreatmentReportByShiftId(int shiftId);
        TreatmentReportViewModel GetTreatmentReportById(int id);
        bool EditTreatmentReport(TreatmentReportViewModel treatmentReportViewModel);
        ICollection<TreatmentReportViewModel> GetTodayTreatmentReportByShiftId(int shiftId);
        bool CreateTreatmentReportDrugs(ICollection<TreatmentReportDrugViewModel> treatmentReportDrugViewModels);
        TreatmentMedication GetDrugRequirementForNurse(int shiftId);
        Task<bool> AssignNurse(int shiftId, int nurseId);
        Task<ICollection<NurseViewModel>> GetAllNurse();
        NurseViewModel GetNurseByShiftId(int shiftId);

    }
}
