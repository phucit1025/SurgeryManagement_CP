using Castle.Core.Internal;
using Microsoft.EntityFrameworkCore;
using Surgery_1.Data.Context;
using Surgery_1.Data.Entities;
using Surgery_1.Data.ViewModels;
using Surgery_1.Repositories.Interfaces;
using Surgery_1.Services.Enums;
using Surgery_1.Services.Interfaces;
using Surgery_1.Services.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Surgery_1.Services.Implementations
{
    public class SurgeryService : ISurgeryService
    {
        TimeSpan startAMWorkingHour = TimeSpan.FromHours(ConstantVariable.StartAMWorkingHour);
        TimeSpan endAMWorkingHour = TimeSpan.FromHours(ConstantVariable.EndAMWorkingHour);
        TimeSpan startPMWorkingHour = TimeSpan.FromHours(ConstantVariable.StartPMWorkingHour);
        TimeSpan endPMWorkingHour = TimeSpan.FromHours(ConstantVariable.EndPMWorkingHour);

        private int countNoti = 0;


        private List<SmsShiftViewModel> smsShiftDate = new List<SmsShiftViewModel>();

        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public SurgeryService(AppDbContext _context, INotificationService _notificationService)
        {
            this._context = _context;
            this._notificationService = _notificationService;
        }

        public bool CheckStatusPreviousSurgeryShift(int shiftId)
        {
            var surgeryShift = _context.SurgeryShifts.Find(shiftId);
            var slotRoom = _context.SlotRooms.Find(surgeryShift.SlotRoomId);
            var previousShift = slotRoom.SurgeryShifts
                .Where(s => s.EstimatedStartDateTime.Value.Date == surgeryShift.EstimatedStartDateTime.Value.Date)
                .Where(s => s.EstimatedEndDateTime <= surgeryShift.EstimatedStartDateTime).OrderByDescending(s => s.EstimatedEndDateTime)
                .FirstOrDefault();

            if (previousShift != null)
            {
                var statusName = previousShift.Status.Name;
                if (!statusName.Equals(ConstantVariable.PRE_STATUS) && !statusName.Equals(ConstantVariable.INTRA_STATUS))
                {
                    return true;// cho hiện
                }
            }
            else
            { // Không có thằng nào trước nó, tức nó đứng đầu
                return true;
            }
            return false;
        }

        public bool RefreshSurgeryShift(int shiftId)
        {
            var surgeryShift = _context.SurgeryShifts.Find(shiftId);
            int slotRoomId = surgeryShift.SlotRoomId.Value;
            var slotRoom = _context.SlotRooms.Find(slotRoomId);
            var shiftInRoom = slotRoom.SurgeryShifts
                .Where(s => s.Id != shiftId)
                .Where(s => s.ScheduleDate.Value == surgeryShift.ScheduleDate.Value)
                .Where(s => s.EstimatedStartDateTime >= surgeryShift.EstimatedStartDateTime // .Where(s => s.EstimatedStartDateTime >= surgeryShift.EstimatedEndDateTime
                || s.EstimatedEndDateTime > surgeryShift.EstimatedStartDateTime)
                .OrderBy(s => s.EstimatedStartDateTime).ToList();
            if (shiftInRoom.Count >= 1) // 2 trở lên mới có ảnh hưởng tới những thằng sau
            {
                try
                {
                    var changeShift = _context.SurgeryShifts.Find(shiftInRoom.First().Id);
                    changeShift.EstimatedStartDateTime = surgeryShift.ActualEndDateTime.HasValue ? surgeryShift.ActualEndDateTime : surgeryShift.EstimatedEndDateTime;
                    changeShift.EstimatedEndDateTime = changeShift.EstimatedStartDateTime + TimeSpan.FromHours(changeShift.ExpectedSurgeryDuration);
                    _context.SaveChanges();
                    if (shiftInRoom.Count >= 2)
                    {
                        for (int i = 0; i < shiftInRoom.Count - 1; i++)
                        {
                            var realShift = _context.SurgeryShifts.Find(shiftInRoom.ElementAt(i).Id);
                            if (realShift.EstimatedEndDateTime > shiftInRoom.ElementAt(i + 1).EstimatedStartDateTime)
                            {
                                changeShift = _context.SurgeryShifts.Find(shiftInRoom.ElementAt(i + 1).Id);
                                changeShift.EstimatedStartDateTime = realShift.EstimatedEndDateTime;
                                changeShift.EstimatedEndDateTime = changeShift.EstimatedStartDateTime + TimeSpan.FromHours(changeShift.ExpectedSurgeryDuration);
                                _context.SaveChanges();
                            }

                        }
                    }
                }
                catch (DbUpdateException)
                {
                    return false;
                }
            }
            return true;
        }

        public bool CanViewShiftDetail(int shiftId, string techGuid = "")
        {
            if (techGuid.IsNullOrEmpty())
            {
                return true;
            }
            else
            {
                var techId = _context.UserInfo.FirstOrDefault(ui => ui.GuId.Equals(techGuid)).Id;
                var shift = _context.SurgeryShifts.Find(shiftId);
                if (shift.TechId == techId) return true;
                return false;
            }
        }

        #region Make Schedule
        public bool MakeScheduleList()
        {
            var shifts = GetSurgeryShiftsNoSchedule();
            //Notification
            int tmpCount = shifts.Count;
            if (tmpCount > countNoti) { countNoti = tmpCount; }

            foreach (var shift in shifts)
            {
                int dayNumber = UtilitiesDate.ConvertDateToNumber(shift.ScheduleDate);
                var availableSlotRooms = new List<AvailableRoomViewModel>();
                // TODO: Lấy list phòng trống (chưa có ca phẫu thuật nào trong ngày)
                int roomEmptyId = GetEmptyRoomForDate(dayNumber, shift.SurgeryCatalogId);

                if (roomEmptyId != 0) // Có phòng trống
                {
                    if (shift.IsNormalSurgeryTime) // mổ bình thường
                    {
                        var timeConfirm = shift.ConfirmDate.TimeOfDay; //Time confirm
                        var dateConfirm = shift.ConfirmDate.Date; //Time confirm
                        var duration = TimeSpan.FromHours(shift.ExpectedSurgeryDuration);
                        if (dateConfirm == shift.ScheduleDate)
                        {
                            if (timeConfirm < startAMWorkingHour) // Confirm lúc 6h sáng -> 7h lên lịch
                            {
                                DateTime startEstimatedTime = shift.ScheduleDate + startAMWorkingHour;
                                DateTime endEstimatedTime = startEstimatedTime + duration;
                                InsertDateTimeToSurgeryShift(shift.SurgeryShiftId, startEstimatedTime, endEstimatedTime, roomEmptyId);
                            }
                            else if (timeConfirm + duration <= endPMWorkingHour)
                            {

                                var endTmpTime = timeConfirm + duration;
                                if (endTmpTime <= endAMWorkingHour || timeConfirm >= startPMWorkingHour) //buổi sáng/ chiều
                                {
                                    DateTime startEstimatedTime = shift.ScheduleDate + timeConfirm;
                                    DateTime endEstimatedTime = startEstimatedTime + duration;
                                    InsertDateTimeToSurgeryShift(shift.SurgeryShiftId, startEstimatedTime, endEstimatedTime, roomEmptyId);
                                }
                                if ((timeConfirm >= endAMWorkingHour && timeConfirm < startPMWorkingHour) //buổi trưa trở đi
                                    || (endTmpTime > endAMWorkingHour && endTmpTime <= startPMWorkingHour))
                                {
                                    DateTime startEstimatedTime = shift.ScheduleDate + startPMWorkingHour;
                                    DateTime endEstimatedTime = startEstimatedTime + duration;
                                    InsertDateTimeToSurgeryShift(shift.SurgeryShiftId, startEstimatedTime, endEstimatedTime, roomEmptyId);
                                }
                            }
                        }
                        if (dateConfirm < shift.ScheduleDate)
                        {
                            DateTime startEstimatedTime = shift.ScheduleDate + startAMWorkingHour;
                            DateTime endEstimatedTime = startEstimatedTime + TimeSpan.FromHours(shift.ExpectedSurgeryDuration);
                            InsertDateTimeToSurgeryShift(shift.SurgeryShiftId, startEstimatedTime, endEstimatedTime, roomEmptyId);
                        }
                    }
                    else //mổ chỉ định
                    {
                        InsertDateTimeToSurgeryShift
                               (shift.SurgeryShiftId, shift.ProposedStartDateTime.Value, shift.ProposedEndDateTime.Value, roomEmptyId);
                    }
                }
                else // hết phòng trống
                {
                    availableSlotRooms = GetAvailableSlotRoom(dayNumber, shift.SurgeryCatalogId);

                    // Lấy khoảng thời gian sau thời gian confirm
                    availableSlotRooms = availableSlotRooms.Where(s => s.StartDateTime > shift.ConfirmDate || shift.ConfirmDate < s.EndDateTime).ToList();
                    for (int index = 0; index < availableSlotRooms.Count; index++)
                    {
                        if (availableSlotRooms.ElementAt(index).StartDateTime < shift.ConfirmDate)
                        {
                            availableSlotRooms.ElementAt(index).StartDateTime = shift.ConfirmDate;
                        }
                    }

                    if (shift.IsNormalSurgeryTime)
                    {
                        // Sắp xếp theo khoảng t.g phẫu thuật hợp lý và thời gian sớm nhất
                        var room = availableSlotRooms.Where(s => s.ExpectedSurgeryDuration >= shift.ExpectedSurgeryDuration)
                            .OrderBy(s => s.ExpectedSurgeryDuration)
                            .OrderBy(s => s.StartDateTime)
                            .FirstOrDefault();
                        if (room != null)
                        {
                            var endTime = room.StartDateTime + TimeSpan.FromHours(shift.ExpectedSurgeryDuration);
                            InsertDateTimeToSurgeryShift(shift.SurgeryShiftId, room.StartDateTime, endTime, room.RoomId);
                        }
                    }
                    else // Mổ chỉ định
                    {
                        // Sắp xếp theo thời gian tiệm cận với khoảng chỉ định nhất, 
                        var room = availableSlotRooms.Where(s => s.StartDateTime <= shift.ProposedStartDateTime
                                && s.EndDateTime >= shift.ProposedEndDateTime)
                               .OrderBy(s => s.ExpectedSurgeryDuration)
                               .OrderBy(s => s.EndDateTime)
                               .OrderByDescending(s => s.StartDateTime)
                               .FirstOrDefault();
                        if (room != null)
                        {
                            InsertDateTimeToSurgeryShift
                                (shift.SurgeryShiftId, shift.ProposedStartDateTime.Value, shift.ProposedEndDateTime.Value, room.RoomId);
                        }
                        else //Khoảng thời gian chỉ định ko hợp lý thì vào trong này
                        {
                            int roomProposed = GetAvailableRoomForProposedTime(
                                new EmerSurgeryShift()
                                {
                                    StartTime = shift.ProposedStartDateTime.Value,
                                    EndTime = shift.ProposedEndDateTime.Value
                                }, shift.SurgeryCatalogId
                                );
                            if (roomProposed != 0)
                            {
                                InsertDateTimeToSurgeryShift
                                (shift.SurgeryShiftId, shift.ProposedStartDateTime.Value, shift.ProposedEndDateTime.Value, roomProposed);
                            }
                        }
                    }
                }
            }
            // TODO: =======================Xử lý qua ngày========================
            shifts = GetSurgeryShiftsNoSchedule();
            if (shifts.Count != 0)
            {
                foreach (var shift in shifts)
                {
                    var item = _context.SurgeryShifts.Find(shift.SurgeryShiftId);
                    if (!shift.IsNormalSurgeryTime) //chỉ định chuyển qua bình thường
                    {
                        item.IsNormalSurgeryTime = true;
                    }
                    else
                    {
                        if (shift.ScheduleDate.AddDays(1).DayOfWeek.Equals(ConstantVariable.DAYOFF))
                        {
                            item.ScheduleDate = shift.ScheduleDate.AddDays(2);
                        }
                        else
                        {
                            item.ScheduleDate = shift.ScheduleDate.AddDays(1);
                        }
                    }
                    _context.Update(item);
                    _context.SaveChanges();
                }
                MakeScheduleList();
            }
            //notification

            if (smsShiftDate.Count == countNoti)
            {
                _notificationService.HandleSmsForSurgeon(smsShiftDate);
                _notificationService.AddNotificationForScheduling(smsShiftDate);
                countNoti++;
            }
            _notificationService.HandleSmsForSurgeon(smsShiftDate);
            return true;
        }
        #endregion

        public bool GetTimeSurgeon(int surgeonId, DateTime selectedDay)
        {
            var result = _context.Doctors.Find(surgeonId).SurgeryShifts.Where(s => s.EstimatedStartDateTime.Value.Date == selectedDay); //Tmp

            return false;
        }

        #region GetAvailableRoom
        public List<AvailableRoomViewModel> GetAvailableSlotRoom(int dateNumber, int surgeryCatalogId)
        {
            var specialtyGroupId = _context.SurgeryCatalogs.Find(surgeryCatalogId).Specialty.SpecialtyGroup.Id;
            // TODO: Lấy khoảng thời gian của ConfirmDate, sau khi confirm thì lên lịch ngay
            var slotRooms = _context.SlotRooms.Where(s => s.SurgeryRoom.SpecialtyGroup.Id == specialtyGroupId).ToList();
            var availableRooms = new List<AvailableRoomViewModel>();
            foreach (var slot in slotRooms)
            {
                // TODO: Lấy các ca phẫu thuật theo từng phòng, sắp xếp tăng dần theo EstimatedStartDateTime
                var result = slot.SurgeryShifts
                    .Where(s => UtilitiesDate.ConvertDateToNumber(s.ScheduleDate.Value) == dateNumber
                    && s.IsAvailableMedicalSupplies == true
                    && s.EstimatedEndDateTime.Value.TimeOfDay > startAMWorkingHour
                    && s.EstimatedStartDateTime.Value.TimeOfDay < endPMWorkingHour)
                    .OrderBy(s => s.EstimatedStartDateTime).ToList();
                if (result.Count > 0)
                {
                    var start = result.First().ScheduleDate.Value + startAMWorkingHour; //7h
                    var end = result.First().EstimatedStartDateTime.Value;
                    //nếu có actual
                    if (result.First().ActualStartDateTime != null)
                    {
                        end = result.First().ActualStartDateTime.Value;
                    }
                    // Lấy khoảng ở ngoài các ca mổ [7h - 17h]
                    if (start != end && end.TimeOfDay >= startAMWorkingHour)
                    { // Lấy khoảng từ 7h ->
                        AddAvailableSlotExceptBreakTime(availableRooms, start, end, slot.Id);
                    }

                    start = result.Last().EstimatedEndDateTime.Value;
                    end = result.Last().ScheduleDate.Value + endPMWorkingHour;//17h
                    //nếu có actual
                    if (result.Last().ActualEndDateTime != null)
                    {
                        start = result.Last().ActualEndDateTime.Value;
                    }
                    if (start != end && start.TimeOfDay <= endPMWorkingHour)
                    { // Lấy khoảng từ -> 17h
                        AddAvailableSlotExceptBreakTime(availableRooms, start, end, slot.Id);

                    }
                    if (result.Count != 1)
                    {
                        //======= Lấy khoảng ở giữa các ca mổ==========
                        for (int i = 0; i < result.Count - 1; i++)
                        {

                            var tmpEnd = result.ElementAt(i).EstimatedEndDateTime;
                            if (result.ElementAt(i).ActualEndDateTime != null)
                            {
                                tmpEnd = result.ElementAt(i).ActualEndDateTime;
                            }
                            var tmpStart = result.ElementAt(i + 1).EstimatedStartDateTime;
                            if (result.ElementAt(i + 1).ActualStartDateTime != null)
                            {
                                tmpStart = result.ElementAt(i + 1).ActualStartDateTime;
                            }
                            if (tmpEnd != tmpStart)
                            {
                                start = tmpEnd.Value;
                                end = tmpStart.Value;
                                AddAvailableSlotExceptBreakTime(availableRooms, start, end, slot.Id);
                            }
                        }
                    }
                }
            }
            return availableRooms.ToList();
        }

        public int GetAvailableRoomForProposedTime(EmerSurgeryShift emerShift, int surgeryCatalogId)
        {
            var specialtyGroupId = 0;
            if (emerShift.IsEmergency)
            {
                specialtyGroupId = _context.SpecialtyGroups.Where(s => s.Name == ConstantVariable.EMERGENCY_GROUP).FirstOrDefault().Id;
            }
            else
            {
                specialtyGroupId = _context.SurgeryCatalogs.Find(surgeryCatalogId).Specialty.SpecialtyGroup.Id;
            }
            var parentSlotRooms = _context.SlotRooms.Where(s => s.SurgeryRoom.SpecialtyGroup.Id == specialtyGroupId).ToList();

            var childRoomIds = new List<int>();
            foreach (var room in parentSlotRooms)
            {
                var roomByDate = room.SurgeryShifts.Where(s => s.ScheduleDate == emerShift.StartTime.Date).ToList();
                foreach (var shift in roomByDate)
                {
                    if ((emerShift.StartTime >= shift.EstimatedStartDateTime && emerShift.StartTime < shift.EstimatedEndDateTime) ||
                        (emerShift.EndTime > shift.EstimatedStartDateTime && emerShift.EndTime <= shift.EstimatedEndDateTime) ||
                        (emerShift.StartTime < shift.EstimatedStartDateTime && emerShift.EndTime > shift.EstimatedEndDateTime))
                    {
                        childRoomIds.Add(shift.SlotRoomId.Value);
                        break;
                    }
                }
            }
            ICollection<int> roomIds = parentSlotRooms.Where(p => !childRoomIds.Contains(p.Id)).Select(s => s.Id).ToList();
            return roomIds.FirstOrDefault();
        }

        public int GetEmptyRoomForDate(int scheduleDateNumber, int surgeryCatalogId)
        {
            var specialtyGroupId = _context.SurgeryCatalogs.Find(surgeryCatalogId).Specialty.SpecialtyGroup.Id;

            var slotRooms = _context.SlotRooms.Where(s => s.SurgeryRoom.SpecialtyGroup.Id == specialtyGroupId).ToList();
            ICollection<int> roomIds = new List<int>();
            foreach (var slot in slotRooms)
            {
                if (!slot.SurgeryShifts.Where(s => UtilitiesDate.ConvertDateToNumber(s.ScheduleDate.Value) == scheduleDateNumber).Any())
                {
                    roomIds.Add(slot.Id);
                }
            }
            if (roomIds.Count == 0)
            {
                return 0;
            }
            return roomIds.First();
        }
        public void AddAvailableSlotExceptBreakTime(List<AvailableRoomViewModel> availableRooms, DateTime start, DateTime end, int roomId)
        {
            if (end.TimeOfDay <= endAMWorkingHour || start.TimeOfDay >= startPMWorkingHour) //Add bình thường
            {
                availableRooms.Add(new AvailableRoomViewModel
                {
                    RoomId = roomId,
                    StartDateTime = start,
                    EndDateTime = end,
                    ExpectedSurgeryDuration = (end - start).TotalHours
                });
            }
            else if (start.TimeOfDay < endAMWorkingHour && end.TimeOfDay > startPMWorkingHour)
            {
                var endAM = start.Date + endAMWorkingHour; //11h
                var startPM = end.Date + startPMWorkingHour; //13h
                availableRooms.Add(new AvailableRoomViewModel
                {
                    RoomId = roomId,
                    StartDateTime = start,
                    EndDateTime = endAM,
                    ExpectedSurgeryDuration = (endAM - start).TotalHours
                });
                availableRooms.Add(new AvailableRoomViewModel
                {
                    RoomId = roomId,
                    StartDateTime = startPM,
                    EndDateTime = end,
                    ExpectedSurgeryDuration = (end - startPM).TotalHours
                });
            }
            else if (start.TimeOfDay >= endAMWorkingHour && start.TimeOfDay <= startPMWorkingHour && end.TimeOfDay > startPMWorkingHour) //Add bình thường
            {
                var startPM = end.Date + startPMWorkingHour; //13h
                availableRooms.Add(new AvailableRoomViewModel
                {
                    RoomId = roomId,
                    StartDateTime = startPM,
                    EndDateTime = end,
                    ExpectedSurgeryDuration = (end - startPM).TotalHours
                });
            }
            else if ((end.TimeOfDay >= endAMWorkingHour && end.TimeOfDay <= startPMWorkingHour) || start.TimeOfDay < endAMWorkingHour) //Add bình thường
            {
                var endAM = start.Date + endAMWorkingHour; //11h
                availableRooms.Add(new AvailableRoomViewModel
                {
                    RoomId = roomId,
                    StartDateTime = start,
                    EndDateTime = endAM,
                    ExpectedSurgeryDuration = (endAM - start).TotalHours
                });
            }
        }
        #endregion

        #region Add Emergency Schedule
        public bool AddEmergencyShift(EmerSurgeryShift emerShift)
        {
            var availableRoomId = GetAvailableRoomForProposedTime(emerShift, 0);
            if (availableRoomId != 0) // Trường hợp còn available room thì force hay không force đều đc
            {
                try
                {
                    var insertedShift = new SurgeryShift();
                    insertedShift.EstimatedStartDateTime = UtilitiesDate.GetDateTimeNoSecond(emerShift.StartTime);
                    insertedShift.EstimatedEndDateTime = UtilitiesDate.GetDateTimeNoSecond(emerShift.EndTime);
                    insertedShift.ExpectedSurgeryDuration = (float)(emerShift.EndTime - emerShift.StartTime).TotalHours;
                    insertedShift.ScheduleDate = emerShift.StartTime.Date;
                    insertedShift.ConfirmDate = DateTime.Now;
                    insertedShift.IsAvailableMedicalSupplies = true;
                    insertedShift.IsNormalSurgeryTime = false;
                    insertedShift.SlotRoomId = availableRoomId;
                    insertedShift.StatusId = _context.Statuses.Where(s => s.Name == ConstantVariable.PRE_STATUS).FirstOrDefault().Id;
                    _context.SurgeryShifts.Add(insertedShift);
                    _context.SaveChanges();
                    return true;
                }
                catch (DbUpdateException)
                {
                    return false;
                }
            }
            else
            {   // Hết slot, add force
                if (emerShift.IsForceAdd)
                {
                    try
                    {
                        var slotRoomId = 0;
                        if (emerShift.SlotRoomId.HasValue) // Có slot room chỉ định thì lấy
                        {
                            slotRoomId = emerShift.SlotRoomId.Value;
                        }
                        else
                        {
                            List<ShiftSlotRoomViewModel> slotList = new List<ShiftSlotRoomViewModel>();
                            foreach (var slot in _context.SlotRooms)
                            {
                                var count = slot.SurgeryShifts.Where(s => s.ScheduleDate == emerShift.StartTime.Date
                                && s.EstimatedEndDateTime > emerShift.StartTime).Count();
                                slotList.Add(new ShiftSlotRoomViewModel
                                {
                                    SlotId = slot.Id,
                                    NumberOfShift = count
                                });
                            }
                            slotRoomId = slotList.OrderBy(s => s.NumberOfShift).FirstOrDefault().SlotId;
                        }

                        var insertedShift = new SurgeryShift();
                        insertedShift.EstimatedStartDateTime = emerShift.StartTime;
                        insertedShift.EstimatedEndDateTime = emerShift.EndTime;
                        insertedShift.ExpectedSurgeryDuration = (float)(emerShift.EndTime - emerShift.StartTime).TotalHours;
                        insertedShift.ScheduleDate = emerShift.StartTime.Date;
                        insertedShift.ConfirmDate = DateTime.Now;
                        insertedShift.IsAvailableMedicalSupplies = true;
                        insertedShift.SlotRoomId = slotRoomId;
                        insertedShift.StatusId = _context.Statuses.Where(s => s.Name == ConstantVariable.PRE_STATUS).FirstOrDefault().Id;
                        _context.SurgeryShifts.Add(insertedShift);
                        _context.SaveChanges();
                        // Lấy thằng shift vừa add
                        int createdShiftId = _context.SlotRooms.Find(slotRoomId)
                            .SurgeryShifts.Where(s => s.EstimatedStartDateTime == insertedShift.EstimatedStartDateTime).Select(s => s.Id).FirstOrDefault();
                        RefreshSurgeryShift(createdShiftId);
                        return true;
                    }
                    catch (DbUpdateException)
                    {
                        return false;
                    }
                }
            }
            return false;
        }
        public void InsertDateTimeToSurgeryShift(int surgeryId, DateTime startTime, DateTime endTime, int slotRoomId)
        {
            var surgeryShift = _context.SurgeryShifts.Find(surgeryId);
            surgeryShift.EstimatedStartDateTime = startTime;
            surgeryShift.EstimatedEndDateTime = endTime;
            surgeryShift.SlotRoomId = slotRoomId;
            //Notification
            if (_context.SaveChanges() > 0)
            {
                smsShiftDate.Add(new SmsShiftViewModel
                {
                    Id = surgeryId,
                    EstimatedStartDateTime = startTime,
                    EstimatedEndDateTime = endTime,
                    SlotRoomId = slotRoomId,
                    SurgeonPhone = surgeryShift.TreatmentDoctor.PhoneNumber
                });

                //datetimeShiftList.Add(startTime);
            }


        }
        #endregion


        // TODO: Lấy các phòng phẫu thuật 
        public ICollection<SurgeryRoomViewModel> GetSurgeryRooms()
        {
            var results = new List<SurgeryRoomViewModel>();
            foreach (var room in _context.SurgeryRooms)
            {

                results.Add(new SurgeryRoomViewModel()
                {
                    Id = room.Id,
                    Name = room.Name,
                    SpecialtyGroupId = room.SpecialtyGroupId.Value,
                    SpecialtyGroupName = room.SpecialtyGroup.Name,
                    SlotRooms = room.SlotRooms.Select(sr => new SlotRoomViewModel()
                    {
                        Id = sr.Id,
                        Name = sr.Name
                    }).ToList()
                });
            }
            return results;
        }
        // Lấy slot của room
        public ICollection<SurgeryRoomViewModel> GetSlotRooms()
        {
            var roomList = new List<SurgeryRoomViewModel>();
            List<SlotRoomViewModel> slotRoomList = null;
            foreach (var room in _context.SurgeryRooms)
            {
                slotRoomList = new List<SlotRoomViewModel>();
                foreach (var slotRoom in room.SlotRooms)
                {
                    slotRoomList.Add(new SlotRoomViewModel()
                    {
                        Id = slotRoom.Id,
                        Name = slotRoom.Name
                    });
                }
                roomList.Add(new SurgeryRoomViewModel()
                {
                    Id = room.Id,
                    Name = room.Name,
                    SlotRooms = slotRoomList
                });
            }
            return roomList.ToList();
        }

        // TODO: Xem lịch theo ngày
        public ICollection<SurgeryShiftViewModel> GetSurgeryShiftsByRoomAndDate(int slotRoomId, int dateNumber, int technicalStaffId)
        {
            var results = new List<SurgeryShiftViewModel>();
            var shiftSlotRooms = _context.SlotRooms.Find(slotRoomId);
            var isEmergency = false;
            var shifts = new List<SurgeryShift>();
            if (technicalStaffId == 0)
            {
                shifts = shiftSlotRooms.SurgeryShifts
                .Where(s => (s.EstimatedStartDateTime != null && s.EstimatedEndDateTime != null)
                && (UtilitiesDate.ConvertDateToNumber(s.EstimatedStartDateTime.Value) == dateNumber)) //mm/dd/YYYY
                .OrderBy(s => s.ActualStartDateTime).OrderBy(s => s.EstimatedStartDateTime).ToList();
            }
            else
            {
                shifts = shiftSlotRooms.SurgeryShifts
                .Where(s => (s.EstimatedStartDateTime != null && s.EstimatedEndDateTime != null)
                && (UtilitiesDate.ConvertDateToNumber(s.EstimatedStartDateTime.Value) == dateNumber)
                && s.TechId.HasValue
                && s.TechId.Value == technicalStaffId) //mm/dd/YYYY
                .OrderBy(s => s.ActualStartDateTime).OrderBy(s => s.EstimatedStartDateTime).ToList();
            }

            foreach (var shift in shifts)
            {
                if (!shift.IsNormalSurgeryTime && shift.ProposedStartDateTime == null)
                {
                    isEmergency = true;
                }
                if (!isEmergency || shift.Patient != null && shift.SurgeryCatalog != null)
                {
                    results.Add(new SurgeryShiftViewModel()
                    {
                        Id = shift.Id,
                        SpecialtyName = shift.SurgeryCatalog.Specialty.Name,
                        //CatalogCode = shift.SurgeryCatalog.Code,
                        CatalogName = shift.SurgeryCatalog.Name,
                        IsEmergency = isEmergency,

                        PriorityNumber = shift.PriorityNumber,
                        EstimatedStartDateTime = shift.EstimatedStartDateTime.Value,
                        EstimatedEndDateTime = shift.EstimatedEndDateTime.Value,
                        ActualStartDateTime = shift.ActualStartDateTime,
                        ActualEndDateTime = shift.ActualEndDateTime,
                        StatusName = _context.Statuses.Find(shift.StatusId).Name,
                        PatientName = shift.Patient.FullName,
                        SurgeonNames = shift.SurgeryShiftSurgeons.Select(s => s.Surgeon.FullName).ToList()
                    });
                }
                else
                {
                    results.Add(new SurgeryShiftViewModel()
                    {
                        Id = shift.Id,
                        PriorityNumber = shift.PriorityNumber,
                        IsEmergency = isEmergency,
                        EstimatedStartDateTime = shift.EstimatedStartDateTime.Value,
                        EstimatedEndDateTime = shift.EstimatedEndDateTime.Value,
                        ActualStartDateTime = shift.ActualStartDateTime,
                        ActualEndDateTime = shift.ActualEndDateTime,
                        StatusName = _context.Statuses.Find(shift.StatusId).Name,
                        SurgeonNames = shift.SurgeryShiftSurgeons.Select(s => s.Surgeon.FullName).ToList()
                    });
                }
            }
            return results;
        }

        //TODO: Lấy danh sách ca mổ chưa lên lịch theo độ ưu tiên và ngày (bao gồm chỉ định và ko chỉ định)
        public ICollection<ScheduleViewModel> GetSurgeryShiftsNoSchedule()
        {
            var result = _context.SurgeryShifts
                .Where(s => s.EstimatedStartDateTime == null && s.EstimatedEndDateTime == null
                && s.IsAvailableMedicalSupplies == true
                && s.ProposedStartDateTime == null && s.ProposedEndDateTime == null
                && s.SlotRoomId == null && s.ConfirmDate != null
                && s.ScheduleDate != null)
                .OrderBy(s => s.ConfirmDate)
                .OrderBy(s => s.ExpectedSurgeryDuration)
                .OrderBy(s => s.PriorityNumber).ToList();
            var surgeryShiftList = new List<ScheduleViewModel>();
            foreach (var shift in result)
            {
                surgeryShiftList.Add(new ScheduleViewModel()
                {
                    SurgeryShiftId = shift.Id,
                    SurgeryCatalogId = shift.SurgeryCatalogId.Value,
                    ProposedStartDateTime = shift.ProposedStartDateTime,
                    ProposedEndDateTime = shift.ProposedEndDateTime,
                    IsNormalSurgeryTime = shift.IsNormalSurgeryTime,
                    ConfirmDate = shift.ConfirmDate.Value,
                    ScheduleDate = shift.ScheduleDate.Value,
                    ExpectedSurgeryDuration = shift.ExpectedSurgeryDuration,
                    PriorityNumber = shift.PriorityNumber,
                    TreatmentId = shift.TreatmentDoctorId.Value
                });
            }
            var proposedTimeSurgeryShiftList = GetSurgeryShiftNoScheduleByProposedTime();

            return proposedTimeSurgeryShiftList.Union(surgeryShiftList)
                .OrderBy(s => s.PriorityNumber)
                .OrderBy(s => s.ScheduleDate).ToList();
        }

        // TODO: Lấy những ca mổ chưa lên lịch theo thời gian chỉ định
        public ICollection<ScheduleViewModel> GetSurgeryShiftNoScheduleByProposedTime()
        {
            var shifts = new List<ScheduleViewModel>();
            var surgeryShifts = _context.SurgeryShifts
                .Where(s => (s.IsAvailableMedicalSupplies == true) && (s.SlotRoomId == null)
                && s.EstimatedStartDateTime == null && s.EstimatedEndDateTime == null
                && s.ProposedStartDateTime != null && s.ProposedEndDateTime != null)
                .OrderBy(s => s.ExpectedSurgeryDuration)
                .OrderBy(s => s.PriorityNumber)
                .OrderBy(s => s.ProposedStartDateTime).ToList();
            foreach (var shift in surgeryShifts)
            {
                shifts.Add(new ScheduleViewModel()
                {
                    SurgeryShiftId = shift.Id,
                    SurgeryCatalogId = shift.SurgeryCatalogId.Value,
                    ProposedStartDateTime = shift.ProposedStartDateTime,
                    ProposedEndDateTime = shift.ProposedEndDateTime,
                    IsNormalSurgeryTime = shift.IsNormalSurgeryTime,
                    ConfirmDate = shift.ConfirmDate.Value,
                    ScheduleDate = shift.ScheduleDate.Value.Date,
                    ExpectedSurgeryDuration = shift.ExpectedSurgeryDuration,
                    PriorityNumber = shift.PriorityNumber,
                    TreatmentId = shift.TreatmentDoctorId.Value
                });
            }
            return shifts;
        }

        public SurgeryShiftDetailViewModel GetShiftDetail(int shiftId)
        {
            var shift = _context.SurgeryShifts.Find(shiftId);
            if (shift != null)
            {
                var usedProcedure = "";
                bool isEmergency = false;
                SurgeryShiftDetailViewModel result = null;
                if (!shift.IsNormalSurgeryTime && shift.ProposedStartDateTime == null && shift.ProposedEndDateTime == null)
                {
                    isEmergency = true;
                }
                if (isEmergency && shift.SurgeryCatalog == null && shift.Patient == null)
                {
                    result = new SurgeryShiftDetailViewModel()
                    {
                        Id = shift.Id,
                        StartTime = shift.EstimatedStartDateTime,
                        EndTime = shift.EstimatedEndDateTime,
                        ActualStartTime = shift.ActualStartDateTime,
                        ActualEndTime = shift.ActualEndDateTime,
                        IsEmergency = isEmergency,
                        Procedure = usedProcedure,
                        StatusName = shift.Status.Name
                    };
                }
                else //Không phải ca cấp cứu
                {
                    var treatmentDoctorName = "";
                    if (shift.TreatmentDoctor != null)
                    {
                        treatmentDoctorName = shift.TreatmentDoctor.FullName;
                    }
                    result = new SurgeryShiftDetailViewModel()
                    {
                        Id = shift.Id,
                        PatientName = shift.Patient.FullName,
                        IsEmergency = isEmergency,
                        Gender = shift.Patient.Gender == 0 ? "Nữ" : "Nam",
                        Age = DateTime.Now.Year - shift.Patient.YearOfBirth,
                        Specialty = shift.SurgeryCatalog.Specialty.Name,
                        SurgeryName = shift.SurgeryCatalog.Name,
                        SurgeryType = shift.SurgeryCatalog.Type,
                        StartTime = shift.EstimatedStartDateTime,
                        EndTime = shift.EstimatedEndDateTime,
                        ActualStartTime = shift.ActualStartDateTime,
                        ActualEndTime = shift.ActualEndDateTime,
                        treatmentDoctorName = treatmentDoctorName,
                        //EkipMembers = shift.Ekip.Members.Select(m => new EkipMemberViewModel() { Name = m.Name, WorkJob = m.WorkJob }).ToList(),
                        Procedure = shift.UsedProcedure == null ? shift.SurgeryCatalog.Procedure : shift.UsedProcedure,
                        StatusName = shift.Status.Name
                    };
                }


                return result;
            }
            return null;
        }

        public Boolean SaveSurgeryProcedure(SurgeryProcedureViewModel SurgeryProcedure)
        {
            var shift = _context.SurgeryShifts.Find(SurgeryProcedure.SurgeryShiftId);
            shift.UsedProcedure = SurgeryProcedure.Procedure;
            _context.SaveChanges();
            return true;
        }


        #region Change Shift
        public bool ChangeFirstPriority(ShiftChangeViewModel newShift)
        {
            var shift = _context.SurgeryShifts.Find(newShift.Id);
            if (shift != null)
            {
                var tracker = _context.Database.BeginTransaction();
                try
                {
                    #region Update Shift
                    shift.PriorityNumber = newShift.NewPriority;
                    shift.DateUpdated = DateTime.Now;
                    _context.Update(shift);
                    _context.SaveChanges();
                    #endregion

                    //#region Create Log
                    //_context.SurgeryShiftChangeLogs.Add(new ShiftChangeLog()
                    //{
                    //    Description = newShift.ChangeLogDescription,
                    //    ShiftId = shift.Id
                    //});
                    //_context.SaveChanges();
                    //#endregion

                    tracker.Commit();
                    return true;
                }
                catch (DbUpdateException)
                {
                    tracker.Rollback();
                    return false;
                }

            }
            return false;
        }

        public bool ChangeSchedule(ShiftScheduleChangeViewModel newShift)
        {
            var shift = _context.SurgeryShifts.Find(newShift.Id);
            if (shift != null)
            {
                var tracker = _context.Database.BeginTransaction();

                try
                {
                    #region Change Schedule
                    shift.EstimatedStartDateTime = newShift.EstimatedStartDateTime;
                    shift.EstimatedEndDateTime = newShift.EstimatedEndDateTime;
                    shift.SlotRoomId = newShift.SlotRoomId;
                    shift.DateUpdated = DateTime.Now;
                    _context.Update(shift);
                    _context.SaveChanges();
                    #endregion

                    //#region Create Log
                    //_context.SurgeryShiftChangeLogs.Add(new ShiftChangeLog()
                    //{
                    //    Description = newShift.ChangeLogDescription,
                    //    ShiftId = shift.Id
                    //});
                    //_context.SaveChanges();
                    //#endregion

                    tracker.Commit();
                    return true;
                }
                catch (DbUpdateException)
                {
                    tracker.Rollback();
                    return false;
                }
            }
            return false;
        }

        public List<int> GetAvailableRoom(DateTime start, DateTime end, bool forcedChange, int SpecialtyGroupId = 0)
        {
            if (!IsValidTime(start, end) && !forcedChange)
            {
                throw new Exception("Time");
            }

            var affectedShiftResults = new List<AffectedShiftViewModel>();

            var rooms = new List<SlotRoom>(); //n1
            if (SpecialtyGroupId != 0)
            {
                rooms = _context.SlotRooms.Where(r => !r.IsDeleted && r.SurgeryRoom.SpecialtyGroupId == SpecialtyGroupId).ToList();
            }
            else
            {
                rooms = _context.SlotRooms.Where(r => !r.IsDeleted && !r.SurgeryRoom.SpecialtyGroupId.HasValue).ToList();
            }
            var roomId = new List<int>();

            foreach (var room in rooms)
            {
                var shifts = new List<SurgeryShift>();
                if (start.DayOfYear > DateTime.Now.DayOfYear)//n2
                {
                    var gapDays = (start - DateTime.Now).Days;
                    shifts = room.SurgeryShifts.Where(s =>
                                            !s.IsDeleted &&
                                            s.EstimatedStartDateTime.Value > GetMidnight(DateTime.Now.AddDays(gapDays)) &&
                                            s.Status.Name.Equals("Preoperative", StringComparison.CurrentCultureIgnoreCase))
                                            .ToList();

                }
                else
                {
                    shifts = room.SurgeryShifts.Where(s =>
                                                !s.IsDeleted &&
                                                (s.EstimatedStartDateTime.Value > DateTime.Now
                                                || s.EstimatedStartDateTime.Value <= DateTime.Now && s.EstimatedEndDateTime.Value > DateTime.Now)
                                                &&
                                                s.Status.Name.Equals("Preoperative", StringComparison.CurrentCultureIgnoreCase))
                                                .ToList();
                }


                if (shifts.Any()) // n3
                {
                    var affectedShifts = shifts.Where(s =>
                        (s.EstimatedStartDateTime.Value >= start && s.EstimatedStartDateTime.Value < end)
                        || (s.EstimatedEndDateTime.Value > start && s.EstimatedEndDateTime.Value <= end)
                        || (s.EstimatedStartDateTime.Value <= start && s.EstimatedEndDateTime.Value > end))
                        .ToList();
                    if (!affectedShifts.Any())
                    {
                        roomId.Add(room.Id);
                    }
                    else
                    {
                        affectedShiftResults.AddRange(affectedShifts.Select(s => new AffectedShiftViewModel()
                        {
                            EstimatedEnd = s.EstimatedEndDateTime.Value,
                            EstimatedStart = s.EstimatedStartDateTime.Value,
                            ShiftId = s.Id
                        }).ToList());
                    }
                }
                else
                {
                    roomId.Add(room.Id);
                }

            }
            return roomId.OrderBy(c => c).ToList(); // n1 * ( max(n2,n3))
        }

        public List<AvailableRoomViewModel> GetAvailableRoom(DateTime? date, int specialtyGroupId, int hour, int minute, int? longerShiftId = null, List<int> shiftIds = null)
        {
            var expectedTimeSpan = new TimeSpan(hour, minute, 0);
            var results = new List<AvailableRoomViewModel>();
            var rooms = _context.SlotRooms.Where(r => !r.IsDeleted && r.SurgeryRoom.SpecialtyGroupId == specialtyGroupId).ToList();
            foreach (var room in rooms)
            {
                var shifts = new List<SurgeryShift>();
                if (longerShiftId.HasValue && shiftIds != null)
                {
                    if (date.Value.DayOfYear > DateTime.Now.DayOfYear)
                    {
                        var gapDays = (date.Value - DateTime.Now).Days;

                        shifts = room.SurgeryShifts.Where(s =>
                        !s.IsDeleted &&
                        s.EstimatedStartDateTime.Value > GetMidnight(DateTime.Now.AddDays(gapDays)) &&
                        s.Status.Name.Equals("Preoperative", StringComparison.CurrentCultureIgnoreCase))
                        .ToList();
                    }
                    else
                    {
                        shifts = room.SurgeryShifts.Where(s =>
                        !s.IsDeleted &&
                        s.EstimatedStartDateTime.Value > DateTime.Now &&
                        s.Status.Name.Equals("Preoperative", StringComparison.CurrentCultureIgnoreCase))
                        .ToList();
                    }

                    if (room.Id == _context.SurgeryShifts.Find(longerShiftId).SlotRoomId)
                    {
                        shifts.Add(_context.SurgeryShifts.Find(longerShiftId));
                    }
                    foreach (var shiftId in shiftIds)
                    {
                        if (room.Id == _context.SurgeryShifts.Find(shiftId).SlotRoomId)
                        {
                            shifts.Remove(_context.SurgeryShifts.Find(shiftId));
                        }
                    }
                }
                else
                {
                    shifts = room.SurgeryShifts.Where(s =>
                        !s.IsDeleted &&
                        s.EstimatedStartDateTime.Value > DateTime.Now &&
                        s.Status.Name.Equals("Preoperative", StringComparison.CurrentCultureIgnoreCase))
                        .ToList();
                }


                if (shifts.Any())
                {
                    shifts = shifts.OrderBy(s => s.EstimatedStartDateTime).ToList();

                    for (int i = 0; i < shifts.Count; i++)
                    {
                        var shift = shifts.ElementAt(i);

                        if (IsLastShiftOfADay(shift, shifts) || i == shifts.Count - 1) // Last Shift of a day or Last Shift of that room
                        {
                            if (shift.EstimatedEndDateTime.Value.Hour < 17 && shift.EstimatedEndDateTime.Value.Hour >= 7)
                            {
                                if (!IsInBreakTime(shift.EstimatedEndDateTime.Value))
                                {
                                    results.Add(new AvailableRoomViewModel()
                                    {
                                        RoomId = room.Id,
                                        StartDateTime = shift.EstimatedEndDateTime.Value,
                                        EndDateTime = shift.EstimatedEndDateTime.Value.AddHours(hour).AddMinutes(minute)
                                    });
                                }
                                else
                                {
                                    var startTime = AddToAfterBreakTime(shift.EstimatedEndDateTime.Value);
                                    results.Add(new AvailableRoomViewModel()
                                    {
                                        RoomId = room.Id,
                                        StartDateTime = startTime,
                                        EndDateTime = startTime.AddHours(hour).AddMinutes(minute)
                                    });
                                }

                            }
                            else if (shift.EstimatedEndDateTime.Value.Hour >= 17 && i == shifts.Count - 1)
                            {
                                var start = GetCurrentDayWorkingHour(true, shift.EstimatedStartDateTime.Value).AddDays(1);
                                results.Add(new AvailableRoomViewModel()
                                {
                                    RoomId = room.Id,
                                    StartDateTime = start,
                                    EndDateTime = start.AddHours(hour).AddMinutes(minute)
                                });
                            }
                            else if (shift.EstimatedEndDateTime.Value.Hour < 7 && i == shifts.Count - 1)
                            {
                                var start = GetCurrentDayWorkingHour(true, shift.EstimatedEndDateTime.Value);
                                results.Add(new AvailableRoomViewModel()
                                {
                                    RoomId = room.Id,
                                    StartDateTime = start,
                                    EndDateTime = start.AddHours(hour).AddMinutes(minute)
                                });
                            }
                        }
                        else
                        {
                            var shiftAfter = shifts.ElementAt(i + 1);
                            var gap = shiftAfter.EstimatedStartDateTime.Value - shift.EstimatedEndDateTime.Value;
                            if (expectedTimeSpan <= gap && !IsInBreakTime(shift.EstimatedEndDateTime.Value))
                            {
                                results.Add(new AvailableRoomViewModel()
                                {
                                    RoomId = room.Id,
                                    StartDateTime = shift.EstimatedEndDateTime.Value,
                                    EndDateTime = shift.EstimatedEndDateTime.Value.AddHours(hour).AddMinutes(minute)
                                });
                            }
                        }
                    }
                }
                else
                {
                    results.Add(new AvailableRoomViewModel()
                    {
                        RoomId = room.Id,
                        StartDateTime = GetCurrentDayWorkingHour(true, DateTime.Now),
                        EndDateTime = GetCurrentDayWorkingHour(true, DateTime.Now).AddHours(hour).AddMinutes(minute),
                    });
                }
            }
            return results.OrderBy(r => r.StartDateTime).ToList();
        }

        public bool ChangeShiftStatus(ShiftStatusChangeViewModel currentShift)
        {
            try
            {
                var shift = _context.SurgeryShifts.Find(currentShift.ShiftId);
                if (shift != null)
                {
                    var enumStatus = new ShiftStatus();
                    if (Enum.TryParse<ShiftStatus>(currentShift.CurrentStatus.Trim().ToUpper(), out enumStatus))
                    {
                        var nextStatus = (ShiftStatus)Enum.ToObject(typeof(ShiftStatus), (int)enumStatus + 1);
                        var status = _context.Statuses.FirstOrDefault(s => s.Name.Equals(nextStatus.ToString(), StringComparison.CurrentCultureIgnoreCase));

                        shift.StatusId = status.Id;
                        shift.DateUpdated = DateTime.Now;
                        _context.Update(shift);
                        _context.SaveChanges();
                    }

                    return true;
                }
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }

        }

        public SwapShiftResultViewModel SwapShift(int shift1Id, int shift2Id)
        {
            var result = new SwapShiftResultViewModel();
            var longerShift = _context.SurgeryShifts.Find(shift1Id);
            var shift = _context.SurgeryShifts.Find(shift2Id);
            var longerDuration = longerShift.EstimatedEndDateTime.Value - longerShift.EstimatedStartDateTime.Value;
            var duration = shift.EstimatedEndDateTime.Value - shift.EstimatedStartDateTime.Value;
            try
            {
                var swapParamResult = SwapParamName(ref longerShift, ref shift, ref longerDuration, ref duration);
                if (swapParamResult)
                {
                    if (shift.SlotRoomId != longerShift.SlotRoomId)//n * n1 * ( max(n2,n3))
                    {
                        var longerShiftRoomId = longerShift.SlotRoomId.Value;
                        var shiftRoomId = shift.SlotRoomId.Value;

                        #region Swap and Disable Longer Shift
                        //Shift Schedule Change VMs
                        var shiftChangeVM = new ShiftScheduleChangeViewModel()
                        {
                            Id = shift.Id,
                            SlotRoomId = longerShiftRoomId,
                            EstimatedStartDateTime = longerShift.EstimatedStartDateTime.Value,
                            EstimatedEndDateTime = longerShift.EstimatedStartDateTime.Value + duration
                        };
                        var longerShiftChangeVM = new ShiftScheduleChangeViewModel()
                        {
                            Id = longerShift.Id,
                            SlotRoomId = shiftRoomId,
                            EstimatedStartDateTime = shift.EstimatedStartDateTime.Value,
                            EstimatedEndDateTime = shift.EstimatedStartDateTime.Value + longerDuration
                        };

                        var swapResult = ChangeSchedule(shiftChangeVM);
                        swapResult = ChangeSchedule(longerShiftChangeVM);
                        longerShift.IsDeleted = true;
                        _context.Update(longerShift);
                        _context.SaveChanges();
                        #endregion

                        var affectedShifts = GetAffectedShifts(longerShift, shift); //n
                        var affectedShiftIds = affectedShifts.Select(s => s.Id).ToList();

                        #region Resolve Affected Shifts
                        if (affectedShifts.Any())
                        {
                            foreach (var affectedShift in affectedShifts)
                            {
                                //n1 * (max(n2, n3))
                                var slotRoomIds = GetAvailableRoom(affectedShift.EstimatedStartDateTime.Value, affectedShift.EstimatedEndDateTime.Value, false, affectedShift.SlotRoom.SurgeryRoom.SpecialtyGroupId.Value);
                                if (slotRoomIds.Any())
                                {
                                    var resolvedShift = new AffectedShiftResultViewModel()
                                    {
                                        ShiftId = affectedShift.Id,
                                        OldRoomName = affectedShift.SlotRoom.Name,
                                    };
                                    result.Succeed = ChangeSchedule(new ShiftScheduleChangeViewModel()
                                    {
                                        Id = affectedShift.Id,
                                        SlotRoomId = slotRoomIds.FirstOrDefault(),
                                        EstimatedEndDateTime = affectedShift.EstimatedEndDateTime.Value,
                                        EstimatedStartDateTime = affectedShift.EstimatedStartDateTime.Value
                                    });
                                    if (result.Succeed)
                                    {
                                        resolvedShift.NewRoomName = _context.SurgeryRooms.Find(slotRoomIds.FirstOrDefault()).Name;
                                        result.AffectedShifts.Add(resolvedShift);
                                    }
                                }
                                else
                                {

                                    var affectedShiftDuration = affectedShift.EstimatedEndDateTime.Value - affectedShift.EstimatedStartDateTime.Value;
                                    var rooms = GetAvailableRoom(affectedShift.EstimatedStartDateTime.Value, affectedShift.SurgeryCatalog.Specialty.SpecialtyGroupId.Value, affectedShiftDuration.Hours, affectedShiftDuration.Minutes, longerShift.Id, affectedShiftIds);
                                    affectedShiftIds.Remove(affectedShift.Id);
                                    rooms = rooms.Where(r =>
                                                        r.StartDateTime >= longerShift.EstimatedEndDateTime.Value ||
                                                        r.EndDateTime <= longerShift.EstimatedStartDateTime.Value)
                                                        .ToList();
                                    if (rooms.Any())
                                    {
                                        var resolvedShift = new AffectedShiftResultViewModel()
                                        {
                                            ShiftId = affectedShift.Id,
                                            OldRoomName = affectedShift.SlotRoom.Name,
                                            OldStart = affectedShift.EstimatedStartDateTime.Value,
                                            OldEnd = affectedShift.EstimatedEndDateTime.Value
                                        };

                                        result.Succeed = ChangeSchedule(new ShiftScheduleChangeViewModel()
                                        {
                                            Id = affectedShift.Id,
                                            SlotRoomId = rooms.FirstOrDefault().RoomId,
                                            EstimatedEndDateTime = rooms.FirstOrDefault().EndDateTime,
                                            EstimatedStartDateTime = rooms.FirstOrDefault().StartDateTime
                                        });

                                        if (result.Succeed)
                                        {
                                            resolvedShift.NewRoomName = _context.SurgeryRooms.Find(rooms.FirstOrDefault().RoomId).Name;
                                            resolvedShift.NewStart = rooms.FirstOrDefault().StartDateTime;
                                            resolvedShift.NewEnd = rooms.FirstOrDefault().EndDateTime;
                                            result.AffectedShifts.Add(resolvedShift);
                                        }
                                    }
                                }
                            }
                        }
                        #endregion

                        #region Enable Longer Shift
                        longerShift.IsDeleted = false;
                        _context.Update(longerShift);
                        _context.SaveChanges();
                        #endregion

                        result.Succeed = true;
                        return result;
                    }
                    else
                    {
                        if (IsNextTo(shift, longerShift)) //n4
                        {
                            var longerShiftRoomId = longerShift.SlotRoomId.Value;
                            var shiftRoomId = shift.SlotRoomId.Value;

                            //Shift Schedule Change VMs
                            var longerShiftChangeVM = new ShiftScheduleChangeViewModel()
                            {
                                Id = longerShift.Id,
                                SlotRoomId = shiftRoomId,
                                EstimatedStartDateTime = shift.EstimatedStartDateTime.Value,
                                EstimatedEndDateTime = shift.EstimatedStartDateTime.Value + longerDuration
                            };
                            var shiftChangeVM = new ShiftScheduleChangeViewModel()
                            {
                                Id = shift.Id,
                                SlotRoomId = longerShiftRoomId,
                                EstimatedStartDateTime = longerShiftChangeVM.EstimatedEndDateTime,
                                EstimatedEndDateTime = longerShiftChangeVM.EstimatedEndDateTime + duration
                            };


                            var swapResult = ChangeSchedule(shiftChangeVM);
                            swapResult = ChangeSchedule(longerShiftChangeVM);
                            result.Succeed = swapResult;
                            return result;
                        }
                        else //n * (n1 * (max(n2, n3)))^2
                        {
                            var longerShiftRoomId = longerShift.SlotRoomId.Value;
                            var shiftRoomId = shift.SlotRoomId.Value;

                            #region Swap and Disable Longer Shift
                            //Shift Schedule Change VMs
                            var shiftChangeVM = new ShiftScheduleChangeViewModel()
                            {
                                Id = shift.Id,
                                SlotRoomId = longerShiftRoomId,
                                EstimatedStartDateTime = longerShift.EstimatedStartDateTime.Value,
                                EstimatedEndDateTime = longerShift.EstimatedStartDateTime.Value + duration
                            };
                            var longerShiftChangeVM = new ShiftScheduleChangeViewModel()
                            {
                                Id = longerShift.Id,
                                SlotRoomId = shiftRoomId,
                                EstimatedStartDateTime = shift.EstimatedStartDateTime.Value,
                                EstimatedEndDateTime = shift.EstimatedStartDateTime.Value + longerDuration
                            };

                            var swapResult = ChangeSchedule(shiftChangeVM);
                            swapResult = ChangeSchedule(longerShiftChangeVM);
                            longerShift.IsDeleted = true;
                            _context.Update(longerShift);
                            _context.SaveChanges();
                            #endregion

                            var affectedShifts = GetAffectedShifts(longerShift, shift);
                            var affectedShiftIds = affectedShifts.Select(s => s.Id).ToList();

                            #region Resolve Affected Shifts
                            if (affectedShifts.Any())
                            {
                                foreach (var affectedShift in affectedShifts)
                                {
                                    //n1 * (max(n2, n3))
                                    var roomIds = GetAvailableRoom(affectedShift.EstimatedStartDateTime.Value, affectedShift.EstimatedEndDateTime.Value, false, affectedShift.SurgeryCatalog.SpecialtyId);
                                    if (roomIds.Any())
                                    {
                                        var resolvedShift = new AffectedShiftResultViewModel()
                                        {
                                            ShiftId = affectedShift.Id,
                                            OldRoomName = affectedShift.SlotRoom.Name,
                                        };
                                        result.Succeed = ChangeSchedule(new ShiftScheduleChangeViewModel()
                                        {
                                            Id = affectedShift.Id,
                                            SlotRoomId = roomIds.FirstOrDefault(),
                                            EstimatedEndDateTime = affectedShift.EstimatedEndDateTime.Value,
                                            EstimatedStartDateTime = affectedShift.EstimatedStartDateTime.Value
                                        });
                                        if (result.Succeed)
                                        {
                                            resolvedShift.NewRoomName = _context.SurgeryRooms.Find(roomIds.FirstOrDefault()).Name;
                                            result.AffectedShifts.Add(resolvedShift);
                                        }
                                    }
                                    else
                                    {

                                        var affectedShiftDuration = affectedShift.EstimatedEndDateTime.Value - affectedShift.EstimatedStartDateTime.Value;
                                        var rooms = GetAvailableRoom(affectedShift.EstimatedStartDateTime.Value, affectedShift.SurgeryCatalog.Specialty.SpecialtyGroupId.Value, affectedShiftDuration.Hours, affectedShiftDuration.Minutes, longerShift.Id, affectedShiftIds);
                                        affectedShiftIds.Remove(affectedShift.Id);
                                        rooms = rooms.Where(r =>
                                                            r.StartDateTime >= longerShift.EstimatedEndDateTime.Value ||
                                                            r.EndDateTime <= longerShift.EstimatedStartDateTime.Value)
                                                            .ToList();
                                        if (rooms.Any())
                                        {
                                            var resolvedShift = new AffectedShiftResultViewModel()
                                            {
                                                ShiftId = affectedShift.Id,
                                                OldRoomName = affectedShift.SlotRoom.Name,
                                                OldStart = affectedShift.EstimatedStartDateTime.Value,
                                                OldEnd = affectedShift.EstimatedEndDateTime.Value
                                            };

                                            result.Succeed = ChangeSchedule(new ShiftScheduleChangeViewModel()
                                            {
                                                Id = affectedShift.Id,
                                                SlotRoomId = rooms.FirstOrDefault().RoomId,
                                                EstimatedEndDateTime = rooms.FirstOrDefault().EndDateTime,
                                                EstimatedStartDateTime = rooms.FirstOrDefault().StartDateTime
                                            });

                                            if (result.Succeed)
                                            {
                                                resolvedShift.NewRoomName = _context.SurgeryRooms.Find(rooms.FirstOrDefault().RoomId).Name;
                                                resolvedShift.NewStart = rooms.FirstOrDefault().StartDateTime;
                                                resolvedShift.NewEnd = rooms.FirstOrDefault().EndDateTime;
                                                result.AffectedShifts.Add(resolvedShift);
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region Enable Longer Shift
                            longerShift.IsDeleted = false;
                            _context.Update(longerShift);
                            _context.SaveChanges();
                            #endregion

                            result.Succeed = true;
                            return result;
                        }
                    }
                }
                else
                {
                    var shiftChangeVM = new ShiftScheduleChangeViewModel()
                    {
                        Id = shift.Id,
                        SlotRoomId = longerShift.SlotRoomId.Value,
                        EstimatedStartDateTime = longerShift.EstimatedStartDateTime.Value,
                        EstimatedEndDateTime = longerShift.EstimatedEndDateTime.Value
                    };

                    var longerShiftChangeVM = new ShiftScheduleChangeViewModel()
                    {
                        Id = longerShift.Id,
                        SlotRoomId = shift.SlotRoomId.Value,
                        EstimatedStartDateTime = shift.EstimatedStartDateTime.Value,
                        EstimatedEndDateTime = shift.EstimatedEndDateTime.Value
                    };

                    result.Succeed = ChangeSchedule(shiftChangeVM);//Shift
                    result.Succeed = ChangeSchedule(longerShiftChangeVM);//Longer Shift

                    return result;
                }
            }
            catch (DbUpdateException)
            {
                //transaction.Rollback();
                ChangeSchedule(new ShiftScheduleChangeViewModel()
                {
                    Id = shift.Id,
                    SlotRoomId = longerShift.SlotRoomId.Value,
                    EstimatedStartDateTime = longerShift.EstimatedStartDateTime.Value,
                    EstimatedEndDateTime = longerShift.EstimatedStartDateTime.Value + duration
                });//Shift
                ChangeSchedule(new ShiftScheduleChangeViewModel()
                {
                    Id = longerShift.Id,
                    SlotRoomId = shift.SlotRoomId.Value,
                    EstimatedStartDateTime = shift.EstimatedStartDateTime.Value,
                    EstimatedEndDateTime = shift.EstimatedStartDateTime.Value + longerDuration
                });//Longer Shift
                return result;
            }
        }

        public SwapShiftResultViewModel SwapShiftToRoom(int shiftId, int roomId, bool forcedSwap)
        {
            var result = new SwapShiftResultViewModel();
            var shift = _context.SurgeryShifts.Find(shiftId);
            if (forcedSwap)
            {
                var affectedShifts = GetAffectedShifts(roomId, shift.EstimatedStartDateTime.Value, shift.EstimatedEndDateTime.Value);
                if (affectedShifts.Any())
                {
                    var changeRoomVM = new ShiftScheduleChangeViewModel()
                    {
                        SlotRoomId = roomId,
                        Id = shiftId,
                        EstimatedStartDateTime = shift.EstimatedStartDateTime.Value,
                        EstimatedEndDateTime = shift.EstimatedEndDateTime.Value,
                    };
                    var changeResult = ChangeSchedule(changeRoomVM);
                    shift.IsDeleted = true;
                    _context.Update(shift);
                    _context.SaveChanges();

                    //Resolve Affected Shifts
                    var affectedShiftIds = affectedShifts.Select(s => s.Id).ToList();
                    foreach (var affectedShift in affectedShifts)
                    {
                        var roomIds = GetAvailableRoom(affectedShift.EstimatedStartDateTime.Value, affectedShift.EstimatedEndDateTime.Value, false, affectedShift.SurgeryCatalog.SpecialtyId);
                        if (roomIds.Any())
                        {
                            var resolvedShift = new AffectedShiftResultViewModel()
                            {
                                ShiftId = affectedShift.Id,
                                OldRoomName = affectedShift.SlotRoom.Name,
                            };
                            result.Succeed = ChangeSchedule(new ShiftScheduleChangeViewModel()
                            {
                                Id = affectedShift.Id,
                                SlotRoomId = roomIds.FirstOrDefault(),
                                EstimatedEndDateTime = affectedShift.EstimatedEndDateTime.Value,
                                EstimatedStartDateTime = affectedShift.EstimatedStartDateTime.Value
                            });
                            if (result.Succeed)
                            {
                                resolvedShift.NewRoomName = _context.SurgeryRooms.Find(roomIds.FirstOrDefault()).Name;
                                result.AffectedShifts.Add(resolvedShift);
                            }
                        }
                        else
                        {

                            var affectedShiftDuration = affectedShift.EstimatedEndDateTime.Value - affectedShift.EstimatedStartDateTime.Value;
                            var rooms = GetAvailableRoom(affectedShift.EstimatedStartDateTime.Value, affectedShift.SurgeryCatalog.Specialty.SpecialtyGroupId.Value, affectedShiftDuration.Hours, affectedShiftDuration.Minutes, shift.Id, affectedShiftIds);
                            affectedShiftIds.Remove(affectedShift.Id);
                            rooms = rooms.Where(r =>
                                                r.StartDateTime >= shift.EstimatedEndDateTime.Value ||
                                                r.EndDateTime <= shift.EstimatedStartDateTime.Value)
                                                .ToList();
                            if (rooms.Any())
                            {
                                var resolvedShift = new AffectedShiftResultViewModel()
                                {
                                    ShiftId = affectedShift.Id,
                                    OldRoomName = affectedShift.SlotRoom.Name,
                                    OldStart = affectedShift.EstimatedStartDateTime.Value,
                                    OldEnd = affectedShift.EstimatedEndDateTime.Value
                                };

                                result.Succeed = ChangeSchedule(new ShiftScheduleChangeViewModel()
                                {
                                    Id = affectedShift.Id,
                                    SlotRoomId = rooms.FirstOrDefault().RoomId,
                                    EstimatedEndDateTime = rooms.FirstOrDefault().EndDateTime,
                                    EstimatedStartDateTime = rooms.FirstOrDefault().StartDateTime
                                });

                                if (result.Succeed)
                                {
                                    resolvedShift.NewRoomName = _context.SurgeryRooms.Find(rooms.FirstOrDefault().RoomId).Name;
                                    resolvedShift.NewStart = rooms.FirstOrDefault().StartDateTime;
                                    resolvedShift.NewEnd = rooms.FirstOrDefault().EndDateTime;
                                    result.AffectedShifts.Add(resolvedShift);
                                }
                            }
                        }
                    }

                    shift.IsDeleted = false;
                    _context.Update(shift);
                    _context.SaveChanges();
                }
            }
            else
            {
                var roomIds = GetAvailableRoom(shift.EstimatedStartDateTime.Value, shift.EstimatedEndDateTime.Value, false, shift.SurgeryCatalog.SpecialtyId);
                if (roomIds.Any(r => r == roomId))
                {
                    var changeRoomVM = new ShiftScheduleChangeViewModel()
                    {
                        SlotRoomId = roomId,
                        Id = shiftId,
                        EstimatedStartDateTime = shift.EstimatedStartDateTime.Value,
                        EstimatedEndDateTime = shift.EstimatedEndDateTime.Value,
                    };
                    result.Succeed = ChangeSchedule(changeRoomVM);
                }
                else
                {
                    var affectedShiftDuration = shift.EstimatedEndDateTime.Value - shift.EstimatedStartDateTime.Value;
                    var rooms = GetAvailableRoom(shift.EstimatedStartDateTime.Value, affectedShiftDuration.Hours, affectedShiftDuration.Minutes, shift.Id);

                    if (rooms.Any(r => r.RoomId == roomId))
                    {
                        var newSchedule = rooms.Where(r => r.RoomId == roomId).OrderBy(r => r.StartDateTime).FirstOrDefault();
                        var changeRoomVM = new ShiftScheduleChangeViewModel()
                        {
                            SlotRoomId = roomId,
                            Id = shiftId,
                            EstimatedStartDateTime = newSchedule.StartDateTime,
                            EstimatedEndDateTime = newSchedule.EndDateTime,
                        };

                        result.Succeed = ChangeSchedule(changeRoomVM);
                    }
                }
            }

            return result;
        }

        public List<int> GetSwapableShiftIds()
        {
            var results = new List<int>();
            var rooms = _context.SlotRooms.Where(r => !r.IsDeleted);
            foreach (var room in rooms)
            {
                var shifts = room.SurgeryShifts.Where(s =>
                                                        !s.IsDeleted &&
                                                        s.EstimatedStartDateTime.Value > DateTime.Now &&
                                                        s.Status.Name.Equals("Preoperative", StringComparison.CurrentCultureIgnoreCase))
                                                        .Select(s => s.Id).ToList();
                results.AddRange(shifts);
            }
            return results.OrderBy(r => r).ToList();
        }

        public List<AffectedShiftViewModel> GetAffectedShifts(DateTime start, DateTime end, int specialtyGroupId)
        {
            var affectedShiftResults = new List<AffectedShiftViewModel>();

            var rooms = new List<SlotRoom>();
            rooms = _context.SlotRooms.Where(r => !r.IsDeleted && r.SurgeryRoom.SpecialtyGroupId == specialtyGroupId).ToList();
            var roomId = new List<int>();

            foreach (var room in rooms)
            {
                var shifts = new List<SurgeryShift>();
                if (start.DayOfYear > DateTime.Now.DayOfYear)//n2
                {
                    var gapDays = (start - DateTime.Now).Days;
                    shifts = room.SurgeryShifts.Where(s =>
                                            !s.IsDeleted &&
                                            s.EstimatedStartDateTime.Value > GetMidnight(DateTime.Now.AddDays(gapDays)) &&
                                            s.Status.Name.Equals("Preoperative", StringComparison.CurrentCultureIgnoreCase))
                                            .ToList();

                }
                else
                {
                    shifts = room.SurgeryShifts.Where(s =>
                                                !s.IsDeleted &&
                                                (s.EstimatedStartDateTime.Value > DateTime.Now
                                                || s.EstimatedStartDateTime.Value <= DateTime.Now && s.EstimatedEndDateTime.Value > DateTime.Now)
                                                &&
                                                s.Status.Name.Equals("Preoperative", StringComparison.CurrentCultureIgnoreCase))
                                                .ToList();
                }


                if (shifts.Any())
                {
                    var affectedShifts = shifts.Where(s =>
                        (s.EstimatedStartDateTime.Value >= start && s.EstimatedStartDateTime.Value < end)
                        || (s.EstimatedEndDateTime.Value > start && s.EstimatedEndDateTime.Value <= end)
                        || (s.EstimatedStartDateTime.Value <= start && s.EstimatedEndDateTime.Value > end))
                        .ToList();

                    if (affectedShifts.Any())
                    {
                        affectedShiftResults.AddRange(affectedShifts.Select(s => new AffectedShiftViewModel()
                        {
                            EstimatedEnd = s.EstimatedEndDateTime.Value,
                            EstimatedStart = s.EstimatedStartDateTime.Value,
                            ShiftId = s.Id
                        }).ToList());
                    }
                }
            }
            return affectedShiftResults;
        }

        #endregion

        #region Processing
        private bool IsLastShiftOfADay(SurgeryShift currentShift, List<SurgeryShift> shiftList)
        {
            var currentShiftDay = currentShift.EstimatedStartDateTime.Value.ToShortDateString();
            var currentDayShifts = shiftList.Where(s => s.EstimatedStartDateTime.Value.ToShortDateString().Equals(currentShiftDay)).ToList();
            if (currentShift == currentDayShifts.LastOrDefault())
            {
                return true;
            }
            return false;
        }
        private bool IsInBreakTime(DateTime startTime)
        {
            if (startTime > GetCurrentDayBreakTime(true, startTime) && startTime < GetCurrentDayBreakTime(false, startTime))
            {
                return true;
            }
            return false;
        }
        private DateTime GetCurrentDayWorkingHour(bool isStartTime, DateTime currentDay)
        {
            var midnight = GetMidnight(currentDay);
            if (isStartTime)
            {
                return midnight + new TimeSpan(7, 0, 0);
            }
            return midnight + new TimeSpan(17, 0, 0);
        }
        private DateTime GetCurrentDayBreakTime(bool isStartTime, DateTime currentDay)
        {

            var midnight = GetMidnight(currentDay);
            if (isStartTime)
            {
                return midnight + new TimeSpan(11, 0, 0);
            }
            return midnight + new TimeSpan(13, 0, 0);
        }
        private DateTime AddToAfterBreakTime(DateTime currentTime)
        {
            var gap = GetCurrentDayBreakTime(false, currentTime) - currentTime;
            return currentTime + gap;
        }
        private bool SwapParamName(ref SurgeryShift longerShift, ref SurgeryShift shift, ref TimeSpan longerDuration, ref TimeSpan duration)
        {
            var tempShift = new SurgeryShift();
            var tempDuration = new TimeSpan();

            if (Math.Floor(longerDuration.TotalMinutes) < Math.Floor(duration.TotalMinutes))
            {
                tempShift = shift;
                shift = longerShift;
                longerShift = tempShift;

                tempDuration = duration;
                duration = longerDuration;
                longerDuration = tempDuration;
                return true;
            }
            else if (Math.Floor(longerDuration.TotalMinutes) > Math.Floor(duration.TotalMinutes))
            {
                return true;
            }
            return false;
        }
        private List<SurgeryShift> GetAffectedShifts(SurgeryShift longerShift, SurgeryShift shift)
        {
            var affectedRoom = longerShift.SlotRoom;
            var shifts = new List<SurgeryShift>();
            if (shift.EstimatedStartDateTime.Value.DayOfYear > DateTime.Now.DayOfYear)
            {
                var gapDays = (shift.EstimatedStartDateTime.Value - DateTime.Now).Days;

                shifts = affectedRoom.SurgeryShifts.Where(s =>
                                !s.IsDeleted &&
                                s.EstimatedStartDateTime.Value > GetMidnight(DateTime.Now.AddDays(gapDays)) &&
                                s.Status.Name.Equals("Preoperative", StringComparison.CurrentCultureIgnoreCase) &&
                                s.Id != longerShift.Id &&
                                s.EstimatedStartDateTime.Value < longerShift.EstimatedEndDateTime.Value &&
                                s.EstimatedStartDateTime.Value >= longerShift.EstimatedStartDateTime.Value)
                                .ToList();
            }
            else
            {
                shifts = affectedRoom.SurgeryShifts.Where(s =>
                                                !s.IsDeleted &&
                                                s.EstimatedStartDateTime.Value > DateTime.Now &&
                                                s.Status.Name.Equals("Preoperative", StringComparison.CurrentCultureIgnoreCase) &&
                                                s.Id != longerShift.Id &&
                                                s.EstimatedStartDateTime.Value < longerShift.EstimatedEndDateTime.Value &&
                                                s.EstimatedStartDateTime.Value >= longerShift.EstimatedStartDateTime.Value)
                                                .ToList();
            }

            return shifts;
        }
        private DateTime GetMidnight(DateTime date)
        {
            var hr = date.Hour;
            var min = date.Minute;
            var sec = date.Second;
            var milliSec = date.Millisecond;

            var midnight = date
                .AddHours(-hr)
                .AddMinutes(-min)
                .AddSeconds(-sec)
                .AddMilliseconds(-milliSec);

            return midnight;
        }
        private bool IsValidTime(DateTime start, DateTime end)
        {
            if (start.Day == end.Day)
            {
                var maximumStart = GetMidnight(start) + new TimeSpan(19, 0, 0);
                if (start <= maximumStart && start >= DateTime.Now)
                {
                    return true;
                }
            }
            return false;
        }
        private bool IsNextTo(SurgeryShift shift, SurgeryShift longerShift)
        {
            if (shift.SlotRoomId == longerShift.SlotRoomId)
            {
                var shifts = shift.SlotRoom.SurgeryShifts.Where(s => !s.IsDeleted).OrderBy(s => s.EstimatedStartDateTime).ToList();
                var shiftIndex = shifts.IndexOf(shift);
                var longerShiftIndex = shifts.IndexOf(longerShift);
                if (Math.Abs(shiftIndex - longerShiftIndex) == 1)
                {
                    return true;
                }
            }
            return false;
        }
        private List<SurgeryShift> GetAffectedShifts(int roomId, DateTime start, DateTime end)
        {
            var affectedRoom = _context.SlotRooms.Find(roomId);
            var shifts = affectedRoom.SurgeryShifts.Where(s =>
                !s.IsDeleted &&
                s.EstimatedStartDateTime.Value > DateTime.Now &&
                s.Status.Name.Equals("Preoperative", StringComparison.CurrentCultureIgnoreCase) &&
                (s.EstimatedStartDateTime.Value < end && s.EstimatedStartDateTime.Value >= start) ||
                (s.EstimatedEndDateTime.Value > start && s.EstimatedEndDateTime.Value <= end))
                .OrderBy(s => s.EstimatedStartDateTime)
                .ToList();
            return shifts;
        }
        #endregion

        #region Assign Ekip
        public List<SurgeryShift> GetScheduledSurgeryShifts(int dateNumber)
        {
            return _context.SurgeryShifts
                .Where(s => (s.EstimatedStartDateTime != null && s.EstimatedEndDateTime != null)
                && (UtilitiesDate.ConvertDateToNumber(s.EstimatedStartDateTime.Value) == dateNumber)
                && s.EkipId == null) //mm/dd/YYYY
                .OrderByDescending(s => s.ExpectedSurgeryDuration).ToList();
        }

        public List<Ekip> GetEkip()
        {
            return _context.Ekips.ToList();
        }

        public AssignSurgeryEkip FindShortesSumDurationAndAvailableEkipInScheduledDate(List<AssignSurgeryEkip> assignSurgeryEkips
                                                                            , DateTime estimateStart, DateTime estimateEnd)
        {
            // lấy những ca có giờ đụng với ca phẫu thuật cần assign ekip
            var shifts = _context.SurgeryShifts.Where(s =>
               !s.IsDeleted &&
               (s.EstimatedStartDateTime.Value < estimateStart && s.EstimatedEndDateTime.Value > estimateStart) ||
               (s.EstimatedStartDateTime.Value >= estimateStart && s.EstimatedStartDateTime.Value < estimateEnd) ||
               (s.EstimatedEndDateTime.Value > estimateStart && s.EstimatedEndDateTime.Value <= estimateEnd))
               .ToList();

            // lấy ra những ekip khả dụng trong thời gian ca phẫu thuật cần assign
            var availableEkips = new List<AssignSurgeryEkip>();
            foreach (var e in assignSurgeryEkips)
            {
                if (!shifts.Any(s => s.EkipId == e.EkipId))
                {
                    availableEkips.Add(e);
                }
            }

            //trả về ekip có thời gian làm việc ngắn nhất
            return availableEkips.Aggregate(
                    (shortestSurgery, currentSurgery) =>
                    (currentSurgery.SumDuration <= shortestSurgery.SumDuration) ? currentSurgery : shortestSurgery);

        }

        public void AssignSurgeryToEkip(int ekipId, SurgeryShift shift)
        {
            shift.EkipId = ekipId;
            _context.Update(shift);
            _context.SaveChanges();
        }

        public void AssignEkipByDate(int dateNumber)
        {
            var shifts = GetScheduledSurgeryShifts(dateNumber);

            var ekips = new List<AssignSurgeryEkip>();
            AssignSurgeryEkip assignSurgery;
            foreach (var e in GetEkip())
            {
                assignSurgery = new AssignSurgeryEkip()
                {
                    EkipId = e.Id,
                    SumDuration = e.SurgeryShifts.Sum(s => s.ExpectedSurgeryDuration),
                };
                ekips.Add(assignSurgery);
            }
             ;
            foreach (var shift in shifts)
            {
                // tìm ekip có số lượng thời gian làm việc thấp nhất và khả dụng
                assignSurgery = FindShortesSumDurationAndAvailableEkipInScheduledDate(ekips
                    , shift.EstimatedStartDateTime.Value
                    , shift.EstimatedEndDateTime.Value);

                // assign ca vào ekip
                AssignSurgeryToEkip(assignSurgery.EkipId, shift);

                // cộng thêm vào thời gian làm việc của ekip
                assignSurgery.SumDuration = assignSurgery.SumDuration + shift.ExpectedSurgeryDuration;
            }
        }

        public void AssignEkip()
        {
            var surgeries = _context.SurgeryShifts
                .Where(s => (s.EstimatedStartDateTime != null && s.EstimatedEndDateTime != null)
                        && s.EkipId == null);
            var minDate = UtilitiesDate.ConvertDateToNumber(surgeries.Min(s => s.EstimatedStartDateTime).Value);
            var maxDate = UtilitiesDate.ConvertDateToNumber(surgeries.Max(s => s.EstimatedStartDateTime).Value);

            for (int i = minDate; i <= maxDate; i++)
            {
                AssignEkipByDate(i);
            }
        }





        #endregion

        #region Statistic
        public List<StatisticViewModel> numShiftBySpec(DateTime start, DateTime end)
        {
            var specs = _context.Specialties.ToList();
            var rs = new List<StatisticViewModel>();
            foreach (var spec in specs)
            {
                var statistic = new StatisticViewModel();
                statistic.specialtyName = spec.Name;
                var catalogs = _context.SurgeryCatalogs.Where(s => s.SpecialtyId == spec.Id).ToList();
                var numbShift = 0;
                foreach (var catalog in catalogs)
                {
                    var surgeryShifts = _context.SurgeryShifts.Where(s => s.SurgeryCatalogId == catalog.Id
                                     && s.ActualStartDateTime.Value.Date >= start.Date && s.ActualEndDateTime.Value.Date <= end.Date).ToList();
                    numbShift = numbShift + surgeryShifts.Count;
                }
                statistic.number = numbShift;
                rs.Add(statistic);
            }
            return rs;
        }

        public List<RoomStatisticViewModel> getEfficientcyRoom(DateTime start, DateTime end)
        {
            double days = (end - start).TotalDays == 0 ? 1 : (end - start).TotalDays;
            var rooms = _context.SurgeryRooms.ToList();
            var rs = new List<RoomStatisticViewModel>();
            foreach (var room in rooms)
            {
                var statistic = new RoomStatisticViewModel();
                statistic.roomName = room.Name;
                var slots = _context.SlotRooms.Where(s => s.SurgeryRoomId == room.Id).ToList();
                double roomTime = 0;
                foreach (var slot in slots)
                {
                    double slotTime = 0;
                    var surgeryShifts = _context.SurgeryShifts.Where(s => s.SlotRoomId == slot.Id
                                         && s.ActualStartDateTime.Value.Date >= start.Date && s.ActualEndDateTime.Value.Date <= end.Date).ToList();
                    foreach (var shift in surgeryShifts)
                    {
                        System.TimeSpan time = shift.ActualEndDateTime.Value - shift.ActualStartDateTime.Value;
                        slotTime = slotTime + time.TotalHours;
                    }
                    roomTime = roomTime + slotTime;
                }
                statistic.number = roomTime / (7 * 2 * 8 * days);
                rs.Add(statistic);
            }
            return rs;
        }



        #endregion

    }
}