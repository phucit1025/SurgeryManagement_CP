using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Data.ViewModels
{
    public class TreatmentReportViewModel
    {
        public int Id { get; set; }
        public string DateCreated { get; set; }
        public string ProgressiveDisease { get; set; }
        public int ShiftId { get; set; }
        public ICollection<TreatmentReportDrugViewModel> TreatmentReportDrugs { get; set; }
    }

    public class TreatmentReportDrugViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TreatmentReportId { get; set; }
        public int DrugId { get; set; }
        public int MorningQuantity { get; set; }
        public int AfternoonQuantity { get; set; }
        public int EveningQuantity { get; set; }
        public int NightQuantity { get; set; }
    }

    public class TreatmentMedication
    {
        public int time { get; set; }
        public ICollection<TreatmentReportDrugViewModel> drugs { get; set; }
    }
}
