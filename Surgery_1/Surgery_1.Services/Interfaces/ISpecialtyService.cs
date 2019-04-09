using Surgery_1.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Services.Interfaces
{
    public interface ISpecialtyService
    {
        void SpecialtiesSpecialtyGroup(SpecialtySpecialtyGroupViewModel group);
        void AddSpecialtyGroup(String specialityGroupName);
        ICollection<SpecialtyGroupViewModel> GetSpecialtyGroups();
        void SurgeryRoomSpecialtyGroup(SurgeryRoomSpecialtyGroupViewModel groupRoom);
    }
}
