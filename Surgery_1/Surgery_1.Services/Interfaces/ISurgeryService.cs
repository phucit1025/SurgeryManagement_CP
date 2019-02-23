using Surgery_1.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Services.Interfaces
{
    public interface ISurgeryService
    {
        void MakeScheduleList();
        void MakeSchedule(ScheduleViewModel scheduleViewModel);
        //Lấy phòng có thời gian phẫu thuật trễ nhất (EndStart)
        RoomDateViewModel GetRoomByMaxSurgeryTime(ScheduleViewModel scheduleViewModel);
        void InsertFileToSurgeryShift(ScheduleViewModel scheduleViewModel);
        ICollection<SurgeryRoomViewModel> GetSurgeryRooms();
        ICollection<SurgeryShiftViewModel> GetSurgeryShiftsByRoomAndDate(int surgeryRoomId, int dateNumber);

        // Lấy nhưng ca mổ cần lên lịch theo ngày
        ICollection<ScheduleViewModel> GetSurgeryShiftsNoSchedule(int dateNumber);

        RoomDateViewModel GetRoomByMax(int dayNumber); 
        SurgeryShiftDetailViewModel GetShiftDetail(int shiftId);
    }
}
