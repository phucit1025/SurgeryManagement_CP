using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Surgery_1.Data.Context;
using Surgery_1.Data.Entities;
using Surgery_1.Data.ViewModels;
using Surgery_1.Services.Interfaces;
using Surgery_1.Services.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Surgery_1.Services.Implementations
{
    public class PostOpService : IPostOpService
    {
        private readonly int RECOVERY_STATE = 6;
        private CultureInfo provider = CultureInfo.InvariantCulture;
        private readonly AppDbContext _appDbContext;
        private readonly ILogger _logger;
        public PostOpService(AppDbContext _appDbContext, ILogger<PostOpService> _logger)
        {
            this._appDbContext = _appDbContext;
            this._logger = _logger;
        }

        public ICollection<HealthCareReportViewModel> GetHealthCareRerportBySurgeryShiftId(int surgeryShiftId)
        {
            var healthCareReports = _appDbContext.HealthCareReports
                .Where(a => a.SurgeryShiftId == surgeryShiftId && a.IsDeleted == false)
                .OrderByDescending(a => a.DateCreated)
                .ToList();
            var results = new List<HealthCareReportViewModel>();
            foreach (var healthCareRerport in healthCareReports)
            {
                results.Add(new HealthCareReportViewModel()
                {
                    Id = healthCareRerport.Id,
                    DateCreated = healthCareRerport.DateCreated.Value.ToString("dd-MM-yyyy HH:mm:ss"),
                    VisitReason = healthCareRerport.CareReason,
                    EventContent = healthCareRerport.EventContent,
                    CareContent = healthCareRerport.CareContent,
                    WoundConditionDescription = healthCareRerport.WoundConditionDescription,
                    WoundCondition = healthCareRerport.WoundCondition,
                    SurgeryShiftId = healthCareRerport.SurgeryShiftId
                });
            }
            return results;
        }

        public ICollection<PostOpSurgeryShiftViewModel> GetSurgeryByStatusId(int statusId)
        {
            var surgeryShifts = _appDbContext.SurgeryShifts
                .Where(a => a.StatusId == statusId && a.IsDeleted == false)
                .ToList();
            var results = new List<PostOpSurgeryShiftViewModel>();
            foreach (var shift in surgeryShifts)
            {
                results.Add(new PostOpSurgeryShiftViewModel()
                {
                    Id = shift.Id,
                    CatalogName = shift.SurgeryCatalog.Name,
                    PatientName = shift.Patient.FullName,
                    PostOpBed = shift.PostBedName,
                    PatientAge = DateTime.Now.Year - shift.Patient.YearOfBirth,
                    PatientGender = shift.Patient.Gender
                });
            }
            return results;
        }

        public bool ChangeSurgeryShiftToRecovery(int surgeryShiftId, string postOpRoom, string postOpBed)
        {
            var surgeryShift = _appDbContext.SurgeryShifts.Find(surgeryShiftId);
            surgeryShift.StatusId = RECOVERY_STATE;
            surgeryShift.PostRoomName = postOpRoom;
            surgeryShift.PostBedName = postOpBed;

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
            var surgeryCatalog = surgeryShift.SurgeryCatalog;

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
                    DateCreated = healthCareRerport.DateCreated.Value.ToString("dd-MM-yyyy HH:mm:ss"),
                    VisitReason = healthCareRerport.CareReason,
                    EventContent = healthCareRerport.EventContent,
                    CareContent = healthCareRerport.CareContent,
                    WoundConditionDescription = healthCareRerport.WoundConditionDescription,
                    WoundCondition = healthCareRerport.WoundCondition,
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
                PostOpBed = surgeryShift.PostBedName,
                PostOpRooom = surgeryShift.PostRoomName,
                CareReports = healthCareReportsViewModelList
            };
            return recoverySurgeryShiftViewModel;
        }

        public bool CreateHealthCareReport(HealthCareReportViewModel healthCareReportViewModel)
        {
            var healthCareReport = new HealthCareReport()
            {
               CareReason = healthCareReportViewModel.VisitReason,
                EventContent = healthCareReportViewModel.EventContent,
                CareContent = healthCareReportViewModel.CareContent,
                WoundCondition = healthCareReportViewModel.WoundCondition,
                WoundConditionDescription = healthCareReportViewModel.WoundConditionDescription,
                IsDeleted = false,
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
            healthCareReport.CareReason = healthCareReportViewModel.VisitReason;
            healthCareReport.EventContent = healthCareReportViewModel.EventContent;
            healthCareReport.CareContent = healthCareReportViewModel.CareContent;
            healthCareReport.WoundCondition = healthCareReportViewModel.WoundCondition;
            healthCareReport.WoundConditionDescription = healthCareReportViewModel.WoundConditionDescription;
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

        public ICollection<PostOpSurgeryShiftViewModel> FindPostOpSurgeryByPatientName(string name)
        {
            var surgeryShifts = _appDbContext.SurgeryShifts
                .Where(a => (a.StatusId == 3 || a.StatusId == 4) && a.IsDeleted == false && a.Patient.FullName.Contains(name))
                .ToList();
            var results = new List<PostOpSurgeryShiftViewModel>();
            foreach (var shift in surgeryShifts)
            {
                results.Add(new PostOpSurgeryShiftViewModel()
                {
                    Id = shift.Id,
                    CatalogName = shift.SurgeryCatalog.Name,
                    PatientName = shift.Patient.FullName,
                    PostOpBed = shift.PostBedName,
                    PatientAge = DateTime.Now.Year - shift.Patient.YearOfBirth,
                    PatientGender = shift.Patient.Gender
                });
            }
            return results;
        }

        public bool EditRoomBedSurgeryShift(int surgeryShiftId, string room, string bed)
        {
            var surgeryShift = _appDbContext.SurgeryShifts.Find(surgeryShiftId);
            if (!string.IsNullOrEmpty(room))
            {
                surgeryShift.PostRoomName = room;
            }
            if (!string.IsNullOrEmpty(bed))
            {
                surgeryShift.PostBedName = bed;
            }
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

        public bool CreateTreatmenReport(TreatmentReportViewModel treatmentReportViewModel)
        {
            var transaction = _appDbContext.Database.BeginTransaction();
            try
            {
                if (treatmentReportViewModel.treatmentReportDrugs.Count > 0)
                {
                    var treatmentReportDrugs = new List<TreatmentReportDrug>();
                    foreach (var tmp in treatmentReportViewModel.treatmentReportDrugs)
                    {
                        var rs = new TreatmentReportDrug()
                        {
                            DrugId = tmp.DrugId,
                            MorningQuantity = tmp.MorningQuantity,
                            AfternoonQuantity = tmp.AfternoonQuantity,
                            EveningQuantity = tmp.EveningQuantity,
                            NightQuantity = tmp.NightQuantity
                        };
                        treatmentReportDrugs.Add(rs);
                    }
                    //create treatment
                    var treatmentReport = new TreatmentReport()
                    {
                        IsDeleted = false,
                        SurgeryShiftId = treatmentReportViewModel.ShiftId,
                        ProgressiveDisease = treatmentReportViewModel.ProgressiveDisease,
                        TreatmentReportDrugs = treatmentReportDrugs
                    };
                    _appDbContext.Add(treatmentReport);
                    _appDbContext.SaveChanges();
                    transaction.Commit();
                    return true;
                }
            }
            catch (Exception)
            {
                transaction.Rollback();
                return false;
            }
            return false;
        }

        public ICollection<TreatmentReportViewModel> GetTreatmentReportByShiftId(int shiftId)
        {
            var treatmentReports = _appDbContext.TreatmentReports
                .Where(a => a.SurgeryShiftId == shiftId && a.IsDeleted == false)
                .OrderByDescending(a => a.DateCreated)
                .ToList();
            var result = new List<TreatmentReportViewModel>();
            foreach (var treatmentReport in treatmentReports)
            {
                var treatmentReportDrugs = new List<TreatmentReportDrugViewModel>();
                foreach (var treatmentReportDrug in treatmentReport.TreatmentReportDrugs)
                {
                    treatmentReportDrugs.Add(new TreatmentReportDrugViewModel()
                    {
                        Name = treatmentReportDrug.Drug.DrugName,
                        MorningQuantity = treatmentReportDrug.MorningQuantity,
                        AfternoonQuantity = treatmentReportDrug.AfternoonQuantity,
                        EveningQuantity = treatmentReportDrug.EveningQuantity,
                        NightQuantity = treatmentReportDrug.NightQuantity,
                    });
                }
                result.Add(new TreatmentReportViewModel()
                {
                    Id = treatmentReport.Id,
                    DateCreated = treatmentReport.DateCreated.Value.ToString("dd-MM-yyyy HH:mm:ss"),
                    ProgressiveDisease = treatmentReport.ProgressiveDisease,
                    ShiftId = treatmentReport.SurgeryShiftId,
                    treatmentReportDrugs = treatmentReportDrugs
                });
            }
            return result;
        }
        
        public ICollection<TreatmentReportViewModel> GetTodayTreatmentReportByShiftId(int shiftId)
        {
            int today = UtilitiesDate.ConvertDateToNumber(DateTime.Now);
            var treatmentReports = _appDbContext.TreatmentReports
                .Where(a => a.SurgeryShiftId == shiftId 
                        && a.IsDeleted == false
                        && UtilitiesDate.ConvertDateToNumber(a.DateCreated.Value) == today)
                .OrderByDescending(a => a.DateCreated)
                .ToList();
            var result = new List<TreatmentReportViewModel>();
            foreach (var treatmentReport in treatmentReports)
            {
                result.Add(new TreatmentReportViewModel()
                {
                    Id = treatmentReport.Id,
                    DateCreated = treatmentReport.DateCreated.Value.ToString("dd-MM-yyyy HH:mm:ss"),
                    ProgressiveDisease = treatmentReport.ProgressiveDisease,
                    ShiftId = treatmentReport.SurgeryShiftId
                });
            }
            return result;
        }

        public bool CreateTreatmentReportDrugs(ICollection<TreatmentReportDrugViewModel> treatmentReportDrugViewModels)
        {
            int id = treatmentReportDrugViewModels.First().TreatmentReportId;
            try {
                foreach (var tmp in treatmentReportDrugViewModels)
                {
                    var rs = new TreatmentReportDrug()
                    {
                        DrugId = tmp.DrugId,
                        TreatmentReportId = tmp.TreatmentReportId,
                        MorningQuantity = tmp.MorningQuantity,
                        AfternoonQuantity = tmp.AfternoonQuantity,
                        EveningQuantity = tmp.EveningQuantity,
                        NightQuantity = tmp.NightQuantity
                    };
                    _appDbContext.TreatmentReportDrugs.Add(rs);
                }
                _appDbContext.SaveChanges();
                return true;
            } catch (Exception)
            {
                var treatmentReport = _appDbContext.TreatmentReports.Find(id);
                _appDbContext.TreatmentReports.Remove(treatmentReport);
                _appDbContext.SaveChanges();
                return false;
            }
        }

        public ICollection<TreatmentReportDrugViewModel> GetDrugRequirementForNurse(int time, int shiftId)
        {
            var result = new List<TreatmentReportDrugViewModel>();
            int today = UtilitiesDate.ConvertDateToNumber(DateTime.Now);
            switch (time)
            {   
                case 1:
                    var rs1 = _appDbContext.TreatmentReportDrugs.Where(a => !a.IsDeleted
                            && a.MorningQuantity > 0
                            && a.TreatmentReport.SurgeryShiftId == shiftId
                            && UtilitiesDate.ConvertDateToNumber(a.TreatmentReport.DateCreated.Value) == today)
                            .ToList();
                    foreach (var item in rs1)
                    {
                        result.Add(new TreatmentReportDrugViewModel()
                        {
                            Name = item.Drug.DrugName,
                            MorningQuantity = item.MorningQuantity
                        });
                    }
                    break;
                case 2:
                    var rs2 = _appDbContext.TreatmentReportDrugs.Where(a => !a.IsDeleted
                            && a.AfternoonQuantity > 0
                            && a.TreatmentReport.SurgeryShiftId == shiftId
                            && UtilitiesDate.ConvertDateToNumber(a.TreatmentReport.DateCreated.Value) == today)
                            .ToList();
                    foreach (var item in rs2)
                    {
                        result.Add(new TreatmentReportDrugViewModel()
                        {
                            Name = item.Drug.DrugName,
                            AfternoonQuantity = item.AfternoonQuantity
                        });
                    }
                    break;
                case 3:
                    var rs3 = _appDbContext.TreatmentReportDrugs.Where(a => !a.IsDeleted
                            && a.EveningQuantity > 0
                            && a.TreatmentReport.SurgeryShiftId == shiftId
                            && UtilitiesDate.ConvertDateToNumber(a.TreatmentReport.DateCreated.Value) == today)
                            .ToList();
                    foreach (var item in rs3)
                    {
                        result.Add(new TreatmentReportDrugViewModel()
                        {
                            Name = item.Drug.DrugName,
                            EveningQuantity = item.EveningQuantity
                        });
                    }
                    break;
                case 4:
                    var rs4 = _appDbContext.TreatmentReportDrugs.Where(a => !a.IsDeleted
                            && a.NightQuantity > 0
                            && a.TreatmentReport.SurgeryShiftId == shiftId
                            && UtilitiesDate.ConvertDateToNumber(a.TreatmentReport.DateCreated.Value) == today)
                            .ToList();
                    foreach (var item in rs4)
                    {
                        result.Add(new TreatmentReportDrugViewModel()
                        {
                            Name = item.Drug.DrugName,
                            NightQuantity = item.NightQuantity
                        });
                    }
                    break;
            }
            return result;
        }
    }
}
