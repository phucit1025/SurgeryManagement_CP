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
        public int MorningQuantity { get; set; } 
        public int AfternoonQuantity { get; set; } 
        public int EveningQuantity { get; set; } 
        public int NightQuantity { get; set; } 

        [ForeignKey("TreatmentReportId")]
        public virtual TreatmentReport TreatmentReport { get; set; }
        [ForeignKey("DrugId")]
        public virtual Drug Drug { get; set; }
    }
}
