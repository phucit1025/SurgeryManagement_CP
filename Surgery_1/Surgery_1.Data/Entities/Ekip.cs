using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Data.Entities
{
    public class Ekip : BaseEntity
    {
        public Ekip()
        {
            Members = new HashSet<EkipMember>();
            SurgeryShifts = new HashSet<SurgeryShift>();
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public virtual ICollection<EkipMember> Members { get; set; }
        public virtual ICollection<SurgeryShift> SurgeryShifts { get; set; }
    }
}
