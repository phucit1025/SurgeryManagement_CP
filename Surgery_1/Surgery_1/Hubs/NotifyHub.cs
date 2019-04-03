using Microsoft.AspNetCore.SignalR;
using Surgery_1.Data.Context;
using Surgery_1.Data.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surgery_1.Hubs
{
    public class NotifyHub : Hub<ITypedHubClient>
    {
        private static readonly ConcurrentDictionary<string, UserHubModels> Users =
        new ConcurrentDictionary<string, UserHubModels>(StringComparer.InvariantCultureIgnoreCase);
        //public override Task OnConnectedAsync()
        //{
        //    string username = Context.User.Identity.Name;
        //    string connectionId = Context.ConnectionId;

        //    var user = Users.GetOrAdd(username, _ => new UserHubModels
        //    {
        //        roleName = username,
        //        ConnectionIds = new HashSet<string>()
        //    });

        //    lock (user.ConnectionIds)
        //    {
        //        user.ConnectionIds.Add(connectionId);
        //        if (user.ConnectionIds.Count == 1)
        //        {
        //            Clients.Others.ConnectUser(username);
        //        }
        //    }

        //    return base.OnConnectedAsync();
        //}
    }
}
