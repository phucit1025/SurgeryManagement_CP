using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Data.Entities
{
    public class Surgeon : BaseEntity
    {
        public Surgeon()
        {
            SurgeryShifts = new HashSet<SurgeryShift>();
        }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }

        public virtual ICollection<SurgeryShift> SurgeryShifts { get; set; }
    }
}
