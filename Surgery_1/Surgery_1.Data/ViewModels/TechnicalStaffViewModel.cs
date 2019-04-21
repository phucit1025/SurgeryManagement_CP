using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Data.ViewModels
{
    public class TechnicalStaffViewModel
    {
        public int technicalStaffId { get; set; }
        public int surgeryId { get; set; }
    }

    public class TechnicalStaffInfoViewModel
    {
        public int technicalStaffId { get; set; }
        public String technicalStaffName { get; set; }
    }
}
