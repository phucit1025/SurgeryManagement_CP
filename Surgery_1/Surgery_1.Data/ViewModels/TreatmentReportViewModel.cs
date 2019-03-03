using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Data.ViewModels
{
    public class TreatmentReportViewModel
    {
        public int Id { get; set; }
        public string DateCreated { get; set; }
        public string ProgressiveDisease { get; set; }
        public string MedicalRequirement { get; set; }
        public int ShiftId { get; set; }
    }
}
