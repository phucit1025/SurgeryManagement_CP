using CP_SmsSender;
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



        public string HandleSmsForSurgeon(List<SmsShiftViewModel> smsShiftDate)
        {
            var sortedShift = smsShiftDate.OrderBy(s => s.EstimatedStartDateTime.Date);
            var result = sortedShift.GroupBy(s => s.EstimatedStartDateTime.Date).ToList();
            string content = "eBSMS provides surgery schedule for you: \\n";

            foreach (var item in result)
            {
                content += $"{UtilitiesDate.FormatDateShow(item.First().EstimatedStartDateTime)}: \\n";
                foreach (var shift in sortedShift)
                {
                    if (shift.EstimatedStartDateTime.Date == item.First().EstimatedStartDateTime.Date)
                    {
                        var nameSlotRoom = _context.SlotRooms.Find(shift.SlotRoomId).Name;
                        content += $"- Shift No {shift.Id} start at: {UtilitiesDate.GetTimeFromDate(shift.EstimatedStartDateTime)} - {UtilitiesDate.GetTimeFromDate(shift.EstimatedStartDateTime)} in room {nameSlotRoom} \\n";
                    }
                }
            }

            var smsSender = new SpeedSMS();
            string[] phoneList = { "0326622807" }; //"0764644363"
            var resultSms = smsSender.SendSms(phoneList , content);
            return resultSms;
        }
    }
}
