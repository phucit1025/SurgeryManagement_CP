using Microsoft.EntityFrameworkCore;
using Surgery_1.Data.Context;
using Surgery_1.Data.ViewModels;
using Surgery_1.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Surgery_1.Services.Implementations
{
    public class PostOpService : IPostOpService
    {
        private readonly int RECOVERY_STATE = 4;
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

        public ICollection<SurgeryShiftViewModel> GetSurgeryByStatusId(int statusId)
        {
            var surgeryShifts = _appDbContext.SurgeryShifts.Where(a => a.StatusId == statusId).ToList();
            var results = new List<SurgeryShiftViewModel>();
            foreach (var shift in surgeryShifts)
            {
                results.Add(new SurgeryShiftViewModel()
                {
                    Id = shift.Id,
                    CatalogName = shift.SurgeryRoomCatalog.Name,
                    //EstimatedEndDateTime = $"{shift.EstimatedEndDateTime.ToShortDateString()} {shift.EstimatedEndDateTime.ToShortTimeString()}",
                    //EstimatedStartDateTime = $"{shift.EstimatedStartDateTime.ToShortDateString()} {shift.EstimatedStartDateTime.ToShortTimeString()}",
                    PatientName = shift.Patient.FullName,
                    SurgeonNames = shift.SurgeryShiftSurgeons.Select(s => s.Surgeon.FullName).ToList()
                });
            }
            return results;
        }

        public bool ChangeSurgeryShiftToRecovery(int surgeryShiftId)
        {
            var surgeryShift = _appDbContext.SurgeryShifts.Find(surgeryShiftId);
            surgeryShift.StatusId = RECOVERY_STATE;
            try
            {
                _appDbContext.Update(surgeryShift);
                _appDbContext.SaveChanges();
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }
    }
}
