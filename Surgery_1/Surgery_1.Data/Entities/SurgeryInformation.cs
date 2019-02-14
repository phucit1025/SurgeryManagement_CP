using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Surgery_1.Data.Entities
{
    public class SurgeryInformation : BaseEntity
    {
        public string Content { get; set; }
        public int SurgeryShiftId { get; set; }

        [ForeignKey("SurgeryShiftId")]
        public virtual SurgeryShift SurgeryShift { get; set; }
    }
}
