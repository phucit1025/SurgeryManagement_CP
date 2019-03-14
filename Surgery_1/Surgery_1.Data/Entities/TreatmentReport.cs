using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Data.Entities
{
    public class TreatmentReport : BaseEntity
    {
        public TreatmentReport()
        {
            TreatmentReportDrugs = new HashSet<TreatmentReportDrug>();
        }
        public int SurgeryShiftId { get; set; }
        public string MedicalRequirement { get; set; }
        public string ProgressiveDisease { get; set; }
        public bool IsUsed { get; set; }

        public virtual SurgeryShift SurgeryShift { get; set; }
        public virtual ICollection<TreatmentReportDrug> TreatmentReportDrugs { get; set; }
    }
}
