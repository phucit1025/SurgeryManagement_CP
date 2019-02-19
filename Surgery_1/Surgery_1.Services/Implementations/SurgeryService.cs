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
            GetSurgeryShiftNoScheduleByDay();
            //sau này EstimatedTime sẽ đổi thành ActualTime
            //list những phòng có thời gian phẫu thuật trễ nhất, giảm dần
            //DateTime estimatedDate = scheduleViewModel.StartAMWorkingHour.Date;

            //var result = _context.SurgeryShifts
            //    .FromSql("select top 1 max(EstimatedEndDateTime) as [Datetime], SurgeryRoomId " +
            //                "from SurgeryShifts " +
            //                "where cast(EstimatedStartDateTime as date) = {0} " +
            //                "group by SurgeryRoomId", estimatedDate).ToList();
            //TimeSpan hour = TimeSpan.FromHours(scheduleViewModel.ExpectedSurgeryDuration);
            ////Đổi estimasted thành result. j đó
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
        //TODO: Lấy những ca mổ chưa lên lịch theo ngày
        public List<SurgeryShift> GetSurgeryShiftNoScheduleByDay()
        {
            var surgeryShifts = _context.SurgeryShifts.ToList();
            //    .Where(s => (s.IsAvailableMedicalSupplies == true) && (s.SurgeryRoomId == null)).ToList();
            var result = _context.SurgeryShifts
                .FromSql("select StartAMWorkingHour, EndAMWorkingHour, " +
                "StartPMWorkingHour, EndPMWorkingHour, ExpectedSurgeryDuration, " +
                "IsAvailableMedicalSupplies, PriorityNumber from SurgeryShifts " +
                "where IsAvailableMedicalSupplies = {0} and SurgeryRoomId is null", 1);
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
    }
}
