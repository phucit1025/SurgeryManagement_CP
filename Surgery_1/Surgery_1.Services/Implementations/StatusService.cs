using Surgery_1.Data.Context;
using Surgery_1.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Surgery_1.Services.Implementations
{
    public class StatusService : IStatusService
    {
        private readonly string PRE_STATUS = "Preoperative";
        private readonly string POST_STATUS = "Postoperative";
        private readonly string INTRA_STATUS = "Intraoperative";
        private readonly string RECOVERY_STATUS = "Recovery";
        private readonly string FINISHED_STATUS = "Finished";

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
            var status = _context.Statuses.Where(s => s.Name.Equals(POST_STATUS)).FirstOrDefault();
            if (shift != null)
            {
                shift.StatusId = roomType;
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
            var status = _context.Statuses.Where(s => s.Name.Equals(INTRA_STATUS)).FirstOrDefault();
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
            var status = _context.Statuses.Where(s => s.Name.Equals(FINISHED_STATUS)).FirstOrDefault();
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
