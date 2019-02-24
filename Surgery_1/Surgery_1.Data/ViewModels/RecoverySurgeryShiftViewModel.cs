using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Data.ViewModels
{
    public class RecoverySurgeryShiftViewModel
    {
        public int Id { get; set; }
        public string PatientName { get; set; }
        public int PatientAge { get; set; }
        public int PatientGender { get; set; }
        public string Diagnose { get; set; }

        public List<HealthCareReportViewModel> CareReports { get; set; }
    }

    public class HealthCareReportViewModel
    {
        public int Id { get; set; }
        public string DateCreated { get; set; }
        public string VisitReason { get; set; }
        public string EventContent { get; set; }
        public string CareContent { get; set; }
        public int SurgeryShiftId { get; set; }
    }
}
