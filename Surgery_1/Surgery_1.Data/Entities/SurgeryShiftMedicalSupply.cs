using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Surgery_1.Data.Entities
{
    public class SurgeryShiftMedicalSupply : BaseEntity
    {
        public int MedicalSupplyId { get; set; }
        public int SurgeryShiftId { get; set; }

        [ForeignKey("MedicalSupplyId")]
        public virtual MedicalSupply MedicalSupply { get; set; }
        [ForeignKey("SurgeryShiftId")]
        public virtual SurgeryShift SurgeryShift { get; set; }
    }
}
