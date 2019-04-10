using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Data.Entities
{
    public class SpecialityGroup : BaseEntity
    {
        public SpecialityGroup()
        {
            Specialities = new HashSet<Speciality>();
            SurgeryRooms = new HashSet<SurgeryRoom>();
        }
        public string Name { get; set; }
        public virtual ICollection<Speciality> Specialities { get; set; }
        public virtual ICollection<SurgeryRoom> SurgeryRooms { get; set; }
    }
}
