using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Services.Interfaces
{
    public interface IMedicalSupplyConfirm
    {
        void InsertSupplyRequest(int supplyId, int surgeryShiftId);
        bool ConfirmedSupply(int surgeryShiftId);
        //TODO get request detail
    }
}
