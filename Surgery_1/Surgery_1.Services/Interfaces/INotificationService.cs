using Surgery_1.Data.Entities;
using Surgery_1.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Services.Interfaces
{
    public interface INotificationService
    {
        ICollection<MessageNotificationViewModel> GetNotifications(string tokenRole, int technicalId);
        bool SetIsReadNotification(int notiId);

        void AddNotificationForScheduling(List<SmsShiftViewModel> notiShift);

        string HandleSmsForSurgeon(List<SmsShiftViewModel> smsShiftDate);
        void AddNotificationForTechnicalStaff(List<SmsShiftViewModel> notiList);
    }
}
