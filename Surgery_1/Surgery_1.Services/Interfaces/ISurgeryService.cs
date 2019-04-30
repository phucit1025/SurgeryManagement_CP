﻿using Surgery_1.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Services.Interfaces
{
    public interface ISurgeryService
    {
        #region Tung
        //Making schedule
        List<AvailableRoomViewModel> GetAvailableSlotRoom(int dateNumber, int surgeryCatalogId);
        ICollection<SurgeryRoomViewModel> GetSlotRooms();
        ICollection<SurgeryRoomViewModel> GetSurgeryRooms();
        ICollection<SurgeryShiftViewModel> GetSurgeryShiftsByRoomAndDate(int surgeryRoomId, int dateNumber, int techincalStaffId = 0);

        bool MakeScheduleList();
        bool AddEmergencyShift(EmerSurgeryShift emerShift);
        bool RefreshSurgeryShift(int shiftId);
        bool CheckStatusPreviousSurgeryShift(int shiftId);
        bool CanViewShiftDetail(int shiftId, string techGuid = "");


        // Lấy những ca mổ cần lên lịch theo ngày
        ICollection<ScheduleViewModel> GetSurgeryShiftsNoSchedule();
        ICollection<ScheduleViewModel> GetSurgeryShiftNoScheduleByProposedTime();
        #endregion

        SurgeryShiftDetailViewModel GetShiftDetail(int shiftId);

        bool SaveSurgeryProcedure(SurgeryProcedureViewModel SurgeryProcedure);

        #region Change Surgery Business
        bool ChangeFirstPriority(ShiftChangeViewModel newShift);
        bool ChangeSchedule(ShiftScheduleChangeViewModel newShift);
        List<int> GetAvailableRoom(DateTime start, DateTime end, bool forcedChange, int specialityGroupId = 0);
        List<AvailableRoomViewModel> GetAvailableRoom(DateTime? date, int specialtyGroupId, int hour, int minute, int? longerShiftId = null, List<int> shiftIds = null);
        bool ChangeShiftStatus(ShiftStatusChangeViewModel currentShift);
        SwapShiftResultViewModel SwapShift(int shift1Id, int shift2Id);
        List<int> GetSwapableShiftIds();
        SwapShiftResultViewModel SwapShiftToRoom(int shiftId, int roomId, bool forcedSwap);
        List<AffectedShiftViewModel> GetAffectedShifts(DateTime start, DateTime end, int specialtyGroupId);
        #endregion

        void AssignEkipByDate(int dateNumber);
        void AssignEkip();

        List<StatisticViewModel> numShiftBySpec(DateTime start, DateTime end);
        List<RoomStatisticViewModel> getEfficientcyRoom(DateTime start, DateTime end);
    }
}
