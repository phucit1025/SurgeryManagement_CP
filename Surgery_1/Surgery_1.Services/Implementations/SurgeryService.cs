using Surgery_1.Data.Context;
using Surgery_1.Data.ViewModels;
using Surgery_1.Repositories.Interfaces;
using Surgery_1.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Services.Implementations
{
    public class SurgeryService : ISurgeryService
    {
        private readonly ISurgeryRepository _surgeryRepo;

        
        public SurgeryService(ISurgeryRepository surgeryRepository)
        {
            _surgeryRepo = surgeryRepository;
        }

        private readonly AppDbContext _context;
        public SurgeryService(AppDbContext _context)
        {
            this._context = _context;
        }
        public void MakeSchedule(ScheduleViewModel scheduleViewModel)
        {
            
        }

        public int GetRoomByMaxSurgeryTime(ScheduleViewModel scheduleViewModel)
        {
            //Lấy ngày cần thêm lịch
            return 1;
            
        }

        
    }
}
