using Microsoft.AspNetCore.SignalR;
using Surgery_1.Data.Context;
using Surgery_1.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Surgery_1.Hubs
{
    public class NotifyHub : Hub<ITypedHubClient>
    {
    }
}
