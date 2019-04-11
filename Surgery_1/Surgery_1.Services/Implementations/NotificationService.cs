using Microsoft.AspNetCore.Http;
using Surgery_1.Data.Context;
using Surgery_1.Data.Entities;
using Surgery_1.Data.ViewModels;
using Surgery_1.Services.Interfaces;
using Surgery_1.Services.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Surgery_1.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;

        private const string CHIEFNURSE = "ChiefNurse";
        public NotificationService(AppDbContext _context)
        {
            this._context = _context;
        }

        public void AddNotificationForScheduling(List<DateTime> dateList)
        {
            var result = dateList.OrderBy(s => s.Date).GroupBy(s => s.Date).ToList();
            string content = "";
            if (dateList.Count > 1)
            {
                content = "There are " + dateList.Count + " surgery shifts have just been created: ";
            }
            else
            {
                content = "There is " + dateList.Count + " surgery shift has just been created: ";
            }
            foreach (var item in result)
            {
                int countNoti = item.Count();
                if (countNoti > 1)
                {
                    content += countNoti + " shifts are on " + UtilitiesDate.FormatDateShow(item.First().Date) + ", ";
                }
                else
                {
                    content += countNoti + " shift is on " + UtilitiesDate.FormatDateShow(item.First().Date) + ", ";
                }
            }
            content = content.Remove(content.Length - 2, 2) + ".";
            var notification = new Notification
            {
                Content = content,
                RoleToken = CHIEFNURSE
            };
            _context.Notifications.Add(notification);
            _context.SaveChanges();
        }

        public ICollection<MessageNotificationViewModel> GetNotifications(string roleName)
        {
            var result = _context.Notifications.Where(s => s.RoleToken.Equals(roleName)).OrderByDescending(s => s.DateCreated);
            ICollection<MessageNotificationViewModel> messages = new List<MessageNotificationViewModel>();
            foreach (var message in result)
            {
                messages.Add(new MessageNotificationViewModel
                {
                    Id = message.Id,
                    Content = message.Content,
                    DateCreated = message.DateCreated.Value,
                    IsRead = message.IsRead
                });
            }
            return messages;
        }

        public bool SetIsReadNotification(int notiId)
        {
            _context.Notifications.Find(notiId).IsRead = true;
            return _context.SaveChanges() > 0 ? true : false;
        }
    }
}
