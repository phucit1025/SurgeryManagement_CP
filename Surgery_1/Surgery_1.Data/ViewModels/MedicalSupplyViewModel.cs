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
        public int medicalSupplyId { get; set; }
        public String medicalSupplyName { get; set; }
        public int quantity { get; set; }
    }
}
