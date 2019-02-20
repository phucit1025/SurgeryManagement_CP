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
        public string GetRoomByMaxSurgeryTime(ScheduleViewModel scheduleViewModel)
        {

            scheduleViewModel.StartAMWorkingHour = Convert.ToDateTime("2019-02-20 07:00:00");
            // Lấy ngày cần lên lịch mổ (mổ bình thường)
            DateTime estimatedDate = scheduleViewModel.StartAMWorkingHour.Date;

            // List những phòng có thời gian phẫu thuật trễ nhất, giảm dần
            //max(EstimatedEndDateTime) as [Datetime], SurgeryRoomId
            var result = _context.SurgeryShifts.FromSql("select SurgeryRoomId from dbo.SurgeryShifts ")
                            .Where(s => s.EstimatedStartDateTime.Value.Date.ToString() == "2019-02-20").GroupBy(s => s.SurgeryRoomId)
                            //.Max(s => s.EstimatedEndDateTime).ToString();

            //var result = _context.SurgeryShifts.FromSql("Select * from dbo.SurgeryShifts").Select(s => s.Id).ToList();

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
            return "";
        }
        public void InsertDateTimeToSurgeryShift(int surgeryId, DateTime startTime, DateTime endTime)
        {
            var surgeryShift = _context.SurgeryShifts.Find(surgeryId);
            surgeryShift.EstimatedStartDateTime = startTime;
            surgeryShift.EstimatedEndDateTime = endTime;
            _context.SaveChanges();
        }

        public int GetEmptyRoomForDate()
        {
            return 1;
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
        public void insertFileToSurgeryShift(ScheduleViewModel scheduleViewModel)
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

        public ICollection<SurgeryShiftViewModel> GetSurgeryShifts()
        {
            var results = new List<SurgeryShiftViewModel>();
            foreach (var shift in _context.SurgeryShifts)
            {
                results.Add(new SurgeryShiftViewModel()
                {
                    Id = shift.Id,
                    CatalogName = shift.SurgeryRoomCatalog.Name,
                    EstimatedEndDateTime = $"{shift.EstimatedEndDateTime.ToShortDateString()} {shift.EstimatedEndDateTime.ToShortTimeString()}",
                    EstimatedStartDateTime = $"{shift.EstimatedStartDateTime.ToShortDateString()} {shift.EstimatedStartDateTime.ToShortTimeString()}",
                    PatientName = shift.Patient.FullName,
                    SurgeonNames = shift.SurgeryShiftSurgeons.Select(s => s.Surgeon.FullName).ToList()
                });
            }
            return results;
        }
    }
}
