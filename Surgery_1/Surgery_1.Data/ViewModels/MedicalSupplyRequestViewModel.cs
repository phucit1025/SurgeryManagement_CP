using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Data.ViewModels
{
    public class MedicalSupplyRequestViewModel
    {
        public int Id { get; set; }
        public string PatientName { get; set; }
        public string SpecialtyName { get; set; }
        public string SurgeryName { get; set; }
        public string SurgeryCatalogId { get; set; }
        public string CreatedDate { get; set; }
        public ICollection<MedicalSupplyRequestDetailViewModel> MedicalSupplies { get; set; }
    }

    public class MedicalSupplyDetailImportViewModel
    {
        public int supplyId { get; set; }
        public int surgeryShiftId { get; set; }
        public int quantity { get; set; }
    }

    public class MedicalSupplyRequestDetailViewModel
    {   
        public int id { get; set; }
        public int supplyId { get; set; }
        public string supplyName { get; set; }
        public int quantity { get; set; }
    }
    public class MedicalSupplyIdConfirmViewModel
    {
        public int id { get; set; }
    }
}
