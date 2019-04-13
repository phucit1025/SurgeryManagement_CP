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
    public class SpecialtyService : ISpecialtyService
    {
        private readonly AppDbContext _context;
        public SpecialtyService(AppDbContext _context)
        {
            this._context = _context;
        }

        public bool AddSpecialtyToGroup(SpecialtySpecialtyGroupViewModel group)
        {
            try
            {
                foreach (var sId in group.SpecialtyId)
                {
                    var Specialty = _context.Specialties.Find(sId);
                    Specialty.SpecialtyGroupId = group.SpecialtyGroupId;
                    _context.Update(Specialty);
                }
                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public int CreateSpecialty(string name)
        {
            try
            {
                var newSpecialty = new Specialty()
                {
                    Name = name
                };
                _context.Specialties.Add(newSpecialty);
                _context.SaveChanges();
                return newSpecialty.Id;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public int CreateSpecialtyGroup(string SpecialtyGroupName)
        {
            try
            {
                if (_context.SpecialtyGroups.Any(c => c.Name.Equals(SpecialtyGroupName, StringComparison.CurrentCultureIgnoreCase)))
                {
                    return 0;
                }
                else
                {
                    var SpecialtyGroup = new SpecialtyGroup();
                    SpecialtyGroup.Name = SpecialtyGroupName;
                    _context.SpecialtyGroups.Add(SpecialtyGroup);
                    _context.SaveChanges();
                    return SpecialtyGroup.Id;
                }

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
                    SpecialtyId = c.SpecialtyId,
                    SpecialtyName = c.Specialty.Name,
                }).ToList();
            }
        }

        public ICollection<SurgeryRoomSpecialtyViewModel> GetRooms()
        {
            var rooms = _context.SurgeryRooms.Where(s => !s.IsDeleted);
            if (rooms.Any())
            {
                return rooms.Select(r => new SurgeryRoomSpecialtyViewModel()
                {
                    Id = r.Id,
                    Name = r.Name,
                    GroupId = r.SpecialtyGroupId.GetValueOrDefault(0),
                    GroupName = r.SpecialtyGroup == null ? "" : r.SpecialtyGroup.Name
                }).ToList();
            }
            else
            {
                return new List<SurgeryRoomSpecialtyViewModel>();
            }
        }

        public ICollection<SpecialtyViewModel> GetSpecialties()
        {
            var Specialties = _context.Specialties.Where(s => !s.IsDeleted);
            if (Specialties.Any())
            {
                return Specialties.Select(c => new SpecialtyViewModel()
                {
                    Id = c.Id,
                    GroupId = c.SpecialtyGroupId.GetValueOrDefault(0),
                    Name = c.Name,
                    GroupName = c.SpecialtyGroup == null ? "" : c.SpecialtyGroup.Name
                }).ToList();
            }
            else
            {
                return new List<SpecialtyViewModel>();
            }
        }

        public ICollection<SpecialtyViewModel> GetSpecialties(int groupId)
        {
            var Specialties = _context.Specialties.Where(s => !s.IsDeleted && s.SpecialtyGroupId == groupId);
            if (Specialties.Any())
            {
                return Specialties.Select(c => new SpecialtyViewModel()
                {
                    Id = c.Id,
                    GroupId = c.SpecialtyGroupId.GetValueOrDefault(0),
                    Name = c.Name,
                    GroupName = c.SpecialtyGroup == null ? "" : c.SpecialtyGroup.Name
                }).ToList();
            }
            else
            {
                return new List<SpecialtyViewModel>();
            }
        }

        public ICollection<SpecialtyGroupViewModel> GetSpecialtyGroups()
        {
            var SpecialtyGroupList = new List<SpecialtyGroupViewModel>();
            var list = _context.SpecialtyGroups.Where(a => a.IsDeleted == false).ToArray();
            foreach (var sg in list)
            {
                var vm = new SpecialtyGroupViewModel();
                vm.SpecialtyGroupId = sg.Id;
                vm.Name = sg.Name;
                SpecialtyGroupList.Add(vm);
            }
            return SpecialtyGroupList;
        }

        public bool SetCatalogToSpecialty(CatalogToSpecialtyViewModel model)
        {
            try
            {
                foreach (var catalogId in model.CatalogIds)
                {
                    var catalog = _context.SurgeryCatalogs.Find(catalogId);
                    catalog.SpecialtyId = model.SpecialtyId;
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

        public bool SetSpecialtyToRoom(SurgeryRoomSpecialtyGroupCreateViewModel groupRoom)
        {
            try
            {
                foreach (var roomId in groupRoom.SurgeryRoomId)
                {
                    var room = _context.SurgeryRooms.Find(groupRoom.SurgeryRoomId);
                    room.SpecialtyGroupId = groupRoom.SpecialtyGroupId;
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
