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
        private readonly string PRE_STATUS = "Preoperative";
        private readonly string POST_STATUS = "Postoperative";
        private readonly string INTRA_STATUS = "Intraoperative";
        private readonly string RECOVERY_STATUS = "Recovery";
        private readonly AppDbContext _context;
        StringBuilder notificationMakeSchedule = new StringBuilder();

        public SurgeryService(AppDbContext _context)
        {
            this._context = _context;
        }
        #region Status

        public bool SetPostoperativeStatus(int shiftId, string roomPost, string bedPost, string actualEndDateTime)
        {
            var shift = _context.SurgeryShifts.Find(shiftId);
            var status = _context.Statuses.Where(s => s.Name.Equals("Postoperative")).FirstOrDefault();
            if (shift != null)
            {
                shift.StatusId = status.Id;
                shift.PostRoomName = roomPost;
                shift.PostBedName = bedPost;
                shift.ActualEndDateTime = DateTime.ParseExact(actualEndDateTime, "yyyy-MM-dd HH:mm",
                                       System.Globalization.CultureInfo.InvariantCulture);
                _context.Update(shift);
                _context.SaveChanges();
                return true;
            }
            return false;
        }
        public bool SetIntraoperativeStatus(int shiftId, string actualStartDateTime)
        {
            var shift = _context.SurgeryShifts.Find(shiftId);
            var status = _context.Statuses.Where(s => s.Name.Equals("Intraoperative")).FirstOrDefault();
            if (shift != null)
            {
                shift.StatusId = status.Id;
                shift.ActualStartDateTime = DateTime.ParseExact(actualStartDateTime, "yyyy-MM-dd HH:mm",
                                       System.Globalization.CultureInfo.InvariantCulture);
                _context.Update(shift);
                _context.SaveChanges();
                return true;
            }
            return false;
        }
        public bool SetFinishedStatus(int shiftId)
        {
            var shift = _context.SurgeryShifts.Find(shiftId);
            var status = _context.Statuses.Where(s => s.Name.Equals("Finished")).FirstOrDefault();
            if (shift != null)
            {
                shift.StatusId = status.Id;
                _context.Update(shift);
                _context.SaveChanges();
                return true;
            }
            return false;
        }
        public int CheckPostStatus(int shiftId)
        {
            var shift = _context.SurgeryShifts.Find(shiftId);
            int result = 0;
            if (shift != null)
            {
                if (shift.Status.Name.Equals(INTRA_STATUS, StringComparison.CurrentCultureIgnoreCase))
                {
                    result = 1; //Hiện nút khi ca mổ sau thời gian phẫu thuật
                }
                else if (shift.Status.Name.Equals(POST_STATUS, StringComparison.CurrentCultureIgnoreCase))
                {
                    result = 2; //Disable nút khi đã set status
                }
            }
            return result;
        }
        public bool CheckRecoveryStatus(int shiftId)
        {
            var shift = _context.SurgeryShifts.Find(shiftId);
            if (shift != null)
            {
                if (shift.Status.Name.Equals(RECOVERY_STATUS, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        public bool RefreshSurgeryShift(int shiftId)
        {
            var surgeryShift = _context.SurgeryShifts.Find(shiftId);
            int roomId = surgeryShift.SurgeryRoomId.Value;
            var room = _context.SurgeryRooms.Find(roomId);
            var shiftInRoom = room.SurgeryShifts
                .Where(s => UtilitiesDate.ConvertDateToNumber(s.ScheduleDate.Value) == UtilitiesDate.ConvertDateToNumber(surgeryShift.ScheduleDate.Value))
                .Where(s => s.EstimatedStartDateTime >= surgeryShift.EstimatedEndDateTime).OrderBy(s => s.EstimatedStartDateTime).ToList();
            if (shiftInRoom.Count >= 1) // 2 trở lên mới có ảnh hưởng tới những thằng sau
            {
                try
                {
                    if (surgeryShift.ActualEndDateTime > shiftInRoom.First().EstimatedStartDateTime)
                    {
                        var changeShift = _context.SurgeryShifts.Find(shiftInRoom.First().Id);
                        changeShift.EstimatedStartDateTime = surgeryShift.ActualEndDateTime;
                        changeShift.EstimatedEndDateTime = changeShift.EstimatedStartDateTime + TimeSpan.FromHours(changeShift.ExpectedSurgeryDuration);
                        _context.SaveChanges();
                        if (shiftInRoom.Count >= 2)
                        {
                            for (int i = 0; i < shiftInRoom.Count - 1; i++)
                            {
                                //shiftInRoom = GetRealtimeShiftByRoom(room, surgeryShift);
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
                }
                catch (DbUpdateException)
                {
                    return false;
                }


            }
            return true;
        }

        #region Make Schedule
        public StringBuilder MakeScheduleList()
        {
            var shifts = GetSurgeryShiftsNoSchedule();
            foreach (var shift in shifts)
            {
                int dayNumber = UtilitiesDate.ConvertDateToNumber(shift.ScheduleDate);
                var availableSlotRooms = GetAvailableSlotRoom(dayNumber);

                // TODO: Lấy list phòng trống (chưa có ca phẫu thuật nào trong ngày)
                int roomEmptyId = GetEmptyRoomForDate(dayNumber);

                //// Lấy khoảng thời gian sau thời gian confirm
                //var roomList = availableSlotRooms.Where(s => s.StartDateTime > shift.ConfirmDate).ToList();

                //TODO: Trường hợp mổ theo chương trình, biến = true
                if (shift.IsNormalSurgeryTime)
                {
                    // TODO: 1.1. Nếu có thì thẳng tay add vào
                    if (roomEmptyId != 0)
                    {
                        DateTime startEstimatedTime = shift.ScheduleDate + startAMWorkingHour;
                        DateTime endEstimatedTime = startEstimatedTime + TimeSpan.FromHours(shift.ExpectedSurgeryDuration);
                        InsertDateTimeToSurgeryShift(shift.SurgeryShiftId, startEstimatedTime, endEstimatedTime, roomEmptyId);
                    }
                    else  // TODO: 1.2. Nếu ko thì tìm các khoảng trống hợp lệ của từng phòng
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
                }
                else //TODO: Trường hợp mổ theo thời gian chỉ định
                {
                    if (roomEmptyId != 0)
                    {
                        InsertDateTimeToSurgeryShift
                                (shift.SurgeryShiftId, shift.ProposedStartDateTime.Value, shift.ProposedEndDateTime.Value, roomEmptyId);
                    }
                    else
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
                            int roomProposed = GetAvailableRoomForProposedTime(new EmerSurgeryShift() { StartTime = shift.ProposedStartDateTime.Value, EndTime = shift.ProposedEndDateTime.Value });
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
                    if (!shift.IsNormalSurgeryTime)
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
            return notificationMakeSchedule;
        }
        #endregion


        #region GetAvailableRoom
        public List<AvailableRoomViewModel> GetAvailableSlotRoom(int dateNumber)
        {
            // TODO: Lấy khoảng thời gian của ConfirmDate, sau khi confirm thì lên lịch ngay
            var rooms = _context.SurgeryRooms.ToList();
            var availableRooms = new List<AvailableRoomViewModel>();
            foreach (var room in rooms)
            {
                // TODO: Lấy các ca phẫu thuật theo từng phòng, sắp xếp tăng dần theo EstimatedStartDateTime
                var result = room.SurgeryShifts
                    .Where(s => UtilitiesDate.ConvertDateToNumber(s.ScheduleDate.Value) == dateNumber
                    && s.IsAvailableMedicalSupplies == true
                    && s.EstimatedStartDateTime.Value.TimeOfDay >= startAMWorkingHour
                    && s.EstimatedEndDateTime.Value.TimeOfDay <= endPMWorkingHour)
                    .OrderBy(s => s.EstimatedStartDateTime).ToList();
                if (result.Count > 0)
                {
                    var start = result.First().ScheduleDate.Value + startAMWorkingHour;
                    var end = result.First().EstimatedStartDateTime.Value;
                    //nếu có actual
                    if (result.First().ActualStartDateTime != null)
                    {
                        end = result.First().ActualStartDateTime.Value;
                    }
                    // Lấy khoảng ở ngoài các ca mổ [7h - 17h]
                    if (start != end)
                    { // Lấy khoảng từ 7h ->
                        AddAvailableSlotExceptBreakTime(availableRooms, start, end, room.Id);
                    }

                    start = result.Last().EstimatedEndDateTime.Value;
                    end = result.Last().ScheduleDate.Value + endPMWorkingHour;
                    //nếu có actual
                    if (result.Last().ActualEndDateTime != null)
                    {
                        end = result.Last().ActualEndDateTime.Value;
                    }
                    if (start != end)
                    { // Lấy khoảng từ -> 17h
                        AddAvailableSlotExceptBreakTime(availableRooms, start, end, room.Id);

                    }
                    if (result.Count != 1)
                    {
                        //======= Lấy khoảng ở giữa các ca mổ==========
                        for (int i = 0; i < result.Count - 1; i++)
                        {
                            //if (result.Last().EstimatedEndDateTime.Value.TimeOfDay <= endPMWorkingHour)
                            //{
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
                                AddAvailableSlotExceptBreakTime(availableRooms, start, end, room.Id);
                            }
                            //}
                        }
                    }
                }

            }
            return availableRooms.ToList();
        }
        public int GetAvailableRoomForProposedTime(EmerSurgeryShift emerShift)
        {
            var parentRooms = _context.SurgeryRooms.ToList();
            var childRoomIds = new List<int>();
            foreach (var room in parentRooms)
            {
                var roomByDate = room.SurgeryShifts.Where(s => s.ScheduleDate == emerShift.StartTime.Date).ToList();
                foreach (var shift in roomByDate)
                {
                    if ((emerShift.StartTime >= shift.EstimatedStartDateTime && emerShift.StartTime < shift.EstimatedEndDateTime)
                        || (emerShift.EndTime > shift.EstimatedStartDateTime && emerShift.EndTime <= shift.EstimatedEndDateTime))
                    {
                        childRoomIds.Add(shift.SurgeryRoomId.Value);
                        break;
                    }
                }
            }
            ICollection<int> roomIds = parentRooms.Where(p => !childRoomIds.Contains(p.Id)).Select(s => s.Id).ToList();
            return roomIds.FirstOrDefault();
        }
        public int GetEmptyRoomForDate(int scheduleDateNumber)
        {
            var rooms = _context.SurgeryRooms.ToList();
            ICollection<int> roomIds = new List<int>();
            foreach (var room in rooms)
            {
                if (!room.SurgeryShifts.Where(s => UtilitiesDate.ConvertDateToNumber(s.ScheduleDate.Value) == scheduleDateNumber).Any())
                {
                    roomIds.Add(room.Id);
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
            else if (start.TimeOfDay < endAMWorkingHour && end.TimeOfDay > startPMWorkingHour) //Add bình thường
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

        #region Add Schedule
        public bool AddEmergencyShift(EmerSurgeryShift emerShift)
        {
            var availableRoomId = GetAvailableRoomForProposedTime(emerShift);
            if (availableRoomId != 0)
            {
                try
                {
                    var insertedShift = new SurgeryShift();
                    insertedShift.EstimatedStartDateTime = emerShift.StartTime;
                    insertedShift.EstimatedEndDateTime = emerShift.EndTime;
                    insertedShift.ScheduleDate = emerShift.StartTime.Date;
                    insertedShift.ConfirmDate = DateTime.Now;
                    insertedShift.IsAvailableMedicalSupplies = true;
                    insertedShift.SurgeryRoomId = availableRoomId;
                    insertedShift.StatusId = _context.Statuses.Where(s => s.Name == PRE_STATUS).FirstOrDefault().Id;
                    _context.SurgeryShifts.Add(insertedShift);
                    _context.SaveChanges();
                    return true;
                }
                catch (DbUpdateException)
                {
                    return false;
                }
            }
            return false;
        }
        public void InsertDateTimeToSurgeryShift(int surgeryId, DateTime startTime, DateTime endTime, int roomId)
        {
            var surgeryShift = _context.SurgeryShifts.Find(surgeryId);
            surgeryShift.EstimatedStartDateTime = startTime;
            surgeryShift.EstimatedEndDateTime = endTime;
            surgeryShift.SurgeryRoomId = roomId;
            _context.SaveChanges();
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
                    Name = room.Name
                });
            }
            return results;
        }

        // TODO: Xem lịch theo ngày
        public ICollection<SurgeryShiftViewModel> GetSurgeryShiftsByRoomAndDate(int surgeryRoomId, int dateNumber)
        {
            var results = new List<SurgeryShiftViewModel>();
            foreach (var shift in _context.SurgeryShifts
                .Where(s => (s.EstimatedStartDateTime != null && s.EstimatedEndDateTime != null)
                && (UtilitiesDate.ConvertDateToNumber(s.EstimatedStartDateTime.Value) == dateNumber) //mm/dd/YYYY
                && (s.SurgeryRoomId == surgeryRoomId))
                .OrderBy(s => s.EstimatedStartDateTime))
            {
                //try
                //{
                if (shift.SurgeryCatalog != null && shift.Patient != null)
                {
                    results.Add(new SurgeryShiftViewModel()
                    {
                        Id = shift.Id,
                        CatalogName = shift.SurgeryCatalog.Name,
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
                        EstimatedStartDateTime = shift.EstimatedStartDateTime.Value,
                        EstimatedEndDateTime = shift.EstimatedEndDateTime.Value,
                        ActualStartDateTime = shift.ActualStartDateTime,
                        ActualEndDateTime = shift.ActualEndDateTime,
                        StatusName = _context.Statuses.Find(shift.StatusId).Name,
                        SurgeonNames = shift.SurgeryShiftSurgeons.Select(s => s.Surgeon.FullName).ToList()

                    });
                }
                //}
                //catch (Exception)
                //{

                //}

            }
            return results;
        }

        //TODO: Lấy danh sách ca mổ chưa lên lịch theo độ ưu tiên và ngày (bao gồm chỉ định và ko chỉ định)
        public ICollection<ScheduleViewModel> GetSurgeryShiftsNoSchedule()
        {
            var result = _context.SurgeryShifts
                .Where(s => (s.EstimatedStartDateTime == null) && s.EstimatedEndDateTime == null
                && s.IsAvailableMedicalSupplies == true
                && s.ProposedStartDateTime == null && s.ProposedEndDateTime == null
                && s.SurgeryRoomId == null && s.ConfirmDate != null
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
                    ProposedStartDateTime = shift.ProposedStartDateTime,
                    ProposedEndDateTime = shift.ProposedEndDateTime,
                    IsNormalSurgeryTime = shift.IsNormalSurgeryTime,
                    ConfirmDate = shift.ConfirmDate.Value,
                    ScheduleDate = shift.ScheduleDate.Value,
                    ExpectedSurgeryDuration = shift.ExpectedSurgeryDuration,
                    PriorityNumber = shift.PriorityNumber
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
                .Where(s => (s.IsAvailableMedicalSupplies == true) && (s.SurgeryRoomId == null)
                && s.EstimatedStartDateTime == null && s.EstimatedEndDateTime == null
                && s.ProposedStartDateTime != null && s.ProposedEndDateTime != null)
                .OrderBy(s => s.ExpectedSurgeryDuration)
                .OrderBy(s => s.PriorityNumber)
                .OrderBy(s => s.ProposedStartDateTime).ToList();
            foreach (var index in surgeryShifts)
            {
                shifts.Add(new ScheduleViewModel()
                {
                    SurgeryShiftId = index.Id,
                    ProposedStartDateTime = index.ProposedStartDateTime,
                    ProposedEndDateTime = index.ProposedEndDateTime,
                    IsNormalSurgeryTime = index.IsNormalSurgeryTime,
                    ConfirmDate = index.ConfirmDate.Value,
                    ScheduleDate = index.ScheduleDate.Value,
                    ExpectedSurgeryDuration = index.ExpectedSurgeryDuration,
                    PriorityNumber = index.PriorityNumber
                });
            }
            return shifts;
        }

        public SurgeryShiftDetailViewModel GetShiftDetail(int shiftId)
        {
            var shift = _context.SurgeryShifts.Find(shiftId);
            if (shift != null)
            {
                var result = new SurgeryShiftDetailViewModel()
                {
                    Id = shift.Id,
                    PatientName = shift.Patient.FullName,
                    Gender = shift.Patient.Gender == -1 ? "Nam" : "Nữ",
                    Age = DateTime.Now.Year - shift.Patient.YearOfBirth,
                    Speciality = shift.SurgeryCatalog.Speciality.Name,
                    SurgeryName = shift.SurgeryCatalog.Name,
                    SurgeryType = shift.SurgeryCatalog.Type,
                    StartTime = shift.EstimatedStartDateTime,
                    EndTime = shift.EstimatedEndDateTime,
                    ActualStartTime = shift.ActualStartDateTime,
                    ActualEndTime = shift.ActualEndDateTime,
                    //EkipMembers = shift.Ekip.Members.Select(m => new EkipMemberViewModel() { Name = m.Name, WorkJob = m.WorkJob }).ToList(),
                    Procedure = shift.SurgeryCatalog.Procedure
                };
                return result;
            }
            return null;
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
                    shift.SurgeryRoomId = newShift.RoomId;
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

        public List<int> GetAvailableRoom(DateTime start, DateTime end, bool forcedChange)
        {
            if (!IsValidTime(start, end) && !forcedChange)
            {
                return null;
            }
            var rooms = _context.SurgeryRooms.Where(r => !r.IsDeleted);
            var roomId = new List<int>();
            foreach (var room in rooms)
            {
                var shifts = room.SurgeryShifts.Where(s =>
                    !s.IsDeleted &&
                    s.EstimatedStartDateTime.Value > DateTime.Now &&
                    s.Status.Name.Equals("Preoperative", StringComparison.CurrentCultureIgnoreCase))
                    .ToList();

                if (shifts.Any())
                {
                    var affectedShifts = shifts.Where(s =>
                                                    (s.EstimatedStartDateTime.Value >= start && s.EstimatedStartDateTime.Value < end)
                                                    || (s.EstimatedEndDateTime.Value > start && s.EstimatedEndDateTime.Value <= end))
                                                    .ToList();
                    if (!affectedShifts.Any())
                    {
                        roomId.Add(room.Id);
                    }
                }
                else
                {
                    roomId.Add(room.Id);
                }

            }
            return roomId.OrderBy(c => c).ToList();
        }

        public List<AvailableRoomViewModel> GetAvailableRoom(int hour, int minute, int? longerShiftId = null, List<int> shiftIds = null)
        {
            var expectedTimeSpan = new TimeSpan(hour, minute, 0);
            var results = new List<AvailableRoomViewModel>();
            var rooms = _context.SurgeryRooms.Where(r => !r.IsDeleted).ToList();
            foreach (var room in rooms)
            {
                var shifts = new List<SurgeryShift>();
                if (longerShiftId.HasValue && shiftIds != null)
                {
                    shifts = room.SurgeryShifts.Where(s =>
                        !s.IsDeleted &&
                        s.EstimatedStartDateTime.Value > DateTime.Now &&
                        s.Status.Name.Equals("Preoperative", StringComparison.CurrentCultureIgnoreCase))
                        .ToList();
                    if (room.Id == _context.SurgeryShifts.Find(longerShiftId).SurgeryRoomId)
                    {
                        shifts.Add(_context.SurgeryShifts.Find(longerShiftId));
                    }
                    foreach (var shiftId in shiftIds)
                    {
                        if (room.Id == _context.SurgeryShifts.Find(shiftId).SurgeryRoomId)
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
            //var transaction = _context.Database.BeginTransaction();
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
                    if (shift.SurgeryRoomId != longerShift.SurgeryRoomId)
                    {
                        var longerShiftRoomId = longerShift.SurgeryRoomId.Value;
                        var shiftRoomId = shift.SurgeryRoomId.Value;

                        #region Swap and Disable Longer Shift
                        //Shift Schedule Change VMs
                        var shiftChangeVM = new ShiftScheduleChangeViewModel()
                        {
                            Id = shift.Id,
                            RoomId = longerShiftRoomId,
                            EstimatedStartDateTime = longerShift.EstimatedStartDateTime.Value,
                            EstimatedEndDateTime = longerShift.EstimatedStartDateTime.Value + duration
                        };
                        var longerShiftChangeVM = new ShiftScheduleChangeViewModel()
                        {
                            Id = longerShift.Id,
                            RoomId = shiftRoomId,
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
                                var roomIds = GetAvailableRoom(affectedShift.EstimatedStartDateTime.Value, affectedShift.EstimatedEndDateTime.Value, false);
                                if (roomIds.Any())
                                {
                                    var resolvedShift = new AffectedShiftResultViewModel()
                                    {
                                        ShiftId = affectedShift.Id,
                                        OldRoomName = affectedShift.SurgeryRoom.Name,
                                    };
                                    result.Succeed = ChangeSchedule(new ShiftScheduleChangeViewModel()
                                    {
                                        Id = affectedShift.Id,
                                        RoomId = roomIds.FirstOrDefault(),
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
                                    var rooms = GetAvailableRoom(affectedShiftDuration.Hours, affectedShiftDuration.Minutes, longerShift.Id, affectedShiftIds);
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
                                            OldRoomName = affectedShift.SurgeryRoom.Name,
                                            OldStart = affectedShift.EstimatedStartDateTime.Value,
                                            OldEnd = affectedShift.EstimatedEndDateTime.Value
                                        };

                                        result.Succeed = ChangeSchedule(new ShiftScheduleChangeViewModel()
                                        {
                                            Id = affectedShift.Id,
                                            RoomId = rooms.FirstOrDefault().RoomId,
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
                        if (IsNextTo(shift, longerShift))
                        {
                            var longerShiftRoomId = longerShift.SurgeryRoomId.Value;
                            var shiftRoomId = shift.SurgeryRoomId.Value;

                            //Shift Schedule Change VMs
                            var longerShiftChangeVM = new ShiftScheduleChangeViewModel()
                            {
                                Id = longerShift.Id,
                                RoomId = shiftRoomId,
                                EstimatedStartDateTime = shift.EstimatedStartDateTime.Value,
                                EstimatedEndDateTime = shift.EstimatedStartDateTime.Value + longerDuration
                            };
                            var shiftChangeVM = new ShiftScheduleChangeViewModel()
                            {
                                Id = shift.Id,
                                RoomId = longerShiftRoomId,
                                EstimatedStartDateTime = longerShiftChangeVM.EstimatedEndDateTime,
                                EstimatedEndDateTime = longerShiftChangeVM.EstimatedEndDateTime + duration
                            };


                            var swapResult = ChangeSchedule(shiftChangeVM);
                            swapResult = ChangeSchedule(longerShiftChangeVM);
                            result.Succeed = swapResult;
                            return result;
                        }
                        else
                        {
                            var longerShiftRoomId = longerShift.SurgeryRoomId.Value;
                            var shiftRoomId = shift.SurgeryRoomId.Value;

                            #region Swap and Disable Longer Shift
                            //Shift Schedule Change VMs
                            var shiftChangeVM = new ShiftScheduleChangeViewModel()
                            {
                                Id = shift.Id,
                                RoomId = longerShiftRoomId,
                                EstimatedStartDateTime = longerShift.EstimatedStartDateTime.Value,
                                EstimatedEndDateTime = longerShift.EstimatedStartDateTime.Value + duration
                            };
                            var longerShiftChangeVM = new ShiftScheduleChangeViewModel()
                            {
                                Id = longerShift.Id,
                                RoomId = shiftRoomId,
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
                                    var roomIds = GetAvailableRoom(affectedShift.EstimatedStartDateTime.Value, affectedShift.EstimatedEndDateTime.Value, false);
                                    if (roomIds.Any())
                                    {
                                        var resolvedShift = new AffectedShiftResultViewModel()
                                        {
                                            ShiftId = affectedShift.Id,
                                            OldRoomName = affectedShift.SurgeryRoom.Name,
                                        };
                                        result.Succeed = ChangeSchedule(new ShiftScheduleChangeViewModel()
                                        {
                                            Id = affectedShift.Id,
                                            RoomId = roomIds.FirstOrDefault(),
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
                                        var rooms = GetAvailableRoom(affectedShiftDuration.Hours, affectedShiftDuration.Minutes, longerShift.Id, affectedShiftIds);
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
                                                OldRoomName = affectedShift.SurgeryRoom.Name,
                                                OldStart = affectedShift.EstimatedStartDateTime.Value,
                                                OldEnd = affectedShift.EstimatedEndDateTime.Value
                                            };

                                            result.Succeed = ChangeSchedule(new ShiftScheduleChangeViewModel()
                                            {
                                                Id = affectedShift.Id,
                                                RoomId = rooms.FirstOrDefault().RoomId,
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
                        RoomId = longerShift.SurgeryRoomId.Value,
                        EstimatedStartDateTime = longerShift.EstimatedStartDateTime.Value,
                        EstimatedEndDateTime = longerShift.EstimatedEndDateTime.Value
                    };

                    var longerShiftChangeVM = new ShiftScheduleChangeViewModel()
                    {
                        Id = longerShift.Id,
                        RoomId = shift.SurgeryRoomId.Value,
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
                    RoomId = longerShift.SurgeryRoomId.Value,
                    EstimatedStartDateTime = longerShift.EstimatedStartDateTime.Value,
                    EstimatedEndDateTime = longerShift.EstimatedStartDateTime.Value + duration
                });//Shift
                ChangeSchedule(new ShiftScheduleChangeViewModel()
                {
                    Id = longerShift.Id,
                    RoomId = shift.SurgeryRoomId.Value,
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
                        RoomId = roomId,
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
                        var roomIds = GetAvailableRoom(affectedShift.EstimatedStartDateTime.Value, affectedShift.EstimatedEndDateTime.Value, false);
                        if (roomIds.Any())
                        {
                            var resolvedShift = new AffectedShiftResultViewModel()
                            {
                                ShiftId = affectedShift.Id,
                                OldRoomName = affectedShift.SurgeryRoom.Name,
                            };
                            result.Succeed = ChangeSchedule(new ShiftScheduleChangeViewModel()
                            {
                                Id = affectedShift.Id,
                                RoomId = roomIds.FirstOrDefault(),
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
                            var rooms = GetAvailableRoom(affectedShiftDuration.Hours, affectedShiftDuration.Minutes, shift.Id, affectedShiftIds);
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
                                    OldRoomName = affectedShift.SurgeryRoom.Name,
                                    OldStart = affectedShift.EstimatedStartDateTime.Value,
                                    OldEnd = affectedShift.EstimatedEndDateTime.Value
                                };

                                result.Succeed = ChangeSchedule(new ShiftScheduleChangeViewModel()
                                {
                                    Id = affectedShift.Id,
                                    RoomId = rooms.FirstOrDefault().RoomId,
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
                var roomIds = GetAvailableRoom(shift.EstimatedStartDateTime.Value, shift.EstimatedEndDateTime.Value, false);
                if (roomIds.Any(r => r == roomId))
                {
                    var changeRoomVM = new ShiftScheduleChangeViewModel()
                    {
                        RoomId = roomId,
                        Id = shiftId,
                        EstimatedStartDateTime = shift.EstimatedStartDateTime.Value,
                        EstimatedEndDateTime = shift.EstimatedEndDateTime.Value,
                    };
                    result.Succeed = ChangeSchedule(changeRoomVM);
                }
                else
                {
                    var affectedShiftDuration = shift.EstimatedEndDateTime.Value - shift.EstimatedStartDateTime.Value;
                    var rooms = GetAvailableRoom(affectedShiftDuration.Hours, affectedShiftDuration.Minutes, shift.Id);

                    if (rooms.Any(r => r.RoomId == roomId))
                    {
                        var newSchedule = rooms.Where(r => r.RoomId == roomId).OrderBy(r => r.StartDateTime).FirstOrDefault();
                        var changeRoomVM = new ShiftScheduleChangeViewModel()
                        {
                            RoomId = roomId,
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
            var rooms = _context.SurgeryRooms.Where(r => !r.IsDeleted);
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
            if (longerDuration < duration)
            {
                tempShift = shift;
                shift = longerShift;
                longerShift = tempShift;

                tempDuration = duration;
                duration = longerDuration;
                longerDuration = tempDuration;
                return true;
            }
            else if (longerDuration > duration)
            {
                return true;
            }
            return false;
        }
        private List<SurgeryShift> GetAffectedShifts(SurgeryShift longerShift, SurgeryShift shift)
        {
            var affectedRoom = longerShift.SurgeryRoom;
            var shifts = affectedRoom.SurgeryShifts.Where(s =>
                !s.IsDeleted &&
                s.EstimatedStartDateTime.Value > DateTime.Now &&
                s.Status.Name.Equals("Preoperative", StringComparison.CurrentCultureIgnoreCase) &&
                s.Id != longerShift.Id &&
                s.EstimatedStartDateTime.Value < longerShift.EstimatedEndDateTime.Value &&
                s.EstimatedStartDateTime.Value >= longerShift.EstimatedStartDateTime.Value)
                .ToList();
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
                if (start <= maximumStart)
                {
                    return true;
                }
            }
            return false;
        }
        private bool IsNextTo(SurgeryShift shift, SurgeryShift longerShift)
        {
            if (shift.SurgeryRoomId == longerShift.SurgeryRoomId)
            {
                var shifts = shift.SurgeryRoom.SurgeryShifts.Where(s => !s.IsDeleted).OrderBy(s => s.EstimatedStartDateTime).ToList();
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
            var affectedRoom = _context.SurgeryRooms.Find(roomId);
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
    }
}