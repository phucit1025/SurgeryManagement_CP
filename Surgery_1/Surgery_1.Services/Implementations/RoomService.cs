using System;
using System.Collections.Generic;
using Surgery_1.Data.ViewModels;
using Surgery_1.Services.Interfaces;
using Surgery_1.Data.Context;
using System.Linq;
using Surgery_1.Services.Utilities;

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

        public ReportRoomViewModel GetReportByRoom(int roomId, int dayNumber)
        {
            int totalPre = 0;
            int totalIntra = 0;
            int totalPost = 0;
            var room = _context.SurgeryRooms.Find(roomId).SlotRooms.ToList();

            foreach (var slot in room)
            {
                if (slot.SurgeryShifts.Count > 0)
                {
                    var tmpSurgeryShifts = slot.SurgeryShifts.Where(s => UtilitiesDate.ConvertDateToNumber(s.EstimatedStartDateTime.Value.Date) == dayNumber);
                    totalPre += tmpSurgeryShifts.Where(s => s.Status.Name == ConstantVariable.PRE_STATUS).Count();
                    totalIntra += tmpSurgeryShifts.Where(s => s.Status.Name == ConstantVariable.INTRA_STATUS).Count();
                    totalPost += tmpSurgeryShifts.Where(s => s.Status.Name != ConstantVariable.PRE_STATUS && s.Status.Name != ConstantVariable.INTRA_STATUS).Count();
                }
                
            }
            int totalShift = totalPre + totalIntra + totalPost;
            ReportRoomViewModel reportRoom = new ReportRoomViewModel
            {
                TotalShift = totalShift,
                TotalPre = totalPre,
                TotalIntra = totalIntra,
                TotalPost = totalPost
            };

            return reportRoom;
        }
    }
}
