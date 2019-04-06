using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Data.Entities
{
    public class SpecialtyGroup : BaseEntity
    {
        public SpecialtyGroup()
        {
            Specialties = new HashSet<Specialty>();
            SurgeryRooms = new HashSet<SurgeryRoom>();
        } 
        public string Name { get; set; }
        public virtual ICollection<Specialty> Specialties { get; set; }
        public virtual ICollection<SurgeryRoom> SurgeryRooms { get; set; }
    }
}
