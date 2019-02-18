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

        public DateTime MakeSchedule(int a)
        {
            var doctor = _surgeryRepo.GetSurgeon();
            return DateTime.Now;
        }
    }
}
