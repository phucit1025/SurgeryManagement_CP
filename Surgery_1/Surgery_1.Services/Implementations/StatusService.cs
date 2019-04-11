using Surgery_1.Data.Context;
using Surgery_1.Services.Interfaces;
using Surgery_1.Services.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Surgery_1.Services.Implementations
{
    public class StatusService : IStatusService
    {

        private readonly AppDbContext _context;

        public StatusService(AppDbContext _context)
        {
            this._context = _context;
        }

        public string GetStatusByShiftId(int shiftId)
        {
            var shift = _context.SurgeryShifts.Find(shiftId);
            if (shift != null)
            {
                return shift.Status.Name;
            }
            return "";
        }

        public bool SetPostoperativeStatus(int shiftId, string roomPost, string bedPost, string actualEndDateTime, int roomType)
        {
            var shift = _context.SurgeryShifts.Find(shiftId);
            var status = _context.Statuses.Where(s => s.Name.Equals(ConstantVariable.POST_STATUS)).FirstOrDefault();
            if (shift != null)
            {
                shift.StatusId = roomType;
                shift.PostRoomName = roomPost;
                shift.PostBedName = bedPost;
                if (actualEndDateTime != null)
                {
                    shift.ActualEndDateTime = DateTime.ParseExact(actualEndDateTime, "yyyy-MM-dd HH:mm",
                                      System.Globalization.CultureInfo.InvariantCulture);
                }
                _context.Update(shift);
                _context.SaveChanges();
                return true;
            }
            return false;
        }
        public bool SetIntraoperativeStatus(int shiftId, string actualStartDateTime)
        {
            var shift = _context.SurgeryShifts.Find(shiftId);
            var status = _context.Statuses.Where(s => s.Name.Equals(ConstantVariable.INTRA_STATUS)).FirstOrDefault();
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
            var status = _context.Statuses.Where(s => s.Name.Equals(ConstantVariable.FINISHED_STATUS)).FirstOrDefault();
            if (shift != null)
            {
                shift.StatusId = status.Id;
                _context.Update(shift);
                _context.SaveChanges();
                return true;
            }
            return false;
        }
    }
}
