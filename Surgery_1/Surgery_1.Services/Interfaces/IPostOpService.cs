using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Services.Interfaces
{
    public interface IPostOpService
    {
        object GetSurgeryByStatusId(int statusId);
        object GetHealthCareRerportBySurgeryShiftId(int surgeryShiftId);

    }
}
