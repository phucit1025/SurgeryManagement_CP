using Surgery_1.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Services.Interfaces
{
    public interface IMedicalSupplyConfirmService
    {
        void InsertSupplyRequest(ICollection<MedicalSupplyDetailImportViewModel> list);
        bool ConfirmedSupply(int surgeryShiftId);
        ICollection<MedicalSupplyRequestViewModel> GetAllMedicalSupplyRequest();
        ICollection<MedicalSupplyRequestDetailViewModel> GetMedicalSupplyRequestDetail(int surgeryShitfId);
    }
}
