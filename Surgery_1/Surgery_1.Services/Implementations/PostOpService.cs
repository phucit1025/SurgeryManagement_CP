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
using Firebase.Database;
using Newtonsoft.Json;
using System.Net.Http;
using Polly;
using System.Net;
using DinkToPdf.Contracts;
using DinkToPdf;
using System.IO;
using System.Collections;
using MoreLinq;
using Castle.Core.Internal;

namespace Surgery_1.Services.Implementations
{
    public class PostOpService : IPostOpService
    {
        private readonly string FIREBASE_DB_URL = "https://ebsms-dcebe.firebaseio.com/";
        private readonly string SERVER_KEY = "AAAAUsOlx2E:APA91bEU7RZdpi29XOBScui7ceYb4kk0tNqvSG6rLgRMs5fd0pirZQ0qIVlJm5rj2TmBQKUlNpvWEUug-v4RHbS3xFOYIkzfckdIApYiCuIe5ZYlXTnDo6MdFnEWpkyHX5qAvVAxNw7C";
        private readonly string SENDER_ID = "355469739873";
        private readonly AppDbContext _appDbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConverter _converter;

        public PostOpService(AppDbContext appDbContext, IHttpContextAccessor httpContextAccessor
            , UserManager<IdentityUser> userManager, IConverter converter)
        {
            _appDbContext = appDbContext;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _converter = converter;
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
                    ClosestDate = healthCareRerport.DateCreated.Value,
                    VisitReason = healthCareRerport.CareReason,
                    EventContent = healthCareRerport.EventContent,
                    CareContent = healthCareRerport.CareContent,
                    WoundConditionDescription = healthCareRerport.WoundConditionDescription,
                    WoundCondition = healthCareRerport.WoundCondition,
                    DrugAllergy = healthCareRerport.DrugAllergy,
                    DrugAllergyDescription = healthCareRerport.DrugAllergyDescription,
                    SurgeryShiftId = healthCareRerport.SurgeryShiftId,
                    NurseName = _appDbContext.UserInfo.Find(healthCareRerport.NurseId).FullName,
                    SurgeryName = healthCareRerport.SurgeryShift.SurgeryCatalog.Name,
                    PatientName = healthCareRerport.SurgeryShift.Patient.FullName,
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
                .OrderByDescending(a => a.ActualEndDateTime)
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
            //surgeryShift.StatusId = RECOVERY_STATE;
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
            if (surgeryShift != null)
            {
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
                        DrugAllergy = healthCareRerport.DrugAllergy,
                        DrugAllergyDescription = healthCareRerport.DrugAllergyDescription,
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
            return null;
        }

        public bool CreateHealthCareReport(HealthCareReportViewModel healthCareReportViewModel)
        {
            try
            {
                var guid = _httpContextAccessor.HttpContext.User.GetGuid();
                var nurseId = _appDbContext.UserInfo.Where(a => a.GuId == guid).FirstOrDefault().Id;
                var healthCareReport = new HealthCareReport()
                {
                    CareReason = healthCareReportViewModel.VisitReason,
                    EventContent = healthCareReportViewModel.EventContent,
                    CareContent = healthCareReportViewModel.CareContent,
                    WoundCondition = healthCareReportViewModel.WoundCondition,
                    WoundConditionDescription = healthCareReportViewModel.WoundConditionDescription,
                    DrugAllergy = healthCareReportViewModel.DrugAllergy,
                    DrugAllergyDescription = healthCareReportViewModel.DrugAllergyDescription,
                    IsDeleted = false,
                    SurgeryShiftId = healthCareReportViewModel.SurgeryShiftId,
                    NurseId = nurseId,
                };
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
            var guid = _httpContextAccessor.HttpContext.User.GetGuid();
            var nurseId = _appDbContext.UserInfo.Where(a => a.GuId == guid).FirstOrDefault().Id;

            var healthCareReport = _appDbContext.HealthCareReports.Find(healthCareReportViewModel.Id);

            if (nurseId.Equals(healthCareReport.NurseId))
            {
                healthCareReport.CareReason = healthCareReportViewModel.VisitReason;
                healthCareReport.EventContent = healthCareReportViewModel.EventContent;
                healthCareReport.CareContent = healthCareReportViewModel.CareContent;
                healthCareReport.WoundCondition = healthCareReportViewModel.WoundCondition;
                healthCareReport.WoundConditionDescription = healthCareReportViewModel.WoundConditionDescription;
                healthCareReport.DrugAllergy = healthCareReportViewModel.DrugAllergy;
                healthCareReport.DrugAllergyDescription = healthCareReportViewModel.DrugAllergyDescription;
                DateTime date = new DateTime();
                healthCareReport.DateUpdated = date;
                try
                {
                    _appDbContext.Update(healthCareReport);
                    _appDbContext.SaveChanges();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }

        public ICollection<PostOpSurgeryShiftViewModel> FindPostOpSurgeryByQuery(string query)
        {
            int postStatus = _appDbContext.Statuses.Where(s => s.Name == "Postoperative").FirstOrDefault().Id;
            int recoveryStatus = _appDbContext.Statuses.Where(s => s.Name == "Recovery").FirstOrDefault().Id;
            var guid = _httpContextAccessor.HttpContext.User.GetGuid();
            var nurse = _appDbContext.UserInfo.Where(a => a.GuId == guid).FirstOrDefault();
            try
            {
                int id;
                bool success = Int32.TryParse(query, out id);
                if (success)
                {
                    var surgeryShifts = _appDbContext.SurgeryShifts
                     .Where(a => (a.StatusId == postStatus || a.StatusId == recoveryStatus) && a.IsDeleted == false
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
                }
                else
                {
                    var surgeryShifts = _appDbContext.SurgeryShifts
                     .Where(a => (a.StatusId == postStatus || a.StatusId == recoveryStatus) && a.IsDeleted == false
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
                            TimeString = string.Join(",", tmp.TimeString),
                            StatusString = ConvertTimeString2StatusString(tmp.TimeString),
                            Route = tmp.Route,
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


        public string ConvertTimeString2StatusString(string[] arrTimeString)
        {
            var arrStatusString = new List<string>();
            for (int i = 0; i < arrTimeString.Length; i++)
            {
                var timeString = arrTimeString.ElementAt(i);
                var time = timeString.Split('/').ElementAt(0);
                var statusString = time + "/0";
                arrStatusString.Add(statusString);
            }
            return string.Join(",", arrStatusString);
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
                foreach (var treatmentReportDrug in treatmentReport.TreatmentReportDrugs.ToList())
                {
                    treatmentReportDrugs.Add(new TreatmentReportDrugViewModel()
                    {
                        Id = treatmentReportDrug.Id,
                        DrugId = treatmentReportDrug.DrugId,
                        Name = treatmentReportDrug.Drug.DrugName,
                        TimeString = treatmentReportDrug.TimeString.Split(','),
                        StatusString = treatmentReportDrug.StatusString.Split(','),
                        Route = treatmentReportDrug.Route,
                        Unit = treatmentReportDrug.Drug.Unit,
                        IsUsed = treatmentReportDrug.IsUsed,
                        IsDeleted = treatmentReportDrug.IsDeleted,
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
            try
            {
                foreach (var tmp in treatmentReportDrugViewModels)
                {
                    var rs = new TreatmentReportDrug()
                    {
                        DrugId = tmp.DrugId,
                        TreatmentReportId = tmp.TreatmentReportId,
                    };
                    _appDbContext.TreatmentReportDrugs.Add(rs);
                }
                _appDbContext.SaveChanges();
                return true;
            }
            catch (Exception)
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

            var lastedTreatment = _appDbContext.TreatmentReports.Where(a => a.SurgeryShiftId == shiftId).LastOrDefault();
            if (lastedTreatment != null)
            {
                var id = lastedTreatment.Id;
                //switch (time)
                //{
                //    case 1:

                //        var rs1 = _appDbContext.TreatmentReportDrugs.Where(a => !a.IsDeleted
                //                && a.MorningQuantity > 0
                //                && a.TreatmentReport.Id == id)
                //                .ToList();
                //        foreach (var item in rs1)
                //        {
                //            drugs.Add(new TreatmentReportDrugViewModel()
                //            {
                //                Name = item.Drug.DrugName,
                //                MorningQuantity = item.MorningQuantity,
                //                Unit = item.Drug.Unit
                //            });
                //        }
                //        break;
                //    case 2:
                //        var rs2 = _appDbContext.TreatmentReportDrugs.Where(a => !a.IsDeleted
                //                && a.AfternoonQuantity > 0
                //                && a.TreatmentReport.Id == id)
                //                .ToList();
                //        foreach (var item in rs2)
                //        {
                //            drugs.Add(new TreatmentReportDrugViewModel()
                //            {
                //                Name = item.Drug.DrugName,
                //                AfternoonQuantity = item.AfternoonQuantity,
                //                Unit = item.Drug.Unit
                //            });
                //        }
                //        break;
                //    case 3:
                //        var rs3 = _appDbContext.TreatmentReportDrugs.Where(a => !a.IsDeleted
                //                && a.EveningQuantity > 0
                //                && a.TreatmentReport.Id == id)
                //                .ToList();
                //        foreach (var item in rs3)
                //        {
                //            drugs.Add(new TreatmentReportDrugViewModel()
                //            {
                //                Name = item.Drug.DrugName,
                //                EveningQuantity = item.EveningQuantity,
                //                Unit = item.Drug.Unit
                //            });
                //        }
                //        break;
                //    case 4:
                //        var rs4 = _appDbContext.TreatmentReportDrugs.Where(a => !a.IsDeleted
                //                && a.NightQuantity > 0
                //                && a.TreatmentReport.Id == id)
                //                .ToList();
                //        foreach (var item in rs4)
                //        {
                //            drugs.Add(new TreatmentReportDrugViewModel()
                //            {
                //                Name = item.Drug.DrugName,
                //                NightQuantity = item.NightQuantity,
                //                Unit = item.Drug.Unit
                //            });
                //        }
                //        break;
                //}
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
            foreach (var treatmentReportDrug in treatmentReport.TreatmentReportDrugs.ToList())
            {
                treatmentReportDrugs.Add(new TreatmentReportDrugViewModel()
                {
                    Id = treatmentReportDrug.Id,
                    Name = treatmentReportDrug.Drug.DrugName,
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
            treatmentReport.ProgressiveDisease = treatmentReportViewModel.ProgressiveDisease;
            var treatmentReportDrugs = new List<TreatmentReportDrug>();
            if (treatmentReportViewModel.TreatmentReportDrugs != null)
            {
                foreach (var treatmentReportDrugViewModel in treatmentReportViewModel.TreatmentReportDrugs)
                {

                    TreatmentReportDrug treatmentReportDrug = _appDbContext.TreatmentReportDrugs.Find(treatmentReportDrugViewModel.Id);
                    if (treatmentReportDrug != null)
                    {
                        treatmentReportDrug.DrugId = treatmentReportDrugViewModel.DrugId;
                        treatmentReportDrug.TimeString = string.Join(",", treatmentReportDrugViewModel.TimeString);
                        treatmentReportDrug.StatusString = string.Join(",", treatmentReportDrugViewModel.TimeString);
                        treatmentReportDrug.Route = treatmentReportDrugViewModel.Route;
                        treatmentReportDrugs.Add(treatmentReportDrug);
                        _appDbContext.TreatmentReportDrugs.Update(treatmentReportDrug);
                    }
                    else
                    {
                        treatmentReportDrug = new TreatmentReportDrug()
                        {
                            DrugId = treatmentReportDrugViewModel.DrugId,
                            TimeString = string.Join(",", treatmentReportDrugViewModel.TimeString),
                            StatusString = string.Join(",", treatmentReportDrugViewModel.TimeString),
                            Route = treatmentReportDrugViewModel.Route,
                            TreatmentReportId = treatmentReportDrugViewModel.TreatmentReportId,
                        };
                        treatmentReportDrugs.Add(treatmentReportDrug);
                    }
                }
            }
            treatmentReport.TreatmentReportDrugs = treatmentReportDrugs;
            if (treatmentReportViewModel.DeleteTreatmentReportId != null)
            {
                foreach (var item in treatmentReportViewModel.DeleteTreatmentReportId)
                {
                    TreatmentReportDrug treatmentReportDrug = _appDbContext.TreatmentReportDrugs.Find(item);
                    treatmentReportDrug.IsDeleted = true;
                    _appDbContext.TreatmentReportDrugs.Update(treatmentReportDrug);
                }
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
        }

        public bool checkNurseCapacity(int nurseId)
        {
            var capacity = _appDbContext.SurgeryShifts.Where(a => !a.IsDeleted && a.NurseId == nurseId && !a.Status.Name.Equals(ConstantVariable.FINISHED_STATUS)).Count();
            if (capacity >= 7)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> AssignNurse(int shiftId, int nurseId)
        {
            if (checkNurseCapacity(nurseId))
            {
                var shift = _appDbContext.SurgeryShifts.Find(shiftId);
                shift.NurseId = nurseId;
                var guid = _appDbContext.UserInfo.Find(nurseId).GuId;
                var user = await _userManager.FindByIdAsync(guid);
                var username = user.Email;
                var firebase = new FirebaseClient(FIREBASE_DB_URL);
                var deviceTokens = await firebase.Child($"/users/{username}/devices").OnceAsync<Dictionary<string, string>>();
                try
                {
                    _appDbContext.SurgeryShifts.Update(shift);
                    int rs = _appDbContext.SaveChanges();
                    if (rs > 0)
                    {
                        foreach (var item in deviceTokens)
                        {
                            var token = item.Object.FirstOrDefault().Value;
                            await Send(getAndroidMessage(shiftId, new { id = shiftId }, token));
                        }
                    }
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }

        public async Task Send(string notification)
        {
            var http = new HttpClient();
            http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "key=" + SERVER_KEY);
            http.DefaultRequestHeaders.TryAddWithoutValidation("Sender", "id=" + SENDER_ID);
            http.DefaultRequestHeaders.TryAddWithoutValidation("content-length", notification.Length.ToString());
            var content = new StringContent(notification, System.Text.Encoding.UTF8, "application/json");
            HttpStatusCode[] httpStatusCodesWorthRetrying = {
               HttpStatusCode.RequestTimeout, // 408
               HttpStatusCode.InternalServerError, // 500
               HttpStatusCode.BadGateway, // 502
               HttpStatusCode.ServiceUnavailable, // 503
               HttpStatusCode.GatewayTimeout // 504
            };
            var retryPolicy = Policy
               .Handle<HttpRequestException>()
               .OrResult<HttpResponseMessage>(r => httpStatusCodesWorthRetrying.Contains(r.StatusCode))
               .WaitAndRetryAsync(10, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            await retryPolicy.ExecuteAsync(async () =>
            {
                var response = await http.PostAsync("https://fcm.googleapis.com/fcm/send", content);
                return response;
            });


        }

        public string getAndroidMessage(int shiftId, object objData, string regId)
        {
            var payload = new
            {
                to = regId,
                priority = "high",
                content_available = true,
                notification = new
                {
                    click_action = ".activity.FirebaseClickActionActivity",
                    body = "New PostOp Surgery Shift - ID: " + shiftId,
                    title = "eBSMS",
                    badge = 1
                },
                data = objData,
            };

            return JsonConvert.SerializeObject(payload);
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

        public bool SoftDeleteTreatmentReport(int treatmentReportId)
        {
            var treatmentReport = _appDbContext.TreatmentReports.Find(treatmentReportId);
            treatmentReport.IsDeleted = true;
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
        }

        public byte[] CreateSurgeryPdf(string styleSheets, int id, int type)
        {
            var rs = _appDbContext.SurgeryShifts.Find(id);
            var globalSettings = new GlobalSettings();
            var objectSettings = new ObjectSettings();
            switch (type)
            {
                case 1:
                    globalSettings = new GlobalSettings
                    {
                        ColorMode = ColorMode.Color,
                        Orientation = Orientation.Portrait,
                        PaperSize = PaperKind.A4,
                        Margins = new MarginSettings { Top = 10 },
                        DocumentTitle = "Surgical Record",
                    };
                    objectSettings = new ObjectSettings
                    {
                        PagesCount = true,
                        HtmlContent = TemplateGenerator.GetHTMLString(rs, styleSheets),
                        WebSettings = { DefaultEncoding = "utf-8" },
                        HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Page [page] of [toPage]", Left = "eBSMS System", Line = true },
                        FooterSettings = { FontName = "Arial", FontSize = 9, Line = true, Center = "Report Footer" }
                    };
                    break;
                case 2:
                    globalSettings = new GlobalSettings
                    {
                        ColorMode = ColorMode.Color,
                        Orientation = Orientation.Portrait,
                        PaperSize = PaperKind.A4,
                        Margins = new MarginSettings { Top = 10 },
                        DocumentTitle = "Healthcare Report",
                    };
                    objectSettings = new ObjectSettings
                    {
                        PagesCount = true,
                        HtmlContent = TemplateGenerator.GetHTMLStringHealthcare(rs),
                        WebSettings = { DefaultEncoding = "utf-8" },
                        HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Page [page] of [toPage]", Left = "eBSMS System", Line = true },
                        FooterSettings = { FontName = "Arial", FontSize = 9, Line = true, Center = "Report Footer" }
                    };
                    break;

                default:
                    break;
            }

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };

            var file = _converter.Convert(pdf);
            return file;

        }

        public Dictionary<string, List<TreatmentTimelineViewModel>> GetDrugTimelineByShiftIdAndDate(int shiftId)
        {
            int today = UtilitiesDate.ConvertDateToNumber(DateTime.Now);
            var drugs = _appDbContext.TreatmentReportDrugs
                .Where(s =>
                    UtilitiesDate.ConvertDateToNumber(s.DateCreated.Value).Equals(today)
                    && !s.IsDeleted
                    && s.TreatmentReport.SurgeryShiftId == shiftId).ToArray();
            Dictionary<string, List<TreatmentTimelineViewModel>> rs = new Dictionary<string, List<TreatmentTimelineViewModel>>();
            for (int i = 0; i < drugs.Count(); i++)
            {
                var drug = drugs.ElementAt(i);
                var timeQuantity = drug.TimeString.Split(',');
                var status = drug.StatusString.Split(',');
                for (int j = 0; j < timeQuantity.Count(); j++)
                {
                    var t = timeQuantity.ElementAt(j);
                    var time = t.Split('/').ElementAt(0);
                    var quantity = t.Split('/').ElementAt(1);
                    var isUsed = status.ElementAt(j).Split('/').ElementAt(1);
                    if (rs.ContainsKey(time))
                    {
                        var drugListInTimeT = rs[time];
                        drugListInTimeT.Add(new TreatmentTimelineViewModel
                        {
                            TreatmentDrugId = drug.Id,
                            Name = drug.Drug.DrugName,
                            Unit = drug.Drug.Unit,
                            Route = drug.Route,
                            Quantity = int.Parse(quantity),
                            IsUsed = int.Parse(isUsed),
                        });
                    }
                    else
                    {
                        var value = new List<TreatmentTimelineViewModel>();
                        value.Add(new TreatmentTimelineViewModel
                        {
                            TreatmentDrugId = drug.Id,
                            Name = drug.Drug.DrugName,
                            Unit = drug.Drug.Unit,
                            Route = drug.Route,
                            Quantity = int.Parse(quantity),
                            IsUsed = int.Parse(isUsed),
                        });
                        rs.Add(time, value);
                    }
                }

            }
            rs = rs.OrderBy(s => s.Key).ToDictionary();
            return rs;
        }

        public bool ConfirmTakeMedicine(int treatmentReportDrugId, string time)
        {
            var drug = _appDbContext.TreatmentReportDrugs.Find(treatmentReportDrugId);
            drug.StatusString = drug.StatusString.Replace(time + "/0", time + "/1");
            _appDbContext.Update(drug);
            _appDbContext.SaveChanges();
            return true;
        }

        public List<HealthcareSurgeryShiftViewModel> GetHealthcareSurgeryShifts()
        {
            var surgeryShifts = _appDbContext.SurgeryShifts.Where(s => s.StatusId == ConstantVariable.RECOVERY_STATUS_NUM
                                                                    && s.HealthCareReports.Count > 0).ToList();
            var rs = new List<HealthcareSurgeryShiftViewModel>();
            foreach (var surgeryShift in surgeryShifts)
            {
                var shift = new HealthcareSurgeryShiftViewModel();
                shift.ShiftId = surgeryShift.Id;
                shift.SurgeryName = surgeryShift.SurgeryCatalog.Name;
                shift.PatientName = surgeryShift.Patient.FullName;
                var lastHealthcareReport = surgeryShift.HealthCareReports.LastOrDefault();
                shift.WoundCondition = lastHealthcareReport.WoundCondition;
                shift.WoundConditionDescription = lastHealthcareReport.WoundConditionDescription;
                shift.DrugAllergy = lastHealthcareReport.DrugAllergy;
                shift.DrugAllergyDescription = lastHealthcareReport.DrugAllergyDescription;
                shift.ClosestDate = lastHealthcareReport.DateCreated.Value;
                rs.Add(shift);
            }
            return rs;
        }

        #region Statistical
        public List<PostOpViewModel> GetPostOpSurgeryShift(DateTime actualEnd, int speacialtyId, int surgeryId, int doctorId, bool? isPostOp)
        {
            var surgeryShitfs = _appDbContext.SurgeryShifts.Where(s => s.StatusId == ConstantVariable.POST_STATUS_NUM
                                                                  || s.StatusId == ConstantVariable.RECOVERY_STATUS_NUM).ToList();
            var rs = new List<PostOpViewModel>();

            if (actualEnd == DateTime.MinValue && speacialtyId == 0 && surgeryId == 0 && doctorId == 0 && !isPostOp.HasValue)
            {
                var shifts = surgeryShitfs.Select(s => new PostOpViewModel()
                {
                    Id = s.Id,
                    Gender = s.Patient.Gender,
                    PatientName = s.Patient.FullName,
                    Duration = GetDuration(s.DateUpdated.Value),
                    SurgeryName = s.SurgeryCatalog.Name,
                    StatusName = s.Status.Name,
                    SurgeonName = CombineString(s.SurgeryShiftSurgeons.Where(sg => !sg.IsDeleted).Select(sg => sg.Surgeon.FullName).ToList())
                });
                rs.AddRange(shifts);
                return rs;
            }

            if (isPostOp.HasValue)
            {
                if (isPostOp.Value)
                {
                    surgeryShitfs = surgeryShitfs.Where(s => s.StatusId == ConstantVariable.POST_STATUS_NUM).ToList();
                    rs.AddRange(surgeryShitfs.Select(s => new PostOpViewModel()
                    {
                        Id = s.Id,
                        Gender = s.Patient.Gender,
                        PatientName = s.Patient.FullName,
                        Duration = GetDuration(s.DateUpdated.Value),
                        SurgeryName = s.SurgeryCatalog.Name,
                        StatusName = s.Status.Name,
                        SurgeonName = CombineString(s.SurgeryShiftSurgeons.Where(sg => !sg.IsDeleted).Select(sg => sg.Surgeon.FullName).ToList())
                    }));
                }
                else
                {
                    surgeryShitfs = surgeryShitfs.Where(s => s.StatusId == ConstantVariable.RECOVERY_STATUS_NUM).ToList();
                    rs.AddRange(surgeryShitfs.Select(s => new PostOpViewModel()
                    {
                        Id = s.Id,
                        Gender = s.Patient.Gender,
                        PatientName = s.Patient.FullName,
                        Duration = GetDuration(s.DateUpdated.Value),
                        SurgeryName = s.SurgeryCatalog.Name,
                        StatusName = s.Status.Name,
                        SurgeonName = CombineString(s.SurgeryShiftSurgeons.Where(sg => !sg.IsDeleted).Select(sg => sg.Surgeon.FullName).ToList())
                    }));
                }
            }
            if (actualEnd != DateTime.MinValue)
            {
                var surgeryShiftsRs = surgeryShitfs.Where(s => s.ActualEndDateTime.Value.Date == actualEnd.Date).ToList();
                if (surgeryShiftsRs.Any())
                {
                    rs.AddRange(surgeryShiftsRs.Select(s => new PostOpViewModel()
                    {
                        Id = s.Id,
                        Gender = s.Patient.Gender,
                        PatientName = s.Patient.FullName,
                        Duration = GetDuration(s.DateUpdated.Value),
                        SurgeryName = s.SurgeryCatalog.Name,
                        StatusName = s.Status.Name,
                        SurgeonName = CombineString(s.SurgeryShiftSurgeons.Where(sg => !sg.IsDeleted).Select(sg => sg.Surgeon.FullName).ToList())
                    }));
                }
            }
            if (speacialtyId != 0)
            {
                var surgeryShiftsRs = surgeryShitfs.Where(s => s.SurgeryCatalog.SpecialtyId == speacialtyId).ToList();
                if (surgeryShiftsRs.Any())
                {
                    rs.AddRange(surgeryShiftsRs.Select(s => new PostOpViewModel()
                    {
                        Id = s.Id,
                        Gender = s.Patient.Gender,
                        PatientName = s.Patient.FullName,
                        Duration = GetDuration(s.DateUpdated.Value),
                        SurgeryName = s.SurgeryCatalog.Name,
                        StatusName = s.Status.Name,
                        SurgeonName = CombineString(s.SurgeryShiftSurgeons.Where(sg => !sg.IsDeleted).Select(sg => sg.Surgeon.FullName).ToList())
                    }));
                }
            }
            if (surgeryId != 0)
            {
                var surgeryShiftsRs = surgeryShitfs.Where(s => s.SurgeryCatalog.Id == surgeryId).ToList();
                if (surgeryShiftsRs.Any())
                {
                    rs.AddRange(surgeryShiftsRs.Select(s => new PostOpViewModel()
                    {
                        Id = s.Id,
                        Gender = s.Patient.Gender,
                        PatientName = s.Patient.FullName,
                        Duration = GetDuration(s.DateUpdated.Value),
                        SurgeryName = s.SurgeryCatalog.Name,
                        StatusName = s.Status.Name,
                        SurgeonName = CombineString(s.SurgeryShiftSurgeons.Where(sg => !sg.IsDeleted).Select(sg => sg.Surgeon.FullName).ToList())
                    }));
                }
            }
            if (doctorId != 0)
            {
                var surgeryShiftsRs = surgeryShitfs.Where(s => s.SurgeryShiftSurgeons.Any(x => x.Surgeon.Id == doctorId)).ToList();
                if (surgeryShiftsRs.Any())
                {
                    rs.AddRange(surgeryShiftsRs.Select(s => new PostOpViewModel()
                    {
                        Id = s.Id,
                        Gender = s.Patient.Gender,
                        PatientName = s.Patient.FullName,
                        Duration = GetDuration(s.DateUpdated.Value),
                        SurgeryName = s.SurgeryCatalog.Name,
                        StatusName = s.Status.Name,
                        SurgeonName = CombineString(s.SurgeryShiftSurgeons.Where(sg => !sg.IsDeleted).Select(sg => sg.Surgeon.FullName).ToList())
                    }));
                }
            }
            rs = rs.DistinctBy(x => x.Id).ToList();
            return rs;
        }

        public List<DoctorSearchViewModel> GetDoctors(string name)
        {
            var doctors = new List<Doctor>();
            if (!name.IsNullOrEmpty())
            {
                doctors = _appDbContext.Doctors.Where(d => !d.IsDeleted && d.FullName.Contains(name, StringComparison.CurrentCultureIgnoreCase)).ToList();
            }
            else
            {
                doctors = _appDbContext.Doctors.Where(d => !d.IsDeleted).ToList();
            }
            if (doctors.Any())
            {
                return doctors.Select(d => new DoctorSearchViewModel()
                {
                    Id = d.Id,
                    Name = d.FullName
                }).ToList();
            }
            return new List<DoctorSearchViewModel>();
        }

        public List<SpecialtySearchViewModel> GetSpecialties(string name)
        {
            var specialties = new List<Specialty>();
            if (!name.IsNullOrEmpty())
            {
                specialties = _appDbContext.Specialties.Where(s => !s.IsDeleted && s.Name.Contains(name, StringComparison.CurrentCultureIgnoreCase)).ToList();
            }
            else
            {
                specialties = _appDbContext.Specialties.Where(s => !s.IsDeleted).ToList();
            }
            if (specialties.Any())
            {
                return specialties.Select(s => new SpecialtySearchViewModel()
                {
                    Id = s.Id,
                    Name = s.Name
                }).ToList();
            }
            return new List<SpecialtySearchViewModel>();
        }

        public List<SurgeryCatalogSearchViewModel> GetSurgeryCatalogs(string name, int specialtyId)
        {
            if (specialtyId == 0)
            {
                var catalogs = new List<SurgeryCatalog>();

                if (!name.IsNullOrEmpty())
                {
                    catalogs = _appDbContext.SurgeryCatalogs.Where(sc => !sc.IsDeleted && (sc.Code.Contains(name, StringComparison.CurrentCultureIgnoreCase) || sc.Name.Contains(name, StringComparison.CurrentCultureIgnoreCase))).ToList();

                }
                else
                {
                    catalogs = _appDbContext.SurgeryCatalogs.Where(sc => !sc.IsDeleted).ToList();
                }

                if (catalogs.Any())
                {
                    return catalogs.Select(sc => new SurgeryCatalogSearchViewModel()
                    {
                        Id = sc.Id,
                        Name = $"{sc.Code} - {sc.Name}"
                    }).ToList();
                }
            }
            else
            {

                var catalogs = _appDbContext.SurgeryCatalogs.Where(sc => !sc.IsDeleted && sc.SpecialtyId == specialtyId);
                if (catalogs.Any())
                {
                    if (name.IsNullOrEmpty())
                    {
                        catalogs = catalogs.Where(sc => (sc.Code.Contains(name, StringComparison.CurrentCultureIgnoreCase) || sc.Name.Contains(name, StringComparison.CurrentCultureIgnoreCase)));
                    }
                    if (catalogs.Any())
                    {
                        return catalogs.Select(sc => new SurgeryCatalogSearchViewModel()
                        {
                            Id = sc.Id,
                            Name = $"{sc.Code} - {sc.Name}"
                        }).ToList();
                    }
                }
            }

            return new List<SurgeryCatalogSearchViewModel>();
        }
        #endregion

        #region Processing
        private string CombineString(List<string> names)
        {
            var result = "";
            for (int i = 0; i < names.Count; i++)
            {
                if (i == names.Count - 1)
                {
                    result += $"{names[i]}.";
                }
                else
                {
                    result += $"{names[i]}; ";
                }
            }
            return result;
        }

        private string GetDuration(DateTime date)
        {
            var duration = DateTime.Now - date;
            return $"{(double)decimal.Round((decimal)duration.TotalHours, 0)}";
        }
        #endregion
    }
}
