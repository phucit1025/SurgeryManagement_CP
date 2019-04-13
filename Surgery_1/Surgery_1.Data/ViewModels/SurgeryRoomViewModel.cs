using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Data.ViewModels
{
    public class SurgeryRoomViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int SpecialtyGroupId { get; set; }
        public string SpecialtyGroupName { get; set; }

        public ICollection<SlotRoomViewModel> SlotRooms { get; set; }
    }

    public class ReportRoomViewModel
    {
        public int TotalShift { get; set; }
        public int TotalPre { get; set; }
        public int TotalIntra { get; set; }
        public int TotalPost { get; set; }
    }

    public class SlotRoomViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ShiftSlotRoomViewModel
    {
        public int SlotId { get; set; }
        public int NumberOfShift { get; set; }
    }
}
