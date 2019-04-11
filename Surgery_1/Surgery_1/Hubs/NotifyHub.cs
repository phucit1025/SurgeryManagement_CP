using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Surgery_1.Data.Context;
using Surgery_1.Data.ViewModels;
using Surgery_1.Services.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surgery_1.Hubs
{
    public class NotifyHub : Hub<ITypedHubClient>
    {
        private readonly INotificationService _notificationService;
        private readonly static ConnectionMapping<string> _connections =
           new ConnectionMapping<string>();

        //public void PushNotification(string roleName)
        //{

        //    string name = Context.User.Identity.Name;
        //    var result = _notificationService.GetNotifications(roleName).ToList();
        //    foreach (var connectionId in _connections.GetConnections(roleName))
        //    {
        //        Clients.Client(connectionId).BroadcastMessage(result);
        //    }
        //}

        //public override Task OnConnectedAsync()
        //{
        //    string roleName = Context.User.Identity.Name;
        //    _connections.Add(roleName, Context.ConnectionId);
        //    return base.OnConnectedAsync();
        //}
        
        //public override Task OnDisconnectedAsync(Exception stopCalled)
        //{
        //    string name = Context.User.Identity.Name;
        //    _connections.Remove(name, Context.ConnectionId);

        //    return base.OnDisconnectedAsync(stopCalled);
        //}

        

    }

}
