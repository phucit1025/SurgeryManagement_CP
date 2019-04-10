using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Data.ViewModels
{
    public class SpecialityGroupViewModel
    {
        public int SpecialityGroupId { get; set; }
        public String Name { get; set; }
    }

    public class SpecialitySpecialityGroupViewModel
    {
        public int SpecialityGroupId { get; set; }
        public ICollection<int> SpecialityId { get; set; }
    }

    public class SurgeryRoomSpecialityGroupCreateViewModel
    {
        public int SpecialityGroupId { get; set; }
        public ICollection<int> SurgeryRoomId { get; set; }
    }

    public class SpecialityGroupCreateViewModel
    {
        public string Name { get; set; }
    }

    public class SpecialityCreateViewModel
    {
        public string Name { get; set; }
    }

    public class SpecialityViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
    }

    public class CatalogToSpecialityViewModel
    {
        public int SpecialityId { get; set; }
        public List<int> CatalogIds { get; set; }
    }

    public class SurgeryCatalogViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SpecialityId { get; set; }
        public string SpecialityName { get; set; }
    }

    public class SurgeryRoomSpecialityViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
    }
}
