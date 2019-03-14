using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
using System.Security.Claims;
using System.Text;
using Surgery_1.Services.Extensions;
using System.Threading.Tasks;

namespace Surgery_1.Services.Implementations
{
    public class PostOpService : IPostOpService
    {
        private readonly int RECOVERY_STATE = 4;
        private readonly AppDbContext _appDbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IdentityUser> _userManager;

        public PostOpService(AppDbContext appDbContext, IHttpContextAccessor httpContextAccessor, UserManager<IdentityUser> userManager)
        {
            _appDbContext = appDbContext;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
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
                    SurgeryShiftId = healthCareRerport.SurgeryShiftId,
                    NurseName = _appDbContext.UserInfo.Find(healthCareRerport.NurseId).FullName,
                });
            }
            return results;
        }

        public ICollection<PostOpSurgeryShiftViewModel> GetSurgeryByStatusId(int statusId)
        {
            var guid = _httpContextAccessor.HttpContext.User.GetGuid();
            var nurse = _appDbContext.UserInfo.Where(a => a.GuId == guid).FirstOrDefault();
            var surgeryShifts = _appDbContext.SurgeryShifts
                .Where(a => a.StatusId == statusId && a.IsDeleted == false && a.NurseId == nurse.Id)
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
            var guid = _httpContextAccessor.HttpContext.User.GetGuid();
            var nurseId = _appDbContext.UserInfo.Where(a => a.GuId == guid).FirstOrDefault().Id;
            var tr = _appDbContext.TreatmentReports.OrderByDescending(a => a.DateCreated).FirstOrDefault();
            tr.IsUsed = true;
            var healthCareReport = new HealthCareReport()
            {
                CareReason = healthCareReportViewModel.VisitReason,
                EventContent = healthCareReportViewModel.EventContent,
                CareContent = healthCareReportViewModel.CareContent,
                WoundCondition = healthCareReportViewModel.WoundCondition,
                WoundConditionDescription = healthCareReportViewModel.WoundConditionDescription,
                IsDeleted = false,
                SurgeryShiftId = healthCareReportViewModel.SurgeryShiftId,
                TreatmentReportId = tr.Id,
                NurseId = nurseId,
            };
            try
            {
                _appDbContext.TreatmentReports.Update(tr);
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
                return false;
            }

        }

        public ICollection<PostOpSurgeryShiftViewModel> FindPostOpSurgeryByQuery(string query)
        {
            var guid = _httpContextAccessor.HttpContext.User.GetGuid();
            var nurse = _appDbContext.UserInfo.Where(a => a.GuId == guid).FirstOrDefault();
            try
            {
                int id;
                bool success = Int32.TryParse(query, out id);
                if (success)
                {
                    var surgeryShifts = _appDbContext.SurgeryShifts
                     .Where(a => (a.StatusId == 5 || a.StatusId == 6) && a.IsDeleted == false
                     && (a.Patient.FullName.Contains(query) || a.Id == id || a.TreatmentDoctor.FullName.Contains(query))
                     && a.NurseId == nurse.Id)
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
                } else
                {
                    var surgeryShifts = _appDbContext.SurgeryShifts
                     .Where(a => (a.StatusId == 3 || a.StatusId == 4) && a.IsDeleted == false
                     && (a.Patient.FullName.Contains(query) || a.TreatmentDoctor.FullName.Contains(query)))
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
            }
            catch (Exception)
            {
                return null;
            }
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
                if (treatmentReportViewModel.TreatmentReportDrugs.Count > 0)
                {
                    var treatmentReportDrugs = new List<TreatmentReportDrug>();
                    foreach (var tmp in treatmentReportViewModel.TreatmentReportDrugs)
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
                .Where(t => t.SurgeryShiftId == shiftId && !t.IsDeleted)
                .OrderByDescending(t => t.DateCreated)
                .ToList();
            var result = new List<TreatmentReportViewModel>();
            foreach (var treatmentReport in treatmentReports)
            {
                var treatmentReportDrugs = new List<TreatmentReportDrugViewModel>();
                foreach (var treatmentReportDrug in treatmentReport.TreatmentReportDrugs.Where(a => !a.IsDeleted).ToList())
                {
                    treatmentReportDrugs.Add(new TreatmentReportDrugViewModel()
                    {   
                        Id = treatmentReportDrug.Id,
                        Name = treatmentReportDrug.Drug.DrugName,
                        MorningQuantity = treatmentReportDrug.MorningQuantity,
                        AfternoonQuantity = treatmentReportDrug.AfternoonQuantity,
                        EveningQuantity = treatmentReportDrug.EveningQuantity,
                        NightQuantity = treatmentReportDrug.NightQuantity,
                        Unit = treatmentReportDrug.Drug.Unit,
                    });
                }
                result.Add(new TreatmentReportViewModel()
                {
                    Id = treatmentReport.Id,
                    DateCreated = treatmentReport.DateCreated.Value.ToString("dd-MM-yyyy HH:mm:ss"),
                    ProgressiveDisease = treatmentReport.ProgressiveDisease,
                    ShiftId = treatmentReport.SurgeryShiftId,
                    TreatmentReportDrugs = treatmentReportDrugs,
                    IsUsed = treatmentReport.IsUsed,
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

        public TreatmentMedication GetDrugRequirementForNurse(int shiftId)
        {
            var drugs = new List<TreatmentReportDrugViewModel>();
            int today = UtilitiesDate.ConvertDateToNumber(DateTime.Now);
            int now = DateTime.Now.Hour;
            int time;
            if (now >= 5 && now < 11)
            {
                time = 1;
            }
            else if (now >= 11 && now < 15)
            {
                time = 2;
            }
            else if (now >= 15 && now < 20)
            {
                time = 3;
            }
            else
            {
                time = 4;
            }

            var tr = _appDbContext.TreatmentReports.Max(a => a.DateCreated);
            switch (time)
            {   
                case 1:
                   
                    var rs1 = _appDbContext.TreatmentReportDrugs.Where(a => !a.IsDeleted
                            && a.MorningQuantity > 0
                            && a.TreatmentReport.SurgeryShiftId == shiftId
                            && a.TreatmentReport.DateCreated == tr)
                            .ToList();
                    foreach (var item in rs1)
                    {
                        drugs.Add(new TreatmentReportDrugViewModel()
                        {
                            Name = item.Drug.DrugName,
                            MorningQuantity = item.MorningQuantity,
                            Unit = item.Drug.Unit
                        });
                    }
                    break;
                case 2:
                    var rs2 = _appDbContext.TreatmentReportDrugs.Where(a => !a.IsDeleted
                            && a.AfternoonQuantity > 0
                            && a.TreatmentReport.SurgeryShiftId == shiftId
                            && a.TreatmentReport.DateCreated == tr)
                            .ToList();
                    foreach (var item in rs2)
                    {
                        drugs.Add(new TreatmentReportDrugViewModel()
                        {
                            Name = item.Drug.DrugName,
                            AfternoonQuantity = item.AfternoonQuantity,
                            Unit = item.Drug.Unit
                        });
                    }
                    break;
                case 3:
                    var rs3 = _appDbContext.TreatmentReportDrugs.Where(a => !a.IsDeleted
                            && a.EveningQuantity > 0
                            && a.TreatmentReport.SurgeryShiftId == shiftId
                            && a.TreatmentReport.DateCreated == tr)
                            .ToList();
                    foreach (var item in rs3)
                    {
                        drugs.Add(new TreatmentReportDrugViewModel()
                        {
                            Name = item.Drug.DrugName,
                            EveningQuantity = item.EveningQuantity,
                            Unit = item.Drug.Unit
                        });
                    }
                    break;
                case 4:
                    var rs4 = _appDbContext.TreatmentReportDrugs.Where(a => !a.IsDeleted
                            && a.NightQuantity > 0
                            && a.TreatmentReport.SurgeryShiftId == shiftId
                            && a.TreatmentReport.DateCreated == tr)
                            .ToList();
                    foreach (var item in rs4)
                    {
                        drugs.Add(new TreatmentReportDrugViewModel()
                        {
                            Name = item.Drug.DrugName,
                            NightQuantity = item.NightQuantity,
                            Unit = item.Drug.Unit
                        });
                    }
                    break;
            }
            var result = new TreatmentMedication()
            {
                drugs = drugs,
                time = time
            };
            return result;
        }

        public TreatmentReportViewModel GetTreatmentReportById(int id)
        {
            var treatmentReport = _appDbContext.TreatmentReports.Find(id);
            var treatmentReportDrugs = new List<TreatmentReportDrugViewModel>();
            foreach (var treatmentReportDrug in treatmentReport.TreatmentReportDrugs.Where(a => a.IsDeleted == false).ToList())
            {
                treatmentReportDrugs.Add(new TreatmentReportDrugViewModel()
                {   
                    Id = treatmentReportDrug.Id,
                    Name = treatmentReportDrug.Drug.DrugName,
                    MorningQuantity = treatmentReportDrug.MorningQuantity,
                    AfternoonQuantity = treatmentReportDrug.AfternoonQuantity,
                    EveningQuantity = treatmentReportDrug.EveningQuantity,
                    NightQuantity = treatmentReportDrug.NightQuantity,
                    Unit = treatmentReportDrug.Drug.Unit,
                });
            }
            var result = new TreatmentReportViewModel()
            {
                Id = treatmentReport.Id,
                ProgressiveDisease = treatmentReport.ProgressiveDisease,
                TreatmentReportDrugs = treatmentReportDrugs
            };
            return result;
        }

        public bool EditTreatmentReport(TreatmentReportViewModel treatmentReportViewModel)
        {
            var treatmentReport = _appDbContext.TreatmentReports.Find(treatmentReportViewModel.Id);
            if (!treatmentReport.IsUsed)
            {
                treatmentReport.ProgressiveDisease = treatmentReportViewModel.ProgressiveDisease;
                var treatmentReportDrugs = new List<TreatmentReportDrug>();
                foreach (var treatmentReportDrugViewModel in treatmentReportViewModel.TreatmentReportDrugs)
                {

                    TreatmentReportDrug treatmentReportDrug = _appDbContext.TreatmentReportDrugs.Find(treatmentReportDrugViewModel.Id);
                    if (treatmentReportDrug != null)
                    {
                        treatmentReportDrug.DrugId = treatmentReportDrugViewModel.DrugId;
                        treatmentReportDrug.MorningQuantity = treatmentReportDrugViewModel.MorningQuantity;
                        treatmentReportDrug.AfternoonQuantity = treatmentReportDrugViewModel.AfternoonQuantity;
                        treatmentReportDrug.EveningQuantity = treatmentReportDrugViewModel.EveningQuantity;
                        treatmentReportDrug.NightQuantity = treatmentReportDrugViewModel.NightQuantity;
                        treatmentReportDrugs.Add(treatmentReportDrug);
                        _appDbContext.TreatmentReportDrugs.Update(treatmentReportDrug);
                    }
                    else
                    {
                        treatmentReportDrug = new TreatmentReportDrug()
                        {
                            DrugId = treatmentReportDrugViewModel.DrugId,
                            MorningQuantity = treatmentReportDrugViewModel.MorningQuantity,
                            AfternoonQuantity = treatmentReportDrugViewModel.AfternoonQuantity,
                            EveningQuantity = treatmentReportDrugViewModel.EveningQuantity,
                            NightQuantity = treatmentReportDrugViewModel.NightQuantity,
                            TreatmentReportId = treatmentReportDrugViewModel.TreatmentReportId,
                        };
                        treatmentReportDrugs.Add(treatmentReportDrug);
                    }
                    _appDbContext.SaveChanges();
                }
                treatmentReport.TreatmentReportDrugs = treatmentReportDrugs;

                foreach (var item in treatmentReportViewModel.DeleteTreatmentReportId)
                {
                    TreatmentReportDrug treatmentReportDrug = _appDbContext.TreatmentReportDrugs.Find(item);
                    treatmentReportDrug.IsDeleted = true;
                    _appDbContext.TreatmentReportDrugs.Update(treatmentReportDrug);
                    _appDbContext.SaveChanges();
                }
                try
                {
                    _appDbContext.Update(treatmentReport);
                    _appDbContext.SaveChanges();
                    return true;
                }
                catch (DbUpdateException)
                {
                    return false;
                }
            } else
            {
                return false;
            }
        }

        public bool AssignNurse(int shiftId, int nurseId)
        {
            var shift = _appDbContext.SurgeryShifts.Find(shiftId);
            shift.NurseId = nurseId;
            try
            {
                _appDbContext.SurgeryShifts.Update(shift);
                _appDbContext.SaveChanges();
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }

        public async Task<ICollection<NurseViewModel>> GetAllNurse()
        {
            var nurses = _appDbContext.UserInfo.Where(a => a.IsDeleted == false).ToList();
            var result = new List<NurseViewModel>();
            foreach (var item in nurses)
            {
                var capacity = _appDbContext.SurgeryShifts.Where(a => !a.IsDeleted && a.NurseId == item.Id && !a.Status.Name.Equals("Finished")).Count();

                var user = _appDbContext.Users.Find(item.GuId);
                var roleList = await _userManager.GetRolesAsync(user);
                var userRole = roleList.FirstOrDefault();
                if (userRole.Equals("Nurse"))
                {
                    var nurse = new NurseViewModel()
                    {
                        FullName = item.FullName,
                        Id = item.Id,
                        Capacity = capacity,
                    };
                    result.Add(nurse);
                }
            }
            return result;
        }

        public NurseViewModel GetNurseByShiftId(int shiftId)
        {
            var nurseId = _appDbContext.SurgeryShifts.Find(shiftId).NurseId;
            if (nurseId != null)
            {
                var nurse = _appDbContext.UserInfo.Find(nurseId);
                var rs = new NurseViewModel()
                {
                    Id = nurse.Id,
                    FullName = nurse.FullName,
                };
                return rs;
            }
            return null;
        }
    }
}
