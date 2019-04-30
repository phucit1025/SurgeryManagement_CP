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
        public string TreatmentRequirement { get; set; }
        public int ShiftId { get; set; }
        public bool IsUsed { get; set; }
        public ICollection<TreatmentReportDrugViewModel> TreatmentReportDrugs { get; set; }
        public ICollection<int> DeleteTreatmentReportId { get; set; }
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
        public string[] TimeString { get; set; }
        public string[] StatusString { get; set; }
        public string Route { get; set; }
        public string Unit { get; set; }
        public bool IsUsed { get; set; }
        public bool IsDeleted { get; set; }
        public string StatusUsedBy { get; set; }
    }

    public class TreatmentMedication
    {
        public int time { get; set; }
        public ICollection<TreatmentReportDrugViewModel> drugs { get; set; }
    }

    public class TreatmentTimelineViewModel
    {
        public string Name { get; set; }
        public string Unit { get; set; }
        public string Route { get; set; }
        public int Quantity { get; set; }
        public int IsUsed { get; set; }
        public int TreatmentDrugId { get; set; }
    }
}
