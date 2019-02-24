using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Data.ViewModels
{
    public class MedicalSupplyRequestViewModel
    {
        public int Id { get; set; }
        public string PatientName { get; set; }
        public string SurgeryName { get; set; }
        public string SurgeryCatalogId { get; set; }
        public string CreatedDate { get; set; }
    }

    public class MedicalSupplyDetailImportViewModel
    {
        public int supplyId { get; set; }
        public int surgeryShiftId { get; set; }
        public int quantity { get; set; }
    }

    public class MedicalSupplyRequestDetailViewModel
    {
        public int code { get; set; }
        public string name { get; set; }
        public int quantity { get; set; }
    }
}
