using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Data.Entities
{
    public class Notification : BaseEntity
    {
        public string Content { get; set; }
        public string RoleToken { get; set; }
        public string StaffGuid { get; set; }
        public bool IsRead { get; set; } = false;
    }
}
