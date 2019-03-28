using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Surgery_1.Data.Entities
{
    public class UserInfo : BaseEntity
    {
        public UserInfo()
        {
            NurseSurgeryShifts = new HashSet<SurgeryShift>();
            TechSurgeryShifts = new HashSet<SurgeryShift>();
        }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string GuId { get; set; }

        [InverseProperty(nameof(SurgeryShift.Nurse))]
        public virtual ICollection<SurgeryShift> NurseSurgeryShifts { get; set; }
        [InverseProperty(nameof(SurgeryShift.TechnicalStaff))]
        public virtual ICollection<SurgeryShift> TechSurgeryShifts { get; set; }
    }
}
