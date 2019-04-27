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
        public string TimeString { get; set; }
        public string Route { get; set; }
        public bool IsUsed { get; set; }
        public string StatusString { get; set; }

        [ForeignKey("TreatmentReportId")]
        public virtual TreatmentReport TreatmentReport { get; set; }
        [ForeignKey("DrugId")]
        public virtual Drug Drug { get; set; }
    }
}
