using Surgery_1.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Services.Interfaces
{
    public interface ISpecialityService
    {
        #region Catalog => Speciality
        ICollection<SpecialityViewModel> GetSpecialities();
        ICollection<SpecialityViewModel> GetSpecialities(int groupId);
        ICollection<SurgeryCatalogViewModel> GetCatalogs();
        bool SetCatalogToSpeciality(CatalogToSpecialityViewModel model);
        int CreateSpeciality(string name);
        #endregion

        #region Speciality => Group
        ICollection<SpecialityGroupViewModel> GetSpecialityGroups();
        int CreateSpecialityGroup(string specialityGroupName);
        bool AddSpecialityToGroup(SpecialitySpecialityGroupViewModel group);
        #endregion

        #region Group => Room
        ICollection<SurgeryRoomSpecialityViewModel> GetRooms();
        bool SetSpecialityToRoom(SurgeryRoomSpecialityGroupCreateViewModel groupRoom);
        #endregion



    }
}
