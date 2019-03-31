using Surgery_1.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surgery_1.Hubs
{
    public interface ITypedHubClient
    {
        Task BroadcastMessage(string type, string payload);

        Task GetNotifications(List<MessageNotificationViewModel> messages);




    }
}
