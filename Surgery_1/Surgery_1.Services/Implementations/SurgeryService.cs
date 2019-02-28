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
        //private readonly ISurgeryRepository _surgeryRepo;


        //public SurgeryService(ISurgeryRepository surgeryRepository)
        //{
        //    _surgeryRepo = surgeryRepository;
        //}

        private readonly AppDbContext _context;

        public SurgeryService(AppDbContext _context)
        {
            this._context = _context;
        }


        public void MakeScheduleList()
        {
            var result = GetSurgeryShiftsNoSchedule();
            foreach (var index in result)
            {
                if (index.ProposedStartDateTime != null && index.ProposedEndDateTime != null)
                {
                    int roomId = GetAvailableRoomForProposedTime(index.ProposedStartDateTime.Value, index.ProposedEndDateTime.Value);
                    if (roomId == 0)
                    {

                    }
                    else
                    {
                        InsertDateTimeToSurgeryShift(index.SurgeryShiftId, index.ProposedStartDateTime.Value, index.ProposedEndDateTime.Value, roomId);
                    }

                    //foreach (var room in roomIds)
                    //{
                    //    var shift = _context.SurgeryShifts
                    //        .Max(s => s.EstimatedEndDateTime <= index.ProposedStartDateTime);
                    //}
                }
                else
                {
                    MakeSchedule(index);
                }

            }
        }

        public int GetAvailableRoomForProposedTime(DateTime startTime, DateTime endTime)
        {

            var parentRoomIds = _context.SurgeryRooms.Select(s => s.Id).ToList();
            // TODO: Lây những phòng không hợp lệ (điều có khoảng thời gian trung với startTime và endTime)
            var childRoomIds = _context.SurgeryShifts
                .Where(s => (s.EstimatedStartDateTime >= startTime && s.EstimatedStartDateTime < endTime)
                || (s.EstimatedEndDateTime > startTime && s.EstimatedEndDateTime <= endTime))
                .Select(s => s.SurgeryRoomId).ToList();
            // Loại những phòng không hợp lệ
            ICollection<int> roomIds = parentRoomIds.Where(p => !childRoomIds.Contains(p)).ToList();
            if (roomIds == null || roomIds.Count == 0)
            {
                return 0;
            }
            return roomIds.First();
        }

        public void MakeSchedule(ScheduleViewModel scheduleViewModel)
        {
            scheduleViewModel.StartAMWorkingHour
                = (scheduleViewModel.ScheduleDate + TimeSpan.FromHours(ConstantVariable.StartAMWorkingHour));
            scheduleViewModel.EndAMWorkingHour
                = (scheduleViewModel.ScheduleDate + TimeSpan.FromHours(ConstantVariable.EndAMWorkingHour));
            scheduleViewModel.StartPMWorkingHour
                = (scheduleViewModel.ScheduleDate + TimeSpan.FromHours(ConstantVariable.StartPMWorkingHour));
            scheduleViewModel.EndPMWorkingHour
                = (scheduleViewModel.ScheduleDate + TimeSpan.FromHours(ConstantVariable.EndPMWorkingHour));

            string selectedDay = UtilitiesDate.ConvertDateToString(scheduleViewModel.ScheduleDate);
            // Tìm những phòng còn trống theo ngày
            // Parse số thành giờ: 1.5 => 1h30
            TimeSpan hour = TimeSpan.FromHours(scheduleViewModel.ExpectedSurgeryDuration);
            int roomEmptyId = GetEmptyRoomForDate(selectedDay);
            if (roomEmptyId == 0)
            {
                // Lấy ra cái phòng hợp lý nhất, có ca sớm nhất
                var availableRoom = GetRoomByMaxSurgeryTime(scheduleViewModel);
                if (availableRoom == null)
                {

                } // End If available room
                else
                {
                    DateTime endEstimatedTime = availableRoom.EarlyEndDateTime.Value + hour;
                    if (availableRoom.EarlyEndDateTime.Value >= scheduleViewModel.StartAMWorkingHour && endEstimatedTime <= scheduleViewModel.EndAMWorkingHour) //buổi sáng: thời gian phải <= 11:00
                    {// Khoảng thời gian hợp lý vào buổi sáng
                        InsertDateTimeToSurgeryShift
                            (scheduleViewModel.SurgeryShiftId, availableRoom.EarlyEndDateTime.Value, endEstimatedTime, availableRoom.SurgeryRoomId);
                    }
                    else if (availableRoom.EarlyEndDateTime.Value >= scheduleViewModel.StartPMWorkingHour && endEstimatedTime <= scheduleViewModel.EndPMWorkingHour) //buổi chiều:  >= 13h && <= 17h
                    {// Khoảng thời gian hợp lý vào buổi chiều
                        InsertDateTimeToSurgeryShift
                            (scheduleViewModel.SurgeryShiftId, availableRoom.EarlyEndDateTime.Value, endEstimatedTime, availableRoom.SurgeryRoomId);
                    }
                    else if (endEstimatedTime >= scheduleViewModel.EndAMWorkingHour)
                    {// Nếu thời gian ước tính nằm trong giờ nghỉ trưa thì lấy thời gian đầu của buổi chiều + ExpectedSurgeryDuration
                        endEstimatedTime = scheduleViewModel.StartPMWorkingHour + hour;
                        InsertDateTimeToSurgeryShift
                            (scheduleViewModel.SurgeryShiftId, scheduleViewModel.StartPMWorkingHour, endEstimatedTime, availableRoom.SurgeryRoomId);
                        // Tính sau
                    }
                } // End else available room
            } // End room Id
            else // Trường hợp có phòng chưa có ca phẫu thuật nào sẽ add thời gian từ 7:00
            {
                DateTime endEstimatedTime = scheduleViewModel.StartAMWorkingHour + hour;
                InsertDateTimeToSurgeryShift(scheduleViewModel.SurgeryShiftId, scheduleViewModel.StartAMWorkingHour, endEstimatedTime, roomEmptyId);
            }
            // Lấy thời gian sớm nhất + khoảng giờ phẫu thuật = thời gian bắt đầu của ca phẫu thuật tiếp theo

        }

        public RoomDateViewModel GetRoomByMaxSurgeryTime(ScheduleViewModel scheduleViewModel)
        {
            // TODO: List những phòng có thời gian phẫu thuật sớm nhất, tăng dần
            // Chon thời gian ca mổ gần nhất theo phòng
            // Chọn các phòng theo ngày

            var result1 = _context.SurgeryShifts
                .Where(s => (s.EstimatedStartDateTime != null) && (s.EstimatedEndDateTime != null)
                && (UtilitiesDate.ConvertDateToNumber(s.EstimatedEndDateTime.Value) == UtilitiesDate.ConvertDateToNumber(scheduleViewModel.ScheduleDate)))
                .Select(s => new { s.EstimatedEndDateTime, s.SurgeryRoomId }).ToList();
            // Nhóm các phòng cùng tên
            var result2 = result1.GroupBy(s => s.SurgeryRoomId).ToList();
            // Lấy thời điểm ca mổ gần nhất của phòng
            var result3 = new List<RoomDateViewModel>();
            foreach (var item in result2)
            {
                result3.Add(new RoomDateViewModel()
                {
                    SurgeryRoomId = int.Parse(item.Key.Value.ToString()),
                    EarlyEndDateTime = item.Max(s => s.EstimatedEndDateTime),
                });
            }
            if (result3 == null || result3.Count == 0)
            {
                return null;
            }
            result3 = result3.OrderBy(s => s.EarlyEndDateTime).ToList();
            // Lấy phòng hợp lý nhất
            return result3.First();
        }

        //public RoomDateViewModel GetRoomByMaxSurgeryTimeBeforeStartAM(ScheduleViewModel scheduleViewModel)
        //{
        //    // Lấy ngày cần lên lịch mổ (mổ bình thường), dạng số giảm dần theo thời gian
        //    // TODO: List những phòng có thời gian phẫu thuật trễ nhất, giảm dần
        //    // Chọn các phòng theo ngày

        //    var result1 = _context.SurgeryShifts
        //        .Where(s => (s.EstimatedStartDateTime != null)
        //        && (UtilitiesDate.ConvertDateToNumber(s.EstimatedEndDateTime.Value) == UtilitiesDate.ConvertDateToNumber(scheduleViewModel.StartAMWorkingHour))
        //        && s.EstimatedEndDateTime < s.EndAMWorkingHour)
        //        .Select(s => new { s.EstimatedEndDateTime, s.SurgeryRoomId }).ToList();
        //    // Nhóm các phòng cùng tên
        //    var result2 = result1.GroupBy(s => s.SurgeryRoomId).ToList();
        //    // Lấy thời điểm ca mổ gần nhất của phòng
        //    var result3 = new List<RoomDateViewModel>();
        //    foreach (var item in result2)
        //    {
        //        result3.Add(new RoomDateViewModel()
        //        {
        //            SurgeryRoomId = int.Parse(item.Key.Value.ToString()),
        //            EarlyEndDateTime = item.Max(s => s.EstimatedEndDateTime),
        //        });
        //    }
        //    if (result3 == null || result3.Count == 0)
        //    {
        //        return null;
        //    }
        //    result3 = result3.OrderByDescending(s => s.EarlyEndDateTime).ToList();
        //    // Lấy phòng hợp lý nhất
        //    return result3.First();
        //}


        public void InsertDateTimeToSurgeryShift(int surgeryId, DateTime startTime, DateTime endTime, int roomId)
        {
            var surgeryShift = _context.SurgeryShifts.Find(surgeryId);
            surgeryShift.EstimatedStartDateTime = startTime;
            surgeryShift.EstimatedEndDateTime = endTime;
            surgeryShift.SurgeryRoomId = roomId;
            _context.SaveChanges();
        }

        // TODO: Tim những phòng (RoomId) theo ngày còn đang trống lịch
        //string scheduleDateString
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
                    //CatalogName = shift.SurgeryCatalog.Name,
                    //EstimatedEndDateTime = $"{shift.EstimatedEndDateTime.ToShortDateString()} {shift.EstimatedEndDateTime.ToShortTimeString()}",
                    //EstimatedStartDateTime = $"{shift.EstimatedStartDateTime.ToShortDateString()} {shift.EstimatedStartDateTime.ToShortTimeString()}",
                    PriorityNumber = shift.PriorityNumber,
                    EstimatedStartDateTime = UtilitiesDate.GetTimeFromDate(shift.EstimatedStartDateTime.Value),
                    EstimatedEndDateTime = UtilitiesDate.GetTimeFromDate(shift.EstimatedEndDateTime.Value),
                    PatientName = shift.Patient.FullName,
                    SurgeonNames = shift.SurgeryShiftSurgeons.Select(s => s.Surgeon.FullName).ToList()
                });
            }
            return results;
        }

        //TODO: Lấy danh sách ca mổ chưa lên lịch theo độ ưu tiên và ngày
        public ICollection<ScheduleViewModel> GetSurgeryShiftsNoSchedule()
        {
            var result = _context.SurgeryShifts
                .Where(s => (s.EstimatedStartDateTime == null) && s.EstimatedEndDateTime == null
                && s.IsAvailableMedicalSupplies == true && s.SurgeryRoomId == null)
                .OrderBy(s => s.ScheduleDate).OrderBy(s => s.PriorityNumber).OrderBy(s => s.ExpectedSurgeryDuration).ToList();
            var surgeryShiftList = new List<ScheduleViewModel>();
            foreach (var shift in result)
            {
                surgeryShiftList.Add(new ScheduleViewModel()
                {
                    SurgeryShiftId = shift.Id,
                    ProposedStartDateTime = shift.ProposedStartDateTime,
                    ProposedEndDateTime = shift.ProposedEndDateTime,
                    ScheduleDate = shift.ScheduleDate.Value,
                    ExpectedSurgeryDuration = shift.ExpectedSurgeryDuration,
                    PriorityNumber = shift.PriorityNumber
                });
            }
            return surgeryShiftList;
        }

        // TODO: Lấy những ca mổ chưa lên lịch theo thời gian chỉ điịnh
        public ICollection<ScheduleViewModel> GetSurgeryShiftNoScheduleByProposedTime()
        {
            var result = new List<ScheduleViewModel>();
            var surgeryShifts = _context.SurgeryShifts
                .Where(s => (s.IsAvailableMedicalSupplies == true) && (s.SurgeryRoomId == null)
                && s.EstimatedStartDateTime == null && s.EstimatedEndDateTime == null
                && s.ProposedStartDateTime != null && s.ProposedEndDateTime != null)
                .OrderBy(s => s.ProposedStartDateTime)
                .OrderBy(s => s.PriorityNumber)
                .OrderBy(s => s.ExpectedSurgeryDuration).ToList();
            foreach (var index in surgeryShifts)
            {
                result.Add(new ScheduleViewModel()
                {
                    SurgeryShiftId = index.Id,
                    ProposedStartDateTime = index.ProposedStartDateTime,
                    ProposedEndDateTime = index.ProposedEndDateTime,
                });
            }
            return result;
        }

        public SurgeryShiftDetailViewModel GetShiftDetail(int shiftId)
        {
            var shift = _context.SurgeryShifts.Find(shiftId);
            if (shift != null)
            {
                var result = new SurgeryShiftDetailViewModel()
                {
                    //Id = shift.Id,
                    //Gender = shift.Patient.Gender == -1 ? "Nam" : "Nữ",
                    //Age = DateTime.Now.Year - shift.Patient.YearOfBirth,
                    //Speciality = shift.SurgeryCatalog.Speciality.Name,
                    //SurgeryName = shift.SurgeryCatalog.Name,
                    //SurgeryType = shift.SurgeryCatalog.Type,
                    //StartTime = $"{shift.EstimatedStartDateTime.Value.ToShortTimeString()} {shift.EstimatedStartDateTime.Value.ToShortDateString()}",
                    //EndTime = $"{shift.EstimatedEndDateTime.Value.ToShortTimeString()} {shift.EstimatedEndDateTime.Value.ToShortDateString()}",
                    //EkipMembers = shift.Ekip.Members.Select(m=>new EkipMemberViewModel() {Name = m.Name,WorkJob = m.WorkJob }).ToList(),
                    //Procedure = shift.SurgeryCatalog.Procedure

                    Id = shift.Id,
                    Gender = "Nam",
                    Age = 59,
                    PatientName = "Lê Văn Đạt",
                    Speciality = "Phụ sản",
                    SurgeryName = "Phẫu thuật tiệt căn xương chũm",
                    SurgeryType = "P1",
                    StartTime = $"{shift.EstimatedStartDateTime.Value.ToShortTimeString()} {shift.EstimatedStartDateTime.Value.ToShortDateString()}",
                    EndTime = $"{shift.EstimatedEndDateTime.Value.ToShortTimeString()} {shift.EstimatedEndDateTime.Value.ToShortDateString()}",
                    Procedure = "bn nằm nghiêng dưới mê NKQ" +
                                "Tiên cầm máu tại chỗ bằng nước cất và Addrenailin" +
                                "Rạch da theo hình L" +
                                "bóc tách cơ dưới gai bộc lộ ố gãy xương bả vai" +
                                "Thấy gãy cạnh tròng và cạnh ngoài xương bả vai"
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

        public List<int> GetAvailableRoom(DateTime start, DateTime end)
        {
            var rooms = _context.SurgeryRooms.Where(r => !r.IsDeleted);
            var roomId = new List<int>();
            foreach (var room in rooms)
            {
                var shifts = room.SurgeryShifts.Where(s =>
                !s.IsDeleted &&
                s.Status.Name.Equals("Preoperative", StringComparison.CurrentCultureIgnoreCase)
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
                !s.IsDeleted &&
                s.Status.Name.Equals("Preoperative", StringComparison.CurrentCultureIgnoreCase));
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
        #endregion 

    }
}
