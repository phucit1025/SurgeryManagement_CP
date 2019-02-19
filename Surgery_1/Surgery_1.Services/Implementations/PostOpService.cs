using Surgery_1.Data.Context;
using Surgery_1.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Surgery_1.Services.Implementations
{
    public class PostOpService : IPostOpService
    {
        private readonly AppDbContext _appDbContext;
        public PostOpService(AppDbContext _appDbContext)
        {
            this._appDbContext = _appDbContext;
        }

        public object GetHealthCareRerportBySurgeryShiftId(int surgeryShiftId)
        {
            var result = _appDbContext.HealthCareReports.Where(a => a.SurgeryShiftId == surgeryShiftId).ToList();
            return result;
        }

        public object GetSurgeryByStatusId(int statusId)
        {
            var result = _appDbContext.SurgeryShifts.Where(a => a.StatusId == statusId).ToList();
            return result;
        }
    }
}
