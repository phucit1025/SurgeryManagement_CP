using System;
namespace Surgery_1.Data.ViewModels
{
    public class RoomViewModel
    {
        public int Id { get; set; }
        public string RoomName { get; set; }
        public int Capacity { get; set; }
    }

    public class AvailableRoomViewModel
    {
        public int RoomId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
    }

    public class AvailableRoomParamViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

    }
}
