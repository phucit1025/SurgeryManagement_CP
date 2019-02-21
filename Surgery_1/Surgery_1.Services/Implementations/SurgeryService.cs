using Microsoft.EntityFrameworkCore;
using Surgery_1.Data.Context;
using Surgery_1.Data.Entities;
using Surgery_1.Data.ViewModels;
using Surgery_1.Repositories.Interfaces;
using Surgery_1.Services.Interfaces;
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
        public void MakeSchedule(ScheduleViewModel scheduleViewModel)
        {

        }
        public RoomDateViewModel GetRoomByMaxSurgeryTime(ScheduleViewModel scheduleViewModel)
        {
            // Lấy ngày cần lên lịch mổ (mổ bình thường), dạng số giảm dần theo thời gian
            int estimatedDateNumber = ConvertDateToNumber(scheduleViewModel.StartAMWorkingHour.Date);
            // List những phòng có thời gian phẫu thuật trễ nhất, giảm dần
            var result1 = _context.SurgeryShifts
                .FromSql("select * from SurgeryShifts where cast(EstimatedEndDateTime as date) = {0}", ConvertDateToString(scheduleViewModel.StartAMWorkingHour.Date))
                .OrderBy(s => s.EstimatedEndDateTime)
                .Select(s => new { s.EstimatedEndDateTime, s.SurgeryRoomId }).ToList();

            var result2 = result1.GroupBy(s => s.SurgeryRoomId).ToList();
            var result3 = new List<RoomDateViewModel>();
            foreach (var item in result2)
            {
                result3.Add(new RoomDateViewModel()
                {
                    SurgeryRoomId = int.Parse(item.Key.Value.ToString()),
                    EarlyEndDateTime = item.Max(s => s.EstimatedEndDateTime).ToString(),
                });
            }
            var availableRoom = result3.First();
            //TimeSpan hour = TimeSpan.FromHours(scheduleViewModel.ExpectedSurgeryDuration);
            //Đổi estimasted thành result. j đó
            //DateTime endEstimatedTime = estimatedDate + hour;
            //if (endEstimatedTime <= scheduleViewModel.EndAMWorkingHour) //buổi sáng
            //{
            //    InsertDateTimeToSurgeryShift(scheduleViewModel.SurgeryShiftId, estimatedDate, endEstimatedTime);
            //}
            //else if (endEstimatedTime <= scheduleViewModel.EndPMWorkingHour) //buổi chiều
            //{
            //    InsertDateTimeToSurgeryShift(scheduleViewModel.SurgeryShiftId, estimatedDate, endEstimatedTime);
            //}
            return result3.First();
        }
        public void InsertDateTimeToSurgeryShift(int surgeryId, DateTime startTime, DateTime endTime)
        {
            var surgeryShift = _context.SurgeryShifts.Find(surgeryId);
            surgeryShift.EstimatedStartDateTime = startTime;
            surgeryShift.EstimatedEndDateTime = endTime;
            _context.SaveChanges();
        }
        // TODO: Tim những phòng (RoomId) theo ngày còn đang trống lịch
        public ICollection<int> GetEmptyRoomForDate(int scheduleDateNumber)
        {
            var parentRoomIds = _context.SurgeryRooms.Select(r => r.Id).ToList();
            var childRoomIds = _context.SurgeryShifts
                .Where(s => ConvertDateToNumber(s.EstimatedStartDateTime.Value) == scheduleDateNumber)
                .Select(s => s.SurgeryRoomId).ToList();
            ICollection<int> roomIds = parentRoomIds.Where(p => !childRoomIds.Contains(p)).ToList();
            return roomIds;
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
                && (ConvertDateToNumber(s.EstimatedStartDateTime.Value) == dateNumber) //mm/dd/YYYY
                && (s.SurgeryRoomId == surgeryRoomId))
                .OrderBy(s => s.EstimatedStartDateTime))
            {
                results.Add(new SurgeryShiftViewModel()
                {
                    Id = shift.Id,
                    //CatalogName = shift.SurgeryRoomCatalog.Name,
                    //EstimatedEndDateTime = $"{shift.EstimatedEndDateTime.ToShortDateString()} {shift.EstimatedEndDateTime.ToShortTimeString()}",
                    //EstimatedStartDateTime = $"{shift.EstimatedStartDateTime.ToShortDateString()} {shift.EstimatedStartDateTime.ToShortTimeString()}",
                    EstimatedStartDateTime = GetTimeFromDate(shift.EstimatedStartDateTime.Value),
                    EstimatedEndDateTime = GetTimeFromDate(shift.EstimatedEndDateTime.Value),
                    PatientName = shift.Patient.FullName,
                    SurgeonNames = shift.SurgeryShiftSurgeons.Select(s => s.Surgeon.FullName).ToList()
                });
            }
            return results;
        }

        public int ConvertDateToNumber(DateTime day)
        {
            string dayNum = day.Day < 10 ? "0" + day.Day.ToString() : day.Day.ToString();
            string monthNum = day.Month < 10 ? "0" + day.Month.ToString() : day.Month.ToString(); ;
            string yearNum = day.Year.ToString();
            string date = yearNum + monthNum + dayNum;
            return int.Parse(date);
        }
        public string ConvertDateToString(DateTime day)
        {
            string dayNum = day.Day < 10 ? "0" + day.Day.ToString() : day.Day.ToString();
            string monthNum = day.Month < 10 ? "0" + day.Month.ToString() : day.Month.ToString(); ;
            string yearNum = day.Year.ToString();
            string dateString = yearNum + "-" + monthNum + "-" + dayNum;
            return dateString;
        }
        public string GetTimeFromDate(DateTime day)
        {
            string hour = day.Hour < 10 ? "0" + day.Hour.ToString() : day.Hour.ToString();
            string minute = day.Minute < 10 ? "0" + day.Minute.ToString() : day.Minute.ToString();
            return hour + ":" + minute;
        }
    }
}
