using Surgery_1.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Services.Interfaces
{
    public interface ISurgeryService
    {
        void MakeSchedule(ScheduleViewModel scheduleViewModel);
        //Lấy phòng có thời gian phẫu thuật trễ nhất (EndStart)
        string GetRoomByMaxSurgeryTime(ScheduleViewModel scheduleViewModel);
        void InsertFileToSurgeryShift(ScheduleViewModel scheduleViewModel);
    }
}
