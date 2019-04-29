using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Surgery_1.Data.ViewModels;
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
        public string GetAllNotification(string roleName, int technicalId = 0)
        {
            string messageNotification = string.Empty;
            try
            {
                var result = _notificationService.GetNotifications(roleName, technicalId).ToList();
                
                _hubContext.Clients.All.BroadcastMessage(roleName, result);
            }
            catch (Exception e)
            {
                messageNotification = e.ToString();
            }
            return messageNotification;
        }

        [HttpGet]
        public IActionResult GetNotifications(string roleName, int technicalId = 0)
        {
            var result = _notificationService.GetNotifications(roleName, technicalId).ToList();
            return StatusCode(200, result);
        }

        [HttpPost]
        public IActionResult SetIsReadNotification(int notiId)
        {
            if (_notificationService.SetIsReadNotification(notiId))
            {
                return StatusCode(200);
            }
            
            return StatusCode(400);
        }

        [HttpPost]
        public IActionResult AddNotificationForScheduling([FromBody] List<SmsShiftViewModel> list)
        {
            _notificationService.AddNotificationForScheduling(list);
            return null;
        }

        [HttpPost]
        public IActionResult HandleSmsForSurgeon([FromBody] List<SmsShiftViewModel> smsShiftDate)
        {
            var result = _notificationService.HandleSmsForSurgeon(smsShiftDate);
            return StatusCode(200, result);
        }

    }
}