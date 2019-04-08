using System;
using System.Collections.Generic;
using Surgery_1.Data.ViewModels;
using Surgery_1.Services.Interfaces;
using Surgery_1.Data.Context;
using System.Linq;
namespace Surgery_1.Services.Implementations
{
    public class RoomService : IRoomService
    {
        private readonly AppDbContext _context;

        public RoomService(AppDbContext context)
        {
            _context = context;
        }

        public RoomViewModel GetRoom(int id)
        {
            var room = _context.SurgeryRooms.Find(id);
            return new RoomViewModel()
            {
                Id = id,
                RoomName = room.Name
            };
        }

        public ICollection<RoomViewModel> GetRooms()
        {
            var rooms = _context.SurgeryRooms.Where(r => !r.IsDeleted);
            var results = new List<RoomViewModel>();
            if (rooms != null)
            {
                foreach (var room in rooms.ToList())
                {
                    results.Add(new RoomViewModel()
                    {
                        Id = room.Id,
                        RoomName = room.Name,
                    });
                }
            }
            return results;
        }

        public ICollection<string> GetSpecialtyGroupByRoomId(int roomId)
        {
            var specialties = _context.SurgeryRooms.Find(roomId).SpecialtyGroup.Specialties.Select(s => s.Name).ToList();
            return specialties;
        }

    }
}
