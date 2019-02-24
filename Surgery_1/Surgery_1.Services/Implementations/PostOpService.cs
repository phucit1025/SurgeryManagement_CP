using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Surgery_1.Data.Context;
using Surgery_1.Data.Entities;
using Surgery_1.Data.ViewModels;
using Surgery_1.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Surgery_1.Services.Implementations
{
    public class PostOpService : IPostOpService
    {
        private readonly int RECOVERY_STATE = 4;
        private CultureInfo provider = CultureInfo.InvariantCulture;
        private readonly AppDbContext _appDbContext;
        private readonly ILogger _logger;
        public PostOpService(AppDbContext _appDbContext, ILogger<PostOpService> _logger)
        {
            this._appDbContext = _appDbContext;
            this._logger = _logger;
        }

        public object GetHealthCareRerportBySurgeryShiftId(int surgeryShiftId)
        {
            var result = _appDbContext.HealthCareReports.Where(a => a.SurgeryShiftId == surgeryShiftId).ToList();
            return result;
        }

        public ICollection<SurgeryShiftViewModel> GetSurgeryByStatusId(int statusId)
        {
            var surgeryShifts = _appDbContext.SurgeryShifts
                .Where(a => a.StatusId == statusId && a.IsDeleted == false)
                .ToList();
            var results = new List<SurgeryShiftViewModel>();
            foreach (var shift in surgeryShifts)
            {
                results.Add(new SurgeryShiftViewModel()
                {
                    Id = shift.Id,
                    CatalogName = shift.SurgeryRoomCatalog.Name,
                    PatientName = shift.Patient.FullName
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

        public RecoverySurgeryShiftViewModel GetRecoverySurgeryShiftById(int id)
        {
            var surgeryShift = _appDbContext.SurgeryShifts.Find(id);
            var patient = surgeryShift.Patient;
            var surgeryCatalog = surgeryShift.SurgeryRoomCatalog;

            var healthCareReports = _appDbContext.HealthCareReports
                .Where(a => a.SurgeryShiftId == surgeryShift.Id && a.IsDeleted == false)
                .OrderByDescending(a => a.DateCreated)
                .ToList();
            var healthCareReportsViewModelList = new List<HealthCareReportViewModel>();
            foreach (var healthCareRerport in healthCareReports)
            {
                healthCareReportsViewModelList.Add(new HealthCareReportViewModel()
                {
                    Id = healthCareRerport.Id,
                    DateCreated = healthCareRerport.DateCreated.Value.ToString("dd/MM/yyyy HH:mm:ss"),
                    VisitReason = healthCareRerport.EventContent,
                    EventContent = healthCareRerport.EventContent,
                    CareContent = healthCareRerport.CareContent,
                    SurgeryShiftId = healthCareRerport.SurgeryShiftId
                });
            }
            RecoverySurgeryShiftViewModel recoverySurgeryShiftViewModel = new RecoverySurgeryShiftViewModel()
            {
                Id = surgeryShift.Id,
                PatientName = patient.FullName,
                PatientAge = DateTime.Now.Year - patient.YearOfBirth,
                PatientGender = patient.Gender,
                Diagnose = surgeryCatalog.Name,
                CareReports = healthCareReportsViewModelList
            };
            return recoverySurgeryShiftViewModel;
        }

        public bool CreateHealthCareReport(HealthCareReportViewModel healthCareReportViewModel)
        {
            var healthCareReport = new HealthCareReport()
            {
                //DateCreated = DateTime.ParseExact(healthCareReportViewModel.DateCreated, "ddd, dd-MM-yyyy hh:mm", provider),
                EventContent = healthCareReportViewModel.EventContent,
                CareContent = healthCareReportViewModel.CareContent,
                RoomNumber = 1,
                BedNumber = 1,
                IsDeleted = false,
                IsInRecoveryState = true,
                SurgeryShiftId = healthCareReportViewModel.SurgeryShiftId
            };
            try
            {
                _appDbContext.Add(healthCareReport);
                _appDbContext.SaveChanges();
                return true;
            }
            catch (Exception)
            {

                return false;
            }
                
        }

        public bool SoftDeleteHealthCareReport(int healthCareReportId)
        {
            var healthCareReport = _appDbContext.HealthCareReports.Find(healthCareReportId);
            healthCareReport.IsDeleted = true;
            try
            {
                _appDbContext.Update(healthCareReport);
                _appDbContext.SaveChanges();
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }

        public bool UpdateHealthCareReport(HealthCareReportViewModel healthCareReportViewModel)
        {
            var healthCareReport = _appDbContext.HealthCareReports.Find(healthCareReportViewModel.Id);
            healthCareReport.EventContent = healthCareReportViewModel.EventContent;
            healthCareReport.CareContent = healthCareReportViewModel.CareContent;
            DateTime date = new DateTime();
            healthCareReport.DateUpdated = date;
            try
            {
                _appDbContext.Update(healthCareReport);
                _appDbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Update Health Care Report", ex);
                return false;
            }

        }
    }
}
