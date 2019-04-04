using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Surgery_1.Hubs;
using Surgery_1.Services.Interfaces;

namespace Surgery_1.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MessageNotificationController : ControllerBase
    {
        private IHubContext<NotifyHub, ITypedHubClient> _hubContext;
        private readonly INotificationService _notificationService;
        public MessageNotificationController(IHubContext<NotifyHub, ITypedHubClient> hubContext, INotificationService notificationService)
        {
            _hubContext = hubContext;
            _notificationService = notificationService;
        }

        [HttpGet]
        public string GetAllNotification(string roleName)
        {
            string messageNotification = string.Empty;
            try
            {
                var result = _notificationService.GetNotifications(roleName);
                _hubContext.Clients.All.GetNotifications(roleName, result.ToList());
            }
            catch (Exception e)
            {
                messageNotification = e.ToString();
            }
            return messageNotification;
        }

        [HttpGet]
        public IActionResult GetNotifications(string roleName)
        {
            var result = _notificationService.GetNotifications(roleName).ToList();
            return StatusCode(200, result);
        }

        [HttpPost]
        public IActionResult SetIsReadNotification(string roleName)
        {
            if (_notificationService.SetIsReadNotification(roleName))
            {
                return StatusCode(200);
            }
            
            return StatusCode(400);
        }
    

    }
}