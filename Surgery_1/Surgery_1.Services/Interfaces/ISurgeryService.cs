﻿using Surgery_1.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Services.Interfaces
{
    public interface ISurgeryService
    {
        #region Tung
        List<AvailableRoomViewModel> GetAvailableSlotRoom(int dateNumber);
        ICollection<SurgeryRoomViewModel> GetSurgeryRooms();
        ICollection<SurgeryShiftViewModel> GetSurgeryShiftsByRoomAndDate(int surgeryRoomId, int dateNumber);
        StringBuilder MakeScheduleList();

        //After make schedule
        bool SetPostoperativeStatus(int shiftId, string roomPost, string postBed, string actualEndDateTime);
        bool SetIntraoperativeStatus(int shiftId, string actualStartDateTime);
        bool SetFinishedStatus(int shiftId);
        int CheckPostStatus(int shiftId);
        bool CheckRecoveryStatus(int shiftId);


        // Lấy những ca mổ cần lên lịch theo ngày
        ICollection<ScheduleViewModel> GetSurgeryShiftsNoSchedule();
        ICollection<ScheduleViewModel> GetSurgeryShiftNoScheduleByProposedTime();
        #endregion

        SurgeryShiftDetailViewModel GetShiftDetail(int shiftId);

        #region Change Surgery Business
        bool ChangeFirstPriority(ShiftChangeViewModel newShift);
        bool ChangeSchedule(ShiftScheduleChangeViewModel newShift);
        List<int> GetAvailableRoom(DateTime start, DateTime end, bool forcedChange);
        List<AvailableRoomViewModel> GetAvailableRoom(int hour, int minute, int? longerShiftId = null, List<int> shiftIds = null);
        bool ChangeShiftStatus(ShiftStatusChangeViewModel currentShift);
        SwapShiftResultViewModel SwapShift(int shift1Id, int shift2Id);
        List<int> GetSwapableShiftIds();
        #endregion
    }
}
