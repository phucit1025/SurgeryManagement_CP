using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Data.Entities
{
    public class UserInfo : BaseEntity
    {
        public UserInfo()
        {
            SurgeryShifts = new HashSet<SurgeryShift>();
            HealthCareReports = new HashSet<HealthCareReport>();
        }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string GuId { get; set; } 
        
        public virtual ICollection<SurgeryShift> SurgeryShifts { get; set; }
        public virtual ICollection<HealthCareReport> HealthCareReports { get; set; }
    }
}
