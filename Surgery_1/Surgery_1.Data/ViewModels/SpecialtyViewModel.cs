using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Data.ViewModels
{
    public class SpecialtyGroupViewModel
    {
        public int SpecialtyGroupId { get; set; }
        public String Name { get; set; }
    }

    public class SpecialtySpecialtyGroupViewModel
    {
        public int SpecialtyGroupId { get; set; }
        public ICollection<int> SpecialtyId { get; set; }
    }

    public class SurgeryRoomSpecialtyGroupViewModel
    {
        public int SpecialtyGroupId { get; set; }
        public ICollection<int> SurgeryRoomId { get; set; }
    }


}
