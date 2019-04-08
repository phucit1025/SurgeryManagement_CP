using System;
using System.Collections.Generic;
using Surgery_1.Data.ViewModels;
namespace Surgery_1.Services.Interfaces
{
    public interface IRoomService
    {
        ICollection<RoomViewModel> GetRooms();
        RoomViewModel GetRoom(int id);
        ICollection<string> GetSpecialtyGroupByRoomId(int roomId);
    }
}
