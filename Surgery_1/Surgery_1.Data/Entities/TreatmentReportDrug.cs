using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Surgery_1.Data.Entities
{
    public class TreatmentReportDrug : BaseEntity
    {
        public int TreatmentReportId { get; set; }
        public int DrugId { get; set; }
        public int Quantity { get; set; }
        public bool IsMorning { get; set; } 
        public bool IsAfternoon { get; set; } 
        public bool IsEvening { get; set; } 
        public bool IsNight { get; set; } 

        [ForeignKey("TreatmentReportId")]
        public virtual TreatmentReport TreatmentReport { get; set; }
        [ForeignKey("DrugId")]
        public virtual Drug Drug { get; set; }
    }
}
