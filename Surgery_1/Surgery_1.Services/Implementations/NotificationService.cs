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
        public NotificationService(AppDbContext _context)
        {
            this._context = _context;
        }

        public void AddNotification(Notification notification)
        {
            _context.Notification.Add(notification);
            _context.SaveChanges();
        }

        public ICollection<MessageNotificationViewModel> GetNotifications()
        {
            var result = _context.Notification.OrderByDescending(s => s.DateCreated);
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
    }
}
