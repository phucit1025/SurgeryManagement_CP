using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Data.Entities
{
    public class Status : BaseEntity
    {
        public Status()
        {
            SurgeryShifts = new HashSet<SurgeryShift>();
        }

        public string Name { get; set; }
        public virtual ICollection<SurgeryShift> SurgeryShifts { get; set; }
    }
}
