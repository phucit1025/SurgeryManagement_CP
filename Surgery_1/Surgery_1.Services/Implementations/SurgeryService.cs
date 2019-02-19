using Microsoft.EntityFrameworkCore;
using Surgery_1.Data.Context;
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
            //sau này EstimatedTime sẽ đổi thành ActualTime
            //list những phòng có thời gian phẫu thuật trễ nhất, giảm dần
            DateTime estimatedDate = scheduleViewModel.StartAMWorkingHour.Date;

            var result = _context.SurgeryShifts
                .FromSql("select top 1 max(ActualEndDatetime) as [Datetime], SurgeryRoomId " +
                            "from SurgeryShifts " +
                            "where cast(ActualStartDatetime as date) = {0} " +
                            "group by SurgeryRoomId", estimatedDate).ToList();
            TimeSpan hour = TimeSpan.FromHours(scheduleViewModel.ExpectedSurgeryDuration);
            //Đổi estimasted thành result. j đó
            DateTime endEstimatedTime = estimatedDate + hour;
            if (endEstimatedTime <= scheduleViewModel.EndAMWorkingHour)
            {
                var surgeryShift = _context.SurgeryShifts.Find(scheduleViewModel.SurgeryShiftId);
                surgeryShift.EstimatedStartDateTime = estimatedDate;
                surgeryShift.EstimatedEndDateTime = endEstimatedTime;
                _context.SaveChanges();
            } else if (endEstimatedTime <= scheduleViewModel.EndPMWorkingHour)
            {

            }

            return scheduleViewModel.StartAMWorkingHour.ToString();
        }
        public void InsertDateTimeToSurgeryShift(int id, DateTime startTime, DateTime endTime)
        {
            var surgeryShift = _context.SurgeryShifts.Find(scheduleViewModel.SurgeryShiftId);
            surgeryShift.EstimatedStartDateTime = estimatedDate;
            surgeryShift.EstimatedEndDateTime = endEstimatedTime;
            _context.SaveChanges();
        }

    }
}
