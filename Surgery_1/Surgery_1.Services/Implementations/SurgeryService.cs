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
            //var result = GetSurgeryShiftsNoSchedule(1);
            //foreach (var index in result)
            //{
            //    MakeSchedule(index);
            //}

            var result = GetSurgeryShiftsNoSchedule(1).First();
            //foreach (var index in result)
            //{
                MakeSchedule(result);
            //}
        }
        public void MakeSchedule(ScheduleViewModel scheduleViewModel)
        {
            string selectedDay = UtilitiesDate.ConvertDateToString(scheduleViewModel.StartAMWorkingHour);
            // Tìm những phòng còn trống theo ngày
            // Parse số thành giờ: 1.5 => 1h30
            TimeSpan hour = TimeSpan.FromHours(scheduleViewModel.ExpectedSurgeryDuration);
            int roomEmptyId = GetEmptyRoomForDate(selectedDay);
            if (roomEmptyId == null || roomEmptyId == 0)
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
                    else if (endEstimatedTime >= scheduleViewModel.EndAMWorkingHour && endEstimatedTime <= scheduleViewModel.StartPMWorkingHour)
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
            // Lấy ngày cần lên lịch mổ (mổ bình thường), dạng số giảm dần theo thời gian
            // TODO: List những phòng có thời gian phẫu thuật trễ nhất, giảm dần
            // Chọn các phòng theo ngày

            var result1 = _context.SurgeryShifts
                .Where(s => (s.EstimatedStartDateTime != null) 
                && (UtilitiesDate.ConvertDateToNumber(s.EstimatedEndDateTime.Value) == UtilitiesDate.ConvertDateToNumber(scheduleViewModel.StartAMWorkingHour)))
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

        public RoomDateViewModel GetRoomByMaxSurgeryTimeBeforeStartAM(ScheduleViewModel scheduleViewModel)
        {
            // Lấy ngày cần lên lịch mổ (mổ bình thường), dạng số giảm dần theo thời gian
            // TODO: List những phòng có thời gian phẫu thuật trễ nhất, giảm dần
            // Chọn các phòng theo ngày

            var result1 = _context.SurgeryShifts
                .Where(s => (s.EstimatedStartDateTime != null)
                && (UtilitiesDate.ConvertDateToNumber(s.EstimatedEndDateTime.Value) == UtilitiesDate.ConvertDateToNumber(scheduleViewModel.StartAMWorkingHour))
                && s.EstimatedEndDateTime < s.EndAMWorkingHour)
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
            result3 = result3.OrderByDescending(s => s.EarlyEndDateTime).ToList();
            // Lấy phòng hợp lý nhất
            return result3.First();
        }


        public RoomDateViewModel GetRoomByMax(int dateNumber)
        {
            // Lấy ngày cần lên lịch mổ (mổ bình thường), dạng số giảm dần theo thời gian
            // TODO: List những phòng có thời gian phẫu thuật trễ nhất, giảm dần
            // Chọn các phòng theo ngày

            var result1 = _context.SurgeryShifts
                .Where(s => (s.EstimatedStartDateTime != null) && (UtilitiesDate.ConvertDateToNumber(s.StartAMWorkingHour) == dateNumber))
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
        public void InsertDateTimeToSurgeryShift(int surgeryId, DateTime startTime, DateTime endTime, int roomId)
        {
            var surgeryShift = _context.SurgeryShifts.Find(surgeryId);
            surgeryShift.EstimatedStartDateTime = startTime;
            surgeryShift.EstimatedEndDateTime = endTime;
            surgeryShift.SurgeryRoomId = roomId;
            _context.SaveChanges();
        }

        // TODO: Tim những phòng (RoomId) theo ngày còn đang trống lịch
        public int GetEmptyRoomForDate(string scheduleDateString)
        {
            var parentRoomIds = _context.SurgeryRooms.Select(r => r.Id).ToList();
            var childRoomIds = _context.SurgeryShifts
                .FromSql("select * from SurgeryShifts where cast(EstimatedStartDateTime as date) = {0}", scheduleDateString)
                .Select(s => s.SurgeryRoomId).ToList();
            ICollection<int> roomIds = parentRoomIds.Where(p => !childRoomIds.Contains(p)).ToList();
            if (roomIds.Count == 0)
            {
                return 0;
            }
            return roomIds.First();
        }
        //TODO: Lấy danh sách ca mổ chưa lên lịch theo ngày
        public List<SurgeryShift> GetSurgeryShiftNoScheduleByDay()
        {
            var surgeryShifts = _context.SurgeryShifts
                .Where(s => (s.IsAvailableMedicalSupplies == true) && (s.SurgeryRoomId == null))
                .OrderBy(s => s.StartAMWorkingHour).ToList();
            return surgeryShifts;   
        }
        //TODO: Import file

        public void InsertFileToSurgeryShift(ScheduleViewModel scheduleViewModel)
        {
            var surgeryShift = new SurgeryShift
            {
                StartAMWorkingHour = scheduleViewModel.StartAMWorkingHour,
                EndAMWorkingHour = scheduleViewModel.EndAMWorkingHour,
                StartPMWorkingHour = scheduleViewModel.StartPMWorkingHour,
                EndPMWorkingHour = scheduleViewModel.EndPMWorkingHour,
                ExpectedSurgeryDuration = scheduleViewModel.ExpectedSurgeryDuration,
                PriorityNumber = scheduleViewModel.PriorityNumber
            };
            _context.SurgeryShifts.Add(surgeryShift);
            _context.SaveChanges();
        }

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
                    //CatalogName = shift.SurgeryRoomCatalog.Name,
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
        public ICollection<ScheduleViewModel> GetSurgeryShiftsNoSchedule(int dateNumber)
        {
            var result = _context.SurgeryShifts.Where(s => s.EstimatedStartDateTime == null)
                .OrderBy(s => s.StartAMWorkingHour).OrderBy(s => s.PriorityNumber).OrderBy(s => s.ExpectedSurgeryDuration).ToList();
            var surgeryShiftList = new List<ScheduleViewModel>();
            foreach (var shift in result)
            {
                surgeryShiftList.Add(new ScheduleViewModel()
                {
                    SurgeryShiftId = shift.Id,
                    ProposedStartDateTime = shift.ProposedStartDateTime,
                    ProposedEndDateTime = shift.ProposedEndDateTime,
                    StartAMWorkingHour = shift.StartAMWorkingHour,
                    EndAMWorkingHour = shift.EndAMWorkingHour,
                    StartPMWorkingHour = shift.StartPMWorkingHour,
                    EndPMWorkingHour = shift.EndPMWorkingHour,
                    ExpectedSurgeryDuration = shift.ExpectedSurgeryDuration,
                    PriorityNumber = shift.PriorityNumber
                });
            }
            return surgeryShiftList;
        }

        public SurgeryShiftDetailViewModel GetShiftDetail(int shiftId)
        {
            var shift = _context.SurgeryShifts.Find(shiftId);
            if (shift != null)
            {
                var result = new SurgeryShiftDetailViewModel()
                {
                    Id = shift.Id,
                    Gender = shift.Patient.Gender == -1 ? "Nam" : "Nữ",
                    Age = DateTime.Now.Year - shift.Patient.YearOfBirth,
                    Speciality = shift.SurgeryRoomCatalog.Speciality.Name,
                    SurgeryName = shift.SurgeryRoomCatalog.Name,
                    SurgeryType = shift.SurgeryRoomCatalog.Type,
                    StartTime = $"{shift.EstimatedStartDateTime.Value.ToShortTimeString()} {shift.EstimatedStartDateTime.Value.ToShortDateString()}",
                    EndTime = $"{shift.EstimatedEndDateTime.Value.ToShortTimeString()} {shift.EstimatedEndDateTime.Value.ToShortDateString()}",
                    EkipMembers = shift.Ekip.Members.Select(m=>new EkipMemberViewModel() {Name = m.Name,WorkJob = m.WorkJob }).ToList(),
                    Procedure = shift.SurgeryRoomCatalog.Procedure
                };
                return result;
            }
            return null;
        }
    }
}
