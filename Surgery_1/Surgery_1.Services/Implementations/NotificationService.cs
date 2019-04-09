using Microsoft.AspNetCore.Http;
using Surgery_1.Data.Context;
using Surgery_1.Data.Entities;
using Surgery_1.Data.ViewModels;
using Surgery_1.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Surgery_1.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public NotificationService(AppDbContext _context, IHttpContextAccessor _httpContextAccessor)
        {
            this._context = _context;
            this._httpContextAccessor = _httpContextAccessor;
        }

        public void AddNotification(Notification notification)
        {
            //var notification = new Notification
            //{
            //    Content = "There are " + countNotify + " new medical supplies request need to be confirmed",
            //};
            //_context.Notification.Add(notification);
            //_context.SaveChanges();
            //_context.Notification.Add(notification);
            //_context.SaveChanges();
        }

        public ICollection<MessageNotificationViewModel> GetNotifications(string roleName)
        {
            var result = _context.Notifications.Where(s => s.RoleToken.Equals(roleName)).OrderByDescending(s => s.DateCreated);
            ICollection<MessageNotificationViewModel> messages = new List<MessageNotificationViewModel>();
            foreach(var message in result)
            {
                messages.Add(new MessageNotificationViewModel {
                    Content = message.Content,
                    DateCreated = message.DateCreated.Value,
                    IsRead = message.IsRead
                });
            }
            return messages;
        }

        public bool SetIsReadNotification(string roleName)
        {
            _context.Notifications.Where(s => s.RoleToken.Equals(roleName)).ToList().ForEach(item => { item.IsRead = true; });
            return _context.SaveChanges() > 0 ? true : false;
        }
    }
}
