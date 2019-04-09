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

        public void SpecialtiesSpecialtyGroup(SpecialtySpecialtyGroupViewModel group)
        {
            var specialtyGroup = _context.SpecialtyGroups.Find(group.SpecialtyGroupId);
            foreach (var specialty in group.SpecialtyId)
            {
                var tmp = _context.Specialties.Find(specialty);
                tmp.SpecialtyGroup = specialtyGroup;
            }
            _context.SaveChanges();
        }

        public void AddSpecialtyGroup(String specialityGroupName)
        {
            var specialtyGroup = new SpecialtyGroup();
            specialtyGroup.Name = specialityGroupName;
            _context.SpecialtyGroups.Add(specialtyGroup);
            _context.SaveChanges();
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

        public void SurgeryRoomSpecialtyGroup(SurgeryRoomSpecialtyGroupViewModel groupRoom)
        {
            var specialtyGroup = _context.SpecialtyGroups.Find(groupRoom.SpecialtyGroupId);
            foreach (var room in groupRoom.SurgeryRoomId)
            {
                _context.SurgeryRooms.Find(room).SpecialtyGroup = specialtyGroup;
            }
            _context.SaveChanges();
        }
    }
}
