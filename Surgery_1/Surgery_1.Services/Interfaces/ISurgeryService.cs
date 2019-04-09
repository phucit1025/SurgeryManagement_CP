using Surgery_1.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Services.Interfaces
{
    public interface ISurgeryService
    {
        #region Tung
        //Making schedule
        List<AvailableRoomViewModel> GetAvailableSlotRoom(int dateNumber);
        ICollection<SurgeryRoomViewModel> GetSurgeryRooms();
        ICollection<SurgeryShiftViewModel> GetSurgeryShiftsByRoomAndDate(int surgeryRoomId, int dateNumber);
        bool MakeScheduleList();
        ICollection<SurgeryRoomViewModel> GetSlotRooms();
        bool AddEmergencyShift(EmerSurgeryShift emerShift);
        int GetAvailableRoomForProposedTime(EmerSurgeryShift emerShift);
        bool RefreshSurgeryShift(int shiftId);
        bool CheckStatusPreviousSurgeryShift(int shiftId);


        // Lấy những ca mổ cần lên lịch theo ngày
        ICollection<ScheduleViewModel> GetSurgeryShiftsNoSchedule();
        ICollection<ScheduleViewModel> GetSurgeryShiftNoScheduleByProposedTime();
        #endregion

        SurgeryShiftDetailViewModel GetShiftDetail(int shiftId);

        Boolean SaveSurgeryProcedure(SurgeryProcedureViewModel SurgeryProcedure);

        #region Change Surgery Business
        bool ChangeFirstPriority(ShiftChangeViewModel newShift);
        bool ChangeSchedule(ShiftScheduleChangeViewModel newShift);
        List<int> GetAvailableRoom(DateTime start, DateTime end, bool forcedChange);
        List<AvailableRoomViewModel> GetAvailableRoom(int hour, int minute, int? longerShiftId = null, List<int> shiftIds = null);
        bool ChangeShiftStatus(ShiftStatusChangeViewModel currentShift);
        SwapShiftResultViewModel SwapShift(int shift1Id, int shift2Id);
        List<int> GetSwapableShiftIds();
        SwapShiftResultViewModel SwapShiftToRoom(int shiftId, int roomId, bool forcedSwap);
        #endregion

        void AssignEkipByDate(int dateNumber);
        void AssignEkip();
    }
}
