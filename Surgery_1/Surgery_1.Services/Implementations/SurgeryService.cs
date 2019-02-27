using Microsoft.EntityFrameworkCore;
using Surgery_1.Data.Context;
using Surgery_1.Data.Entities;
using Surgery_1.Data.ViewModels;
using Surgery_1.Repositories.Interfaces;
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

        private readonly AppDbContext _context;

        public SurgeryService(AppDbContext _context)
        {
            this._context = _context;
        }
        public bool SetPostoperativeStatus(int shiftId)
        {
<<<<<<< HEAD
            var result = GetSurgeryShiftsNoSchedule();
            foreach (var index in result)
            {
                MakeSchedule(index);
            }

            //var result = GetSurgeryShiftsNoSchedule().First();
            ////foreach (var index in result)
            ////{
            //MakeSchedule(result);
            ////}
=======
            var shift = _context.SurgeryShifts.Find(shiftId);
            if (shift != null)
            {
                shift.StatusId = 3;
                _context.Update(shift);
                _context.SaveChanges();
                return true;
            }
            return false;
>>>>>>> e17416a75e887b15ffd736100cbfec5fdf47286b
        }
        public void MakeScheduleList()
        {
            var shifts = GetSurgeryShiftsNoSchedule();
            foreach (var shift in shifts)
            {
                int dayNumber = UtilitiesDate.ConvertDateToNumber(shift.ScheduleDate);
                string dayString = UtilitiesDate.ConvertDateToString(shift.ScheduleDate);
                var availableSlotRooms = GetAvailableSlotRoom(dayNumber);
                int roomEmptyId = GetEmptyRoomForDate(dayString);

                // TODO: Xử lý phòng trống

                //// TODO: Lấy khoảng thời gian sau thời gian confirm
                //var roomList = availableSlotRooms.Where(s => s.StartDateTime > shift.ConfirmDate).ToList();se
                if (shift.ProposedStartDateTime == null && shift.ProposedEndDateTime == null)
                {
                    if (roomEmptyId != 0)
                    {
                        DateTime startEstimatedTime = shift.ScheduleDate + TimeSpan.FromHours(ConstantVariable.StartAMWorkingHour);
                        DateTime endEstimatedTime = startEstimatedTime + TimeSpan.FromHours(shift.ExpectedSurgeryDuration);
                        InsertDateTimeToSurgeryShift(shift.SurgeryShiftId, startEstimatedTime, endEstimatedTime, roomEmptyId);
                    }
                    else
                    {
                        var room = availableSlotRooms.Where(s => s.ExpectedSurgeryDuration >= shift.ExpectedSurgeryDuration)
                                    .OrderBy(s => s.StartDateTime).OrderBy(s => s.ExpectedSurgeryDuration).FirstOrDefault();
                        if (room != null)
                        {
                            var endTime = room.StartDateTime + TimeSpan.FromHours(shift.ExpectedSurgeryDuration);
                            InsertDateTimeToSurgeryShift(shift.SurgeryShiftId, room.StartDateTime, endTime, room.RoomId);
                        }
                    }
<<<<<<< HEAD
                    else if (endEstimatedTime >= scheduleViewModel.EndAMWorkingHour)
                    {// Nếu thời gian ước tính nằm trong giờ nghỉ trưa thì lấy thời gian đầu của buổi chiều + ExpectedSurgeryDuration
                        endEstimatedTime = scheduleViewModel.StartPMWorkingHour + hour;
=======
                }
                else // Có thời gian chỉ định
                {
                    if (roomEmptyId != 0)
                    {
>>>>>>> e17416a75e887b15ffd736100cbfec5fdf47286b
                        InsertDateTimeToSurgeryShift
                                (shift.SurgeryShiftId, shift.ProposedStartDateTime.Value, shift.ProposedEndDateTime.Value, roomEmptyId);
                    }
                    else
                    {
                        var room = availableSlotRooms.Where(s => s.StartDateTime <= shift.ProposedStartDateTime
                                && s.EndDateTime >= shift.ProposedEndDateTime)
                               .OrderByDescending(s => s.StartDateTime)
                               .OrderBy(s => s.EndDateTime)
                               .OrderBy(s => s.ExpectedSurgeryDuration)
                               .FirstOrDefault();
                        if (room != null)
                        {
                            InsertDateTimeToSurgeryShift
                                (shift.SurgeryShiftId, shift.ProposedStartDateTime.Value, shift.ProposedEndDateTime.Value, room.RoomId);
                        }
                        else
                        {
                            int roomProposed = GetAvailableRoomForProposedTime(shift.ProposedStartDateTime.Value, shift.ProposedEndDateTime.Value);
                            // TODO: Thông báo ko tìm ra
                            if (roomProposed != 0)
                            {
                                InsertDateTimeToSurgeryShift
                                (shift.SurgeryShiftId, shift.ProposedStartDateTime.Value, shift.ProposedEndDateTime.Value, roomProposed);
                            }
                        }
                    }
                }
            }

            // TODO: Xử lý qua ngày
            shifts = GetSurgeryShiftsNoSchedule();
            if (shifts.Count != 0)
            {
                foreach (var shift in shifts)
                {
                    if (shift.ProposedStartDateTime == null && shift.ProposedEndDateTime == null)
                    {
                        var item = _context.SurgeryShifts.Find(shift.SurgeryShiftId);
                        if (shift.ScheduleDate.AddDays(1).DayOfWeek.Equals(ConstantVariable.DAYOFF))
                        {
                            item.ScheduleDate = shift.ScheduleDate.AddDays(2);
                            _context.SaveChanges();
                        }
                        else
                        {
                            item.ScheduleDate = shift.ScheduleDate.AddDays(1);
                            _context.SaveChanges();
                        }
                    }
                    else
                    {
                        var item = _context.SurgeryShifts.Find(shift.SurgeryShiftId);
                        item.ProposedStartDateTime = null;
                        item.ProposedEndDateTime = null;
                        if (shift.ScheduleDate.AddDays(1).DayOfWeek.Equals(ConstantVariable.DAYOFF))
                        {
                            item.ScheduleDate = shift.ScheduleDate.AddDays(2);
                            _context.SaveChanges();
                        }
                        else
                        {
                            item.ScheduleDate = shift.ScheduleDate.AddDays(1);
                            _context.SaveChanges();
                        }
                    }

                }
                MakeScheduleList();
            }
        }

        public void MakeScheduleByProposedTime()
        {
            var surgeryShifts = GetSurgeryShiftNoScheduleByProposedTime();
            foreach (var shift in surgeryShifts)
            {
                if (shift.ProposedStartDateTime != null && shift.ProposedEndDateTime != null)
                {
                    int roomId = GetAvailableRoomForProposedTime(shift.ProposedStartDateTime.Value, shift.ProposedEndDateTime.Value);
                    if (roomId == 0) // Thông báo ko lên lịch đc
                    {

                    }
                    else
                    {
                        InsertDateTimeToSurgeryShift(shift.SurgeryShiftId, shift.ProposedStartDateTime.Value, shift.ProposedEndDateTime.Value, roomId);
                    }
                }
            }
        }

        public int GetAvailableRoomForProposedTime(DateTime startTime, DateTime endTime)
        {

            var parentRoomIds = _context.SurgeryRooms.Select(s => s.Id).ToList();
            // TODO: Lấy những phòng không hợp lệ (điều có khoảng thời gian trùng với startTime và endTime)
            var childRoomIds = _context.SurgeryShifts
                .Where(s => (s.EstimatedStartDateTime >= startTime && s.EstimatedStartDateTime < endTime)
                || (s.EstimatedEndDateTime > startTime && s.EstimatedEndDateTime <= endTime))
                .Select(s => s.SurgeryRoomId).ToList();
            // Loại những phòng không hợp lệ
            ICollection<int> roomIds = parentRoomIds.Where(p => !childRoomIds.Contains(p)).ToList();
            return roomIds.FirstOrDefault();
        }     

        #region GetAvailableRoom
        public List<AvailableRoomViewModel> GetAvailableSlotRoom(int dateNumber)
        {
            // TODO: Lấy khoảng thời gian của ConfirmDate, sau khi confirm thì lên lịch ngay

            var rooms = _context.SurgeryRooms.ToList();
            var availableRooms = new List<AvailableRoomViewModel>();
            foreach (var room in rooms)
            {
                // TODO: Lấy các ca phẫu thuật theo phòng, sắp xếp giảm dần theo thời gian kết thúc
                var result = room.SurgeryShifts
                    .Where(s => UtilitiesDate.ConvertDateToNumber(s.ScheduleDate.Value) == dateNumber
                    && s.IsAvailableMedicalSupplies == true)
                    .OrderBy(s => s.EstimatedStartDateTime).ToList();
                if (result.Count > 0)
                {
                    if (result.Count == 1) // TODO: Lấy thời gian End + Duration
                    {
                        var start = result.First().ScheduleDate.Value + TimeSpan.FromHours(ConstantVariable.StartAMWorkingHour);
                        var end = result.First().EstimatedStartDateTime.Value;
                        if (start != end) // Bỏ trường hợp ca mổ start lúc 7:00
                        {
                            availableRooms.Add(new AvailableRoomViewModel
                            {
                                RoomId = room.Id,
                                StartDateTime = start,
                                EndDateTime = end,
                                ExpectedSurgeryDuration = (end - start).TotalHours
                            });
                        }


                        start = result.ElementAt(0).EstimatedEndDateTime.Value;
                        end = result.ElementAt(0).ScheduleDate.Value + TimeSpan.FromHours(ConstantVariable.EndPMWorkingHour);
                        if (start != end) //Bỏ trường hợp ca mổ end lúc 17:00
                        {
                            availableRooms.Add(new AvailableRoomViewModel
                            {
                                RoomId = room.Id,
                                StartDateTime = start,
                                EndDateTime = end,
                                ExpectedSurgeryDuration = (end - start).TotalHours
                            });
                        }

                    }
                    else // TODO: Số ca trong 1 phòng lớn >= 2                
                    {
                        if (result.ElementAt(0).EstimatedStartDateTime.Value.TimeOfDay != TimeSpan.FromHours(ConstantVariable.StartAMWorkingHour))
                        { // Lấy khoảng từ 7h ->
                            var start = result.First().ScheduleDate.Value + TimeSpan.FromHours(ConstantVariable.StartAMWorkingHour);
                            var end = result.First().EstimatedStartDateTime.Value;
                            availableRooms.Add(new AvailableRoomViewModel
                            {
                                RoomId = room.Id,
                                StartDateTime = start,
                                EndDateTime = end,
                                ExpectedSurgeryDuration = (end - start).TotalHours
                            });
                        }

                        if (result.Last().EstimatedEndDateTime.Value.TimeOfDay != TimeSpan.FromHours(ConstantVariable.EndPMWorkingHour))
                        { // Lấy khoảng từ -> 17h
                            var start = result.Last().EstimatedEndDateTime.Value;
                            var end = result.First().ScheduleDate.Value + TimeSpan.FromHours(ConstantVariable.EndPMWorkingHour);
                            availableRooms.Add(new AvailableRoomViewModel
                            {
                                RoomId = room.Id,
                                StartDateTime = start,
                                EndDateTime = end,
                                ExpectedSurgeryDuration = (end - start).TotalHours
                            });
                        }

                        for (int i = 0; i < result.Count - 1; i++)
                        {
                            if (result.Last().EstimatedEndDateTime.Value.TimeOfDay <= TimeSpan.FromHours(ConstantVariable.EndPMWorkingHour))
                            {
                                if (result.ElementAt(i).EstimatedEndDateTime != result.ElementAt(i + 1).EstimatedStartDateTime)
                                {
                                    var start = result.ElementAt(i).EstimatedEndDateTime.Value;
                                    var end = result.ElementAt(i + 1).EstimatedStartDateTime.Value;

                                    availableRooms.Add(new AvailableRoomViewModel
                                    {
                                        RoomId = room.Id,
                                        StartDateTime = start,
                                        EndDateTime = end,
                                        ExpectedSurgeryDuration = (end - start).TotalHours
                                    });

                                }
                            }
                        }
                    }
                }
            }
            return availableRooms.ToList();
        }
        #endregion



        public void InsertDateTimeToSurgeryShift(int surgeryId, DateTime startTime, DateTime endTime, int roomId)
        {
            var surgeryShift = _context.SurgeryShifts.Find(surgeryId);
            surgeryShift.EstimatedStartDateTime = startTime;
            surgeryShift.EstimatedEndDateTime = endTime;
            surgeryShift.SurgeryRoomId = roomId;
            _context.SaveChanges();
        }

        // TODO: Tim những phòng (RoomId) theo ngày còn đang trống lịch
        // string scheduleDateString
        public int GetEmptyRoomForDate(string scheduleDateString)
        {
            var parentRoomIds = _context.SurgeryRooms.Select(r => r.Id).ToList();
            var childRoomIds = _context.SurgeryShifts
                //.Where(s => UtilitiesDate.ConvertDateToNumber(s.ScheduleDate) == scheduleDateNumber)
                .FromSql("select * from SurgeryShifts where cast(EstimatedStartDateTime as date) = {0}", scheduleDateString)
                .Select(s => s.SurgeryRoomId).ToList();
            ICollection<int> roomIds = parentRoomIds.Where(p => !childRoomIds.Contains(p)).ToList();
            if (roomIds.Count == 0)
            {
                return 0;
            }
            return roomIds.First();
        }
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
                .Where(s => (s.EstimatedStartDateTime != null)
                && (UtilitiesDate.ConvertDateToNumber(s.EstimatedStartDateTime.Value) == dateNumber) //mm/dd/YYYY
                && (s.SurgeryRoomId == surgeryRoomId))
                .OrderBy(s => s.EstimatedStartDateTime))
            {
                results.Add(new SurgeryShiftViewModel()
                {
                    Id = shift.Id,
                    CatalogName = shift.SurgeryCatalog.Name,
                    PriorityNumber = shift.PriorityNumber,
                    EstimatedStartDateTime = UtilitiesDate.GetTimeFromDate(shift.EstimatedStartDateTime.Value),
                    EstimatedEndDateTime = UtilitiesDate.GetTimeFromDate(shift.EstimatedEndDateTime.Value),
                    PatientName = shift.Patient.FullName,
                    SurgeonNames = shift.SurgeryShiftSurgeons.Select(s => s.Surgeon.FullName).ToList()
                });
            }
            return results;
        }

<<<<<<< HEAD
        //TODO: Lấy danh sách ca mổ chưa lên lịch theo độ ưu tiên và ngày
        public ICollection<ScheduleViewModel> GetSurgeryShiftsNoSchedule()
        {
            var result = _context.SurgeryShifts
                .Where(s => (s.EstimatedStartDateTime == null) && (s.EstimatedEndDateTime == null)
                && (s.IsAvailableMedicalSupplies == true) && (s.SurgeryRoomId == null))              
                .OrderBy(s => s.ExpectedSurgeryDuration)
                .OrderBy(s => s.PriorityNumber)
                .OrderBy(s => s.ScheduleDate)
                .ToList();
=======
        //TODO: Lấy danh sách ca mổ chưa lên lịch theo độ ưu tiên và ngày (bao gồm chỉ định và ko chỉ định)
        public ICollection<ScheduleViewModel> GetSurgeryShiftsNoSchedule()
        {
            var result = _context.SurgeryShifts
                .Where(s => (s.EstimatedStartDateTime == null) && s.EstimatedEndDateTime == null
                && s.IsAvailableMedicalSupplies == true
                && s.ProposedStartDateTime == null && s.ProposedEndDateTime == null
                && s.SurgeryRoomId == null)
                .OrderBy(s => s.ConfirmDate)
                .OrderBy(s => s.ExpectedSurgeryDuration)
                .OrderBy(s => s.PriorityNumber).ToList();
>>>>>>> e17416a75e887b15ffd736100cbfec5fdf47286b
            var surgeryShiftList = new List<ScheduleViewModel>();
            foreach (var shift in result)
            {
                surgeryShiftList.Add(new ScheduleViewModel()
                {
                    SurgeryShiftId = shift.Id,
                    ProposedStartDateTime = shift.ProposedStartDateTime,
                    ProposedEndDateTime = shift.ProposedEndDateTime,
                    ConfirmDate = shift.ConfirmDate.Value,
                    ScheduleDate = shift.ScheduleDate.Value,
                    ExpectedSurgeryDuration = shift.ExpectedSurgeryDuration,
                    PriorityNumber = shift.PriorityNumber
                });
            }
            var proposedTimeSurgeryShiftList = GetSurgeryShiftNoScheduleByProposedTime();

            return proposedTimeSurgeryShiftList.Union(surgeryShiftList).ToList();
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
                    StartTime = $"{shift.EstimatedStartDateTime.Value.ToShortTimeString()} {shift.EstimatedStartDateTime.Value.ToShortDateString()}",
                    EndTime = $"{shift.EstimatedEndDateTime.Value.ToShortTimeString()} {shift.EstimatedEndDateTime.Value.ToShortDateString()}",
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
        #endregion
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

        public List<int> GetAvailableRoom(DateTime start, DateTime end)
        {
            var rooms = _context.SurgeryRooms.Where(r => !r.IsDeleted);
            var roomId = new List<int>();
            foreach (var room in rooms)
            {
                var shifts = room.SurgeryShifts.Where(s =>
                !s.IsDeleted
                );
                if (shifts.ToList().Count() != 0)
                {
                    var onScheduleShifts = shifts.Where(s => start < s.EstimatedEndDateTime);
                    if (onScheduleShifts.ToList().Any())
                    {
                        onScheduleShifts = onScheduleShifts.OrderByDescending(s => s.EstimatedStartDateTime);
                        if (end < onScheduleShifts.FirstOrDefault().EstimatedStartDateTime)
                        {
                            roomId.Add(room.Id);
                        }
                    }
                }
                else
                {
                    roomId.Add(room.Id);
                }

            }
            return roomId;
        }

        public List<AvailableRoomViewModel> GetAvailableRoom(int hour, int minute)
        {
            var rooms = _context.SurgeryRooms.Where(r => !r.IsDeleted).ToList();
            var availableRooms = new List<AvailableRoomViewModel>();
            foreach (var room in rooms)
            {
                var shifts = room.SurgeryShifts.Where(s =>
                !s.IsDeleted);
                if (shifts.ToList().Count != 0)
                {
                    shifts = shifts.OrderByDescending(s => s.EstimatedStartDateTime);
                    if (shifts.Count() == 1)
                    {
                        var shift = shifts.FirstOrDefault();
                        var timeCondition = (shift.EstimatedStartDateTime.Value.AddHours(-hour).AddMinutes(-minute - 1).Hour >= 7); //TODO : Explain + 1  minute

                        if (timeCondition)
                        {
                            availableRooms.Add(new AvailableRoomViewModel()
                            {
                                RoomId = room.Id,
                                StartDateTime = shift.EstimatedStartDateTime.Value.AddHours(-hour).AddMinutes(-minute - 1),
                                EndDateTime = shift.EstimatedStartDateTime.Value.AddMinutes(-1)
                            });
                        }
                        timeCondition = (shift.EstimatedEndDateTime.Value.Hour <= 17);
                        if (timeCondition)
                        {
                            availableRooms.Add(new AvailableRoomViewModel()
                            {
                                RoomId = room.Id,
                                StartDateTime = shift.EstimatedEndDateTime.Value.AddMinutes(1), //TODO : Explain + 1  minute
                                EndDateTime = shift.EstimatedEndDateTime.Value.AddHours(hour).AddMinutes(minute + 1)
                            });
                        }
                    }
                    else
                    {
                        for (int i = 0; i < shifts.Count(); i++)
                        {
                            var shift = shifts.ElementAt(i);
                            if (i == (shifts.Count() - 1))
                            {
                                if (shift.EstimatedEndDateTime.Value.Hour <= 17)
                                {
                                    availableRooms.Add(new AvailableRoomViewModel()
                                    {
                                        RoomId = room.Id,
                                        StartDateTime = shift.EstimatedEndDateTime.Value,
                                        EndDateTime = shift.EstimatedEndDateTime.Value.AddHours(hour).AddMinutes(minute)
                                    });
                                }
                            }
                            else
                            {
                                var shiftAfter = shifts.ElementAt(i + 1);
                                if (shiftAfter.EstimatedStartDateTime.Value - shift.EstimatedEndDateTime.Value >= new TimeSpan(hours: hour, minutes: minute, seconds: 0))
                                {
                                    availableRooms.Add(new AvailableRoomViewModel()
                                    {
                                        RoomId = room.Id,
                                        StartDateTime = shift.EstimatedEndDateTime.Value,
                                        EndDateTime = shift.EstimatedEndDateTime.Value.AddHours(hour).AddMinutes(minute)
                                    });
                                }
                            }
                        }
                    }
                }
                else
                {
                    var newStartDateTime = new DateTime(year: DateTime.Now.Year, month: DateTime.Now.Month, day: DateTime.Now.Day, hour: 7, minute: 0, second: 0);
                    availableRooms.Add(new AvailableRoomViewModel()
                    {
                        RoomId = room.Id,
                        StartDateTime = newStartDateTime,
                        EndDateTime = newStartDateTime.AddHours(hour).AddMinutes(minute)
                    });
                }

            }
            return availableRooms;
        }




    }
}
