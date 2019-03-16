using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Data.Entities
{
    public class SurgeryRoom : BaseEntity
    {
        public SurgeryRoom()
        {
            SlotRooms = new HashSet<SlotRoom>();
        }
        public string Name { get; set; }
        public int Capacity { get; set; }

        public virtual ICollection<SlotRoom> SlotRooms { get; set; }
    }
}
