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

        ICollection<SurgeryRoomViewModel> GetSurgeryRooms();
        ICollection<SurgeryShiftViewModel> GetSurgeryShiftsByRoomAndDate(int surgeryRoomId, int dateNumber);

        // Lấy những ca mổ cần lên lịch theo ngày
        ICollection<ScheduleViewModel> GetSurgeryShiftsNoSchedule();
        ICollection<ScheduleViewModel> GetSurgeryShiftNoScheduleByProposedTime();

        SurgeryShiftDetailViewModel GetShiftDetail(int shiftId);

        #region Change Surgery Business
        bool ChangeFirstPriority(ShiftChangeViewModel newShift);
        bool ChangeSchedule(ShiftScheduleChangeViewModel newShift);
        List<int> GetAvailableRoom(DateTime start, DateTime end);
        List<AvailableRoomViewModel> GetAvailableRoom(int hour, int minute);
        bool ChangeShiftStatus(ShiftStatusChangeViewModel currentShift);
        #endregion
    }
}
