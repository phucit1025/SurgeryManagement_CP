using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Data.Entities
{
    public class Drug : BaseEntity
    {
        public Drug()
        {
            TreatmentReportDrug = new HashSet<TreatmentReportDrug>();
        }
        public string DrugName { get; set; }
        public string Unit { get; set; }

        public virtual ICollection<TreatmentReportDrug> TreatmentReportDrug { get; set; }
    }
}
