using System;
using System.Collections;
using System.Collections.Generic;

namespace Surgery_1.Data.ViewModels
{
    public class EkipMemberViewModel
    {
        public string Name { get; set; }
        public string WorkJob { get; set; }
    }

    public class SurgeonsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class UpdateSurgeonsViewModel
    {
        public int surgeryShiftId { get; set; }
        public ICollection<int> surgeonIds { get; set; }
    }
}
