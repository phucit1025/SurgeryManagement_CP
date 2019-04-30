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

        public NotificationService(AppDbContext _context)
        {
            this._context = _context;
        }

        public void AddNotificationForScheduling(List<SmsShiftViewModel> notiList)
        {
            //var resultTechnical = notiList.GroupBy(s => s.Id)
            var result = notiList.OrderBy(s => s.EstimatedStartDateTime.Date).GroupBy(s => s.EstimatedStartDateTime.Date).ToList();
            string content = "";
            if (notiList.Count > 1)
            {
                content = "There are " + notiList.Count + " surgery shifts have just been created: ";
            }
            else
            {
                content = "There is " + notiList.Count + " surgery shift has just been created: ";
            }
            foreach (var item in result)
            {
                int countNoti = item.Count();
                if (countNoti > 1)
                {
                    content += countNoti + " shifts are on " + UtilitiesDate.FormatDateShow(item.First().EstimatedStartDateTime.Date) + ", ";
                }
                else
                {
                    content += countNoti + " shift is on " + UtilitiesDate.FormatDateShow(item.First().EstimatedStartDateTime.Date) + ", ";
                }
            }
            content = content.Remove(content.Length - 2, 2) + ".";
            var notification = new Notification
            {
                Content = content,
                RoleToken = ConstantVariable.CHIEFNURSE
            };
            _context.Notifications.Add(notification);
            _context.SaveChanges();
        }

        public void AddNotificationForTechnicalStaff(List<SmsShiftViewModel> notiList)
        {
            var result = notiList.GroupBy(s => s.TechnicalId).ToList();
            foreach (var noti in result)
            {
                var count = noti.ToList().Count;
                string content = "";
                if (count > 1)
                {
                    content = "There are " + count + " surgery shifts have just been created: ";
                }
                else
                {
                    content = "There is " + count + " surgery shift has just been created: ";
                }
                foreach (var item in noti.GroupBy(s => s.EstimatedStartDateTime.Date).ToList())
                {

                    int countNoti = item.Count();
                    if (countNoti > 1)
                    {
                        content += countNoti + " shifts are on " + UtilitiesDate.FormatDateShow(item.First().EstimatedStartDateTime.Date) + ", ";
                    }
                    else
                    {
                        content += countNoti + " shift is on " + UtilitiesDate.FormatDateShow(item.First().EstimatedStartDateTime.Date) + ", ";
                    }
                }
                content = content.Remove(content.Length - 2, 2) + ".";
                var notification = new Notification
                {
                    Content = content,
                    RoleToken = ConstantVariable.TECHNICAL,
                    StaffGuid = noti.First().TechnicalId.ToString()

                };
                _context.Notifications.Add(notification);
                _context.SaveChanges();
            }

        }


        public ICollection<MessageNotificationViewModel> GetNotifications(string roleName, int technicalId)
        {
            var result = _context.Notifications.OrderByDescending(s => s.DateCreated).ToList();
            if (technicalId == 0)
            {
                result = result.Where(s => s.RoleToken.Equals(roleName)).ToList();
            }
            else
            {
                result = result.Where(s => s.RoleToken.Equals(roleName) && s.StaffGuid == technicalId.ToString()).ToList();
            }
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
            var listByPhone = smsShiftDate.GroupBy(s => s.SurgeonPhone).ToList();

            string resultSms = "";
            foreach (var item in listByPhone)
            {
                List<string> phoneList = new List<string>();
                string content = "eBSMS provides surgery schedule: \\n";
                if (item.First().SurgeonPhone == null) { continue; }
                else { phoneList.Add(item.First().SurgeonPhone); }
                foreach (var index in item.GroupBy(s => s.EstimatedStartDateTime.Date).ToList())
                {
                    content += $"=={UtilitiesDate.FormatDateShow(index.First().EstimatedStartDateTime)}== \\n";
                    foreach (var shift in index.OrderBy(s => s.EstimatedStartDateTime).ToList())
                    {
                        var nameSlotRoom = _context.SlotRooms.Find(shift.SlotRoomId).Name;
                        content += $"*Shift {shift.Id} start at {UtilitiesDate.GetTimeFromDate(shift.EstimatedStartDateTime)} - {UtilitiesDate.GetTimeFromDate(shift.EstimatedEndDateTime)} at {nameSlotRoom} \\n";
                    }
                }
                var smsSender = new SpeedSMS();
                //resultSms = smsSender.SendSms(phoneList.ToArray(), content);
            }
            return resultSms;
        }

    }
}
