using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Data.ViewModels
{
    public class ScheduleViewModel
    {
        public int SurgeryShiftId { get; set; }
        public DateTime? ProposedStartDateTime { get; set; }
        public DateTime? ProposedEndDateTime { get; set; }
        //Working hour of hospital: 7 - 11h; 13 - 17
        public DateTime StartAMWorkingHour { get; set; }
        public DateTime EndAMWorkingHour { get; set; }
        public DateTime StartPMWorkingHour { get; set; }
        public DateTime EndPMWorkingHour { get; set; }
        //Thời gian hoàn thành ca mổ dự kiến
        public float ExpectedSurgeryDuration { get; set; }

        public int PriorityNumber { get; set; }
    }
}
