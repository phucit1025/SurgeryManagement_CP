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
        public DateTime ConfirmDate { get; set; }
        public DateTime ScheduleDate { get; set; }

        //Thời gian hoàn thành ca mổ dự kiến
        public float ExpectedSurgeryDuration { get; set; }

        public int PriorityNumber { get; set; }
    }
}
