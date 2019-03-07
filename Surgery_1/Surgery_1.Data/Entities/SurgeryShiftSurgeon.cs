using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Surgery_1.Data.Entities
{
    public class SurgeryShiftSurgeon : BaseEntity
    {
        public int SurgeryShiftId { get; set; }
        public int SurgeonId { get; set; }

        [ForeignKey("SurgeryShiftId")]
        public virtual SurgeryShift SurgeryShift { get; set; }
        [ForeignKey("SurgeonId")]
        public virtual Doctor Surgeon { get; set; }
    }
}
