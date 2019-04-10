using Surgery_1.Data.Context;
using Surgery_1.Data.Entities;
using Surgery_1.Data.ViewModels;
using Surgery_1.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Surgery_1.Services.Implementations
{
    public class SpecialityService : ISpecialityService
    {
        private readonly AppDbContext _context;
        public SpecialityService(AppDbContext _context)
        {
            this._context = _context;
        }

        public bool AddSpecialityToGroup(SpecialitySpecialityGroupViewModel group)
        {
            try
            {
                foreach (var sId in group.SpecialityId)
                {
                    var speciality = _context.Specialities.Find(sId);
                    speciality.SpecialityGroupId = group.SpecialityGroupId;
                    _context.Update(speciality);
                }
                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public int CreateSpeciality(string name)
        {
            try
            {
                var newSpeciality = new Speciality()
                {
                    Name = name
                };
                _context.Specialities.Add(newSpeciality);
                _context.SaveChanges();
                return newSpeciality.Id;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public int CreateSpecialityGroup(string specialityGroupName)
        {
            try
            {
                var SpecialityGroup = new SpecialityGroup();
                SpecialityGroup.Name = specialityGroupName;
                _context.SpecialityGroups.Add(SpecialityGroup);
                _context.SaveChanges();
                return SpecialityGroup.Id;
            }
            catch (Exception)
            {
                return 0;
            }

        }

        public ICollection<SurgeryCatalogViewModel> GetCatalogs()
        {
            var catalogs = _context.SurgeryCatalogs.Where(c => !c.IsDeleted);
            if (!catalogs.Any())
            {
                return new List<SurgeryCatalogViewModel>();
            }
            else
            {
                return catalogs.Select(c => new SurgeryCatalogViewModel()
                {
                    Id = c.Id,
                    Name = c.Name,
                    SpecialityId = c.SpecialityId,
                    SpecialityName = c.Speciality.Name,
                }).ToList();
            }
        }

        public ICollection<SurgeryRoomSpecialityViewModel> GetRooms()
        {
            var rooms = _context.SurgeryRooms.Where(s => !s.IsDeleted);
            if (rooms.Any())
            {
                return rooms.Select(r => new SurgeryRoomSpecialityViewModel()
                {
                    Id = r.Id,
                    Name = r.Name,
                    GroupId = r.SpecialityGroupId.GetValueOrDefault(0),
                    GroupName = r.SpecialityGroup == null ? "" : r.SpecialityGroup.Name
                }).ToList();
            }
            else
            {
                return new List<SurgeryRoomSpecialityViewModel>();
            }
        }

        public ICollection<SpecialityViewModel> GetSpecialities()
        {
            var specialities = _context.Specialities.Where(s => !s.IsDeleted);
            if (specialities.Any())
            {
                return specialities.Select(c => new SpecialityViewModel()
                {
                    Id = c.Id,
                    GroupId = c.SpecialityGroupId.GetValueOrDefault(0),
                    Name = c.Name,
                    GroupName = c.SpecialityGroup == null ? "" : c.SpecialityGroup.Name
                }).ToList();
            }
            else
            {
                return new List<SpecialityViewModel>();
            }
        }

        public ICollection<SpecialityViewModel> GetSpecialities(int groupId)
        {
            var specialities = _context.Specialities.Where(s => !s.IsDeleted && s.SpecialityGroupId == groupId);
            if (specialities.Any())
            {
                return specialities.Select(c => new SpecialityViewModel()
                {
                    Id = c.Id,
                    GroupId = c.SpecialityGroupId.GetValueOrDefault(0),
                    Name = c.Name,
                    GroupName = c.SpecialityGroup == null ? "" : c.SpecialityGroup.Name
                }).ToList();
            }
            else
            {
                return new List<SpecialityViewModel>();
            }
        }

        public ICollection<SpecialityGroupViewModel> GetSpecialityGroups()
        {
            var SpecialityGroupList = new List<SpecialityGroupViewModel>();
            var list = _context.SpecialityGroups.Where(a => a.IsDeleted == false).ToArray();
            foreach (var sg in list)
            {
                var vm = new SpecialityGroupViewModel();
                vm.SpecialityGroupId = sg.Id;
                vm.Name = sg.Name;
                SpecialityGroupList.Add(vm);
            }
            return SpecialityGroupList;
        }

        public bool SetCatalogToSpeciality(CatalogToSpecialityViewModel model)
        {
            try
            {
                foreach (var catalogId in model.CatalogIds)
                {
                    var catalog = _context.SurgeryCatalogs.Find(catalogId);
                    catalog.SpecialityId = model.SpecialityId;
                    _context.Update(catalog);
                }
                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool SetSpecialityToRoom(SurgeryRoomSpecialityGroupCreateViewModel groupRoom)
        {
            try
            {
                foreach (var roomId in groupRoom.SurgeryRoomId)
                {
                    var room = _context.SurgeryRooms.Find(groupRoom.SurgeryRoomId);
                    room.SpecialityGroupId = groupRoom.SpecialityGroupId;
                    _context.Update(room);
                }
                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
