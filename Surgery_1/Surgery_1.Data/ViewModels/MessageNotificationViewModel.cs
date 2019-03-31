using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Data.ViewModels
{
    public class MessageNotificationViewModel
    {
        public string Content { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsRead { get; set; }
    }

}
