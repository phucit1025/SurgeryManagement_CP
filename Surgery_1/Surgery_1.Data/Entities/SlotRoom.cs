using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Surgery_1.Data.Entities
{
    public class SlotRoom : BaseEntity
    {

        public string Name { get; set; }
        public int SurgeryRoomId { get; set; }

        public virtual ICollection<SurgeryShift> SurgeryShifts { get; set; }

        [ForeignKey("SurgeryRoomId")]
        public virtual SurgeryRoom SurgeryRoom { get; set; }
    }
}
