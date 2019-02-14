using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Data.Entities
{
    public class MedicalSupply : BaseEntity
    {
        public MedicalSupply()
        {
            SurgeryShiftMedicalSupplies = new HashSet<SurgeryShiftMedicalSupply>();
        }
        public string Name { get; set; }
        public string BarCode { get; set; }
        public string Origin { get; set; }
        public string Brand { get; set; }
        public string SupplyCompany { get; set; }
        public string Price { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; }

        public virtual ICollection<SurgeryShiftMedicalSupply> SurgeryShiftMedicalSupplies { get; set; }
    }
}
