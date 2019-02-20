using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Data.ViewModels
{
    public class SurgeryShiftViewModel
    {
        public int Id { get; set; }
        public string CatalogName { get; set; }
        public List<string> SurgeonNames { get; set; }
        public string PatientName { get; set; }
        public string EstimatedStartDateTime { get; set; }
        public string EstimatedEndDateTime { get; set; }
    }
}
