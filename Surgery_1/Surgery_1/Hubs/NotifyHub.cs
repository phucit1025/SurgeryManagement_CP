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
    }

}
