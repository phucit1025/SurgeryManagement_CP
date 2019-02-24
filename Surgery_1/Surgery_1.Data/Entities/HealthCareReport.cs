using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Data.Entities
{
    public class HealthCareReport : BaseEntity
    {
        public int SurgeryShiftId { get; set; }
        public string CareReason { get; set; }
        public string EventContent { get; set; }
        public string CareContent { get; set; }

        public virtual SurgeryShift SurgeryShift { get; set; }
    }
}
