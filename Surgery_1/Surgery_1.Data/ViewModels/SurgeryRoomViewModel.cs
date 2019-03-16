﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Data.ViewModels
{
    public class SurgeryRoomViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<SlotRoomViewModel> SlotRooms { get; set; }
    }

    public class SlotRoomViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
