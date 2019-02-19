using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Data.Entities
{
    public class Patient : BaseEntity
    {
        public Patient()
        {
            SurgeryShifts = new HashSet<SurgeryShift>();
        }

        public string FullName { get; set; }
        public string IdentityNumber { get; set; }
        public string Address { get; set; }
        public int Gender { get; set; }
        public int YearOfBirth { get; set; }

        public virtual ICollection<SurgeryShift> SurgeryShifts { get; set; }
    }
}
