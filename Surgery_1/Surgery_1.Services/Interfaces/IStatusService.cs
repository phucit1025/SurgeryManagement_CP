using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Services.Interfaces
{
    public interface IStatusService
    {
        //After make schedule
        bool SetPostoperativeStatus(int shiftId, string roomPost, string postBed, string actualEndDateTime);
        bool SetIntraoperativeStatus(int shiftId, string actualStartDateTime);
        bool SetFinishedStatus(int shiftId);
        string GetStatusByShiftId(int shiftId);
    }
}
