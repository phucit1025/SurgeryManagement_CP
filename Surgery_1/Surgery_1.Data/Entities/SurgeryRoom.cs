using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Data.Entities
{
    public class SurgeryRoom : BaseEntity
    {
        public SurgeryRoom()
        {
            SurgeryShifts = new HashSet<SurgeryShift>();
        }

        public string Name { get; set; }
        public int Capacity { get; set; }

        public virtual ICollection<SurgeryShift> SurgeryShifts { get; set; }
    }
}
