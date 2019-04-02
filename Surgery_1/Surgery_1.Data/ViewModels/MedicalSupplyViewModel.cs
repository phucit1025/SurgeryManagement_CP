using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Data.ViewModels
{
    public class MedicalSupplyViewModel
    {
        public int surgeryShiftId { get; set; }
        public int medicalSupplyId { get; set; }
        public String medicalSupplyName { get; set; }

    }

    public class MedicalSupplyInfoViewModel
    {   
        public int id { get; set; }
        public int medicalSupplyId { get; set; }
        public String medicalSupplyName { get; set; }
        public int quantity { get; set; }
    }

    public class ShiftMedicalSupplyViewModel
    {
        public int MedicalSupplyId { get; set; }
        public int SurgeryShiftId { get; set; }
        public int Quantity { get; set; }
    }

    public class ShiftMedicalSuppliesViewModel
    {
        public ICollection<ShiftMedicalSupplyViewModel> ShiftMedicals { get; set; }
        public ICollection<int> DeleteMedicalSupplyIds { get; set; }
    }
}
