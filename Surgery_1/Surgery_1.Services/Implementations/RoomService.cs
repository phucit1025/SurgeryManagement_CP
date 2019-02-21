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
    }
}
