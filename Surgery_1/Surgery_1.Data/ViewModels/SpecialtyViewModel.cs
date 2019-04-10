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

    public class SurgeryRoomSpecialtyGroupCreateViewModel
    {
        public int SpecialtyGroupId { get; set; }
        public ICollection<int> SurgeryRoomId { get; set; }
    }

    public class SpecialtyGroupCreateViewModel
    {
        public string Name { get; set; }
    }

    public class SpecialtyCreateViewModel
    {
        public string Name { get; set; }
    }

    public class SpecialtyViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
    }

    public class CatalogToSpecialtyViewModel
    {
        public int SpecialtyId { get; set; }
        public List<int> CatalogIds { get; set; }
    }

    public class SurgeryCatalogViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SpecialtyId { get; set; }
        public string SpecialtyName { get; set; }
    }

    public class SurgeryRoomSpecialtyViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int GroupId { get; set; }
        public string GroupName { get; set; }
    }
}
