using Surgery_1.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Services.Interfaces
{
    public interface ISpecialtyService
    {
        #region Catalog => Specialty
        ICollection<SpecialtyViewModel> GetSpecialties();
        ICollection<SpecialtyViewModel> GetSpecialties(int groupId);
        ICollection<SurgeryCatalogViewModel> GetCatalogs();
        bool SetCatalogToSpecialty(CatalogToSpecialtyViewModel model);
        int CreateSpecialty(string name);
        #endregion

        #region Specialty => Group
        ICollection<SpecialtyGroupViewModel> GetSpecialtyGroups();
        int CreateSpecialtyGroup(string SpecialtyGroupName);
        bool AddSpecialtyToGroup(SpecialtySpecialtyGroupViewModel group);
        #endregion

        #region Group => Room
        ICollection<SurgeryRoomSpecialtyViewModel> GetRooms();
        bool SetSpecialtyToRoom(SurgeryRoomSpecialtyGroupCreateViewModel groupRoom);
        #endregion



    }
}
