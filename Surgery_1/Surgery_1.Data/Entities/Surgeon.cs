using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Data.Entities
{
    public class Doctor : BaseEntity
    {
        public Doctor()
        {
            SurgeryShiftSurgeons = new HashSet<SurgeryShiftSurgeon>();
        }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }

        public virtual ICollection<SurgeryShiftSurgeon> SurgeryShiftSurgeons { get; set; }
    }
}
