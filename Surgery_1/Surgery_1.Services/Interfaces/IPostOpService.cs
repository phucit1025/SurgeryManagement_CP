using Surgery_1.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Services.Interfaces
{
    public interface IPostOpService
    {
        ICollection<PostOpSurgeryShiftViewModel> GetSurgeryByStatusId(int statusId);
        RecoverySurgeryShiftViewModel GetRecoverySurgeryShiftById(int id);
        object GetHealthCareRerportBySurgeryShiftId(int surgeryShiftId);
        bool ChangeSurgeryShiftToRecovery(int surgeryShiftId, string postOpRoom, string postOpBed);
        bool CreateHealthCareReport(HealthCareReportViewModel healthCareRerpotViewModel);
        bool UpdateHealthCareReport(HealthCareReportViewModel healthCareRerpotViewModel);
        bool SoftDeleteHealthCareReport(int healthCareReportId);
        ICollection<PostOpSurgeryShiftViewModel> FindPostOpSurgeryByPatientName(string name);
        bool EditRoomBedSurgeryShift(int surgeryShiftId, string room, string bed);
    }
}
