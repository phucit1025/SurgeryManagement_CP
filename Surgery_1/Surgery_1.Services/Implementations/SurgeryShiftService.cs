using Microsoft.AspNetCore.Identity;
using MoreLinq;
using Surgery_1.Data.Context;
using Surgery_1.Data.Entities;
using Surgery_1.Data.ViewModels;
using Surgery_1.Services.Interfaces;
using Surgery_1.Services.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Surgery_1.Data.ViewModels.PostOpSurgeryShiftViewModel;

namespace Surgery_1.Services.Implementations
{
    public class SurgeryShiftService : ISurgeryShiftService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly INotificationService _notificationService;


        public SurgeryShiftService(AppDbContext context, UserManager<IdentityUser> userManager, INotificationService _notificationService)
        {
            this._context = context;
            this._userManager = userManager;
            this._notificationService = _notificationService;
        }

        #region Technical Staffs
        private async Task<ICollection<TechnicalStaffInfoViewModel>> GetAssignableTechnicalStaff(DateTime? startTime, DateTime? endTime)
        //startTime, endTime is the time of surgery shift wanting to assign a technical
        {
            //Assign theo tung shift: lay thoi gian bat dau va thoi gian ket thuc cua surgery shift can assign
            //Tim tat ca nhung technical staff k phu trach surgery nao trong khoang thoi gian cho truoc

            /* Get all technical staffs to List */
            var result = new List<TechnicalStaffInfoViewModel>();
            var usersInRole = await _userManager.GetUsersInRoleAsync("Technical");
            var staffs = new List<TechnicalStaffInfoViewModel>();
            foreach (var userInRole in usersInRole)
            {
                //var user = new UserInfo();
                //user.FullName = "Techinical"; user.GuId = userInRole.Id; user.Email = "a@gmail.com"; user.PhoneNumber = "653145897";
                //_context.UserInfo.Add(user);
                //_context.SaveChanges();

                var staff = _context.UserInfo.FirstOrDefault(c => c.GuId.Equals(userInRole.Id));
                if (staff != null)
                    staffs.Add(new TechnicalStaffInfoViewModel()
                    { technicalStaffId = staff.Id, technicalStaffName = staff.FullName });
            }

            /* check condition for assignable */
            foreach (var staff in staffs)
            {
                var time = _context.SurgeryShifts.Where(a => !a.IsDeleted &&
                    a.Status.Name == "Preoperative" && a.TechId == staff.technicalStaffId &&
                    (a.EstimatedStartDateTime <= startTime && endTime <= a.EstimatedEndDateTime)).Count();
                if (time > 0) continue;
                result.Add(new TechnicalStaffInfoViewModel() { technicalStaffId = staff.technicalStaffId });
            }
            return result;
        }

        public bool AssignTechnicalStaff()
        {
            List<SmsShiftViewModel> notiShift = new List<SmsShiftViewModel>();
            try
            {
                var preOperative = _context.SurgeryShifts.Where(s => s.Status.Name == ConstantVariable.PRE_STATUS &&
                    (s.EstimatedStartDateTime != null && s.EstimatedEndDateTime != null) && s.TechId == null).ToList();
                foreach (var shift in preOperative)
                {
                    DateTime? startTime = shift.EstimatedStartDateTime, endTime = shift.EstimatedEndDateTime;
                    var techStaffs = GetAssignableTechnicalStaff(startTime, endTime).Result;
                    if (techStaffs.Any())
                    {
                        var tId = techStaffs.RandomSubset(1).ToList().FirstOrDefault().technicalStaffId;
                        shift.TechId = tId;
                        //notify
                        notiShift.Add(new SmsShiftViewModel
                        {
                            Id = shift.Id,
                            EstimatedStartDateTime = shift.EstimatedStartDateTime.Value,
                            TechnicalId = tId
                        });
                    }

                }
                _context.SaveChanges();
                _notificationService.AddNotificationForTechnicalStaff(notiShift);
                return true;
            }
            catch (Exception) { return false; }
        }
        #endregion

        public bool ImportSurgeryShift(ICollection<ImportSurgeryShiftViewModel> surgeryShifts)
        {
            var countDuplicated = 0;
            try
            {
                foreach (var s in surgeryShifts)
                {
                    var shift = new SurgeryShift();
                    shift.IsAvailableMedicalSupplies = false;
                    shift.StatusId = _context.Statuses.Where(x => x.Name.Equals(ConstantVariable.PRE_STATUS)).FirstOrDefault().Id;
                    shift.PriorityNumber = s.PriorityNumber;
                    var patient = _context.Patients.Where(p => p.IdentityNumber == s.PatientID).FirstOrDefault();
                    if (patient == null)
                    {
                        patient = new Patient();
                        patient.IdentityNumber = s.PatientID;
                        patient.YearOfBirth = s.YearOfBirth;
                        patient.FullName = s.PatientName;
                        patient.Gender = s.Gender;
                        _context.Patients.Add(patient);
                        _context.SaveChanges();
                        patient = _context.Patients.Where(p => p.IdentityNumber == s.PatientID).Single();
                    }
                    //else
                    //{
                    //    var doublicated = _context.SurgeryShifts.Where(a => a.PatientId == patient.Id &&
                    //        a.SurgeryCatalogId == s.SurgeryCatalogID && a.Status.Name != ConstantVariable.FINISHED_STATUS).FirstOrDefault();
                    //    if (doublicated != null)
                    //    {
                    //        countDuplicated++;
                    //        continue;
                    //    }
                    //}
                    shift.PatientId = patient.Id;
                    shift.SurgeryCatalogId = s.SurgeryCatalogID;
                    shift.ProposedStartDateTime = s.ProposedStartDateTime;
                    shift.ProposedEndDateTime = s.ProposedEndDateTime;
                    if (shift.ProposedStartDateTime != null && shift.ProposedEndDateTime != null)
                    {
                        shift.ExpectedSurgeryDuration = (float)(shift.ProposedEndDateTime.Value - shift.ProposedStartDateTime.Value).TotalHours;
                    }
                    else
                    {
                        shift.ExpectedSurgeryDuration = s.ExpectedDuration;
                    }
                    shift.TreatmentDoctorId = s.DoctorId;
                    if (s.ProposedStartDateTime != null && s.ProposedEndDateTime != null)
                    {
                        shift.IsNormalSurgeryTime = false; //Cờ để phân biệt mổ chỉ định vs mổ bình thường, mặc định là true
                    }
                    _context.SurgeryShifts.Add(shift);
                    _context.SaveChanges();
                    var shiftId = shift.Id;

                    //Add surgeon to Surgeryshift
                    var surgeon = new SurgeryShiftSurgeon();
                    surgeon.SurgeonId = s.DoctorId;
                    surgeon.SurgeryShiftId = shiftId;
                    _context.SurgeryShiftSurgeons.Add(surgeon);
                    _context.SaveChanges();

                    foreach (var tmp in s.DetailMedical)
                    {
                        var shiftSupply = new SurgeryShiftMedicalSupply();
                        shiftSupply.SurgeryShiftId = shiftId;
                        shiftSupply.MedicalSupplyId = tmp.Code;
                        shiftSupply.Quantity = tmp.Quantity;
                        _context.SurgeryShiftMedicalSupplies.Add(shiftSupply);
                    }
                    _context.SaveChanges();
                }
                // Xử lý notification
                int countNoti = surgeryShifts.Count;
                if (countDuplicated != countNoti)
                {
                    var notification = new Notification
                    {
                        Content = "There are " + (countNoti - countDuplicated) + " new medical supplies request need to be confirmed",
                        RoleToken = ConstantVariable.SUPPLYSTAFF
                    };
                    _context.Notifications.Add(notification);
                    _context.SaveChanges();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool AddMedicalSupply(ShiftMedicalSuppliesViewModel medicalSupply)
        {
            try
            {
                foreach (var tmp in medicalSupply.ShiftMedicals)
                {
                    var existed = _context.SurgeryShiftMedicalSupplies.Where(a => a.SurgeryShiftId == tmp.SurgeryShiftId)
                        .Where(a => a.MedicalSupplyId == tmp.MedicalSupplyId).FirstOrDefault();
                    if (existed != null)
                    {
                        existed.Quantity = tmp.Quantity;
                        if (existed.IsDeleted)
                        {
                            existed.IsDeleted = false;
                        }
                    }
                    else if (tmp.Quantity > 0)
                    {
                        var shiftSupply = new SurgeryShiftMedicalSupply();

                        shiftSupply.SurgeryShiftId = tmp.SurgeryShiftId;
                        shiftSupply.MedicalSupplyId = tmp.MedicalSupplyId;
                        shiftSupply.Quantity = tmp.Quantity;
                        _context.SurgeryShiftMedicalSupplies.Add(shiftSupply);
                    }
                }
                foreach (var id in medicalSupply.DeleteMedicalSupplyIds)
                {
                    var rs = _context.SurgeryShiftMedicalSupplies.Find(id);
                    rs.IsDeleted = true;
                    _context.SurgeryShiftMedicalSupplies.Update(rs);
                }
                _context.SaveChanges();
                return true;
            }
            catch (Exception) { return false; }
        }

        public ICollection<MedicalSupplyInfoViewModel> GetSuppliesUsedInSurgery(int surgeryShiftId)
        {
            List<MedicalSupplyInfoViewModel> list = new List<MedicalSupplyInfoViewModel>();
            var supplies = _context.SurgeryShiftMedicalSupplies.Where(a => a.SurgeryShiftId == surgeryShiftId && !a.IsDeleted).OrderBy(s => s.MedicalSupplyId).ToList();
            foreach (var supply in supplies)
            {
                if (supply.Quantity == 0) continue;
                MedicalSupplyInfoViewModel s = new MedicalSupplyInfoViewModel();
                s.id = supply.Id;
                s.medicalSupplyId = supply.MedicalSupplyId;
                s.medicalSupplyName = _context.MedicalSupplies.Find(supply.MedicalSupplyId).Name;
                s.quantity = supply.Quantity;
                list.Add(s);
            }
            return list;
        }

        public ICollection<EkipMemberViewModel> GetEkipMember(int surgeryShiftId)
        {
            List<EkipMemberViewModel> list = new List<EkipMemberViewModel>();
            //Load Treatment Doctor
            var techName = _context.SurgeryShifts.Find(surgeryShiftId).TechnicalStaff.FullName;
            EkipMemberViewModel techStaff = new EkipMemberViewModel();
            techStaff.Name = techName;
            techStaff.WorkJob = "Technical Staff";
            list.Add(techStaff);
            //Load Surgeons
            var surgeons = _context.SurgeryShiftSurgeons.Where(a => a.IsDeleted == false && a.SurgeryShiftId == surgeryShiftId);
            foreach (var surgeon in surgeons)
            {
                EkipMemberViewModel member = new EkipMemberViewModel();
                member.Name = _context.Doctors.Find(surgeon.SurgeonId).FullName;
                member.WorkJob = "Surgeon";
                list.Add(member);
            }
            //Load Ekip Members
            var ekipId = _context.SurgeryShifts.Find(surgeryShiftId).EkipId;
            var ekipMembers = _context.EkipMembers.Where(a => a.EkipId == ekipId).ToList();
            foreach (var emember in ekipMembers)
            {
                EkipMemberViewModel member = new EkipMemberViewModel();
                member.Name = emember.Name;
                member.WorkJob = emember.WorkJob;
                list.Add(member);
            }
            return list;
        }

        #region Surgeons

        public ICollection<SurgeonsViewModel> GetShiftSurgeons(int surgeryShiftId)
        {
            var surgeons = _context.SurgeryShiftSurgeons.Where(s => !s.IsDeleted && s.SurgeryShiftId == surgeryShiftId).Select(s => s.Surgeon).ToList();
            if (surgeons.Any())
            {
                return surgeons.Select(s => new SurgeonsViewModel()
                {
                    Id = s.Id,
                    Name = s.FullName
                }).ToList();
            }
            else
            {
                return new List<SurgeonsViewModel>();
            }
        }

        public ICollection<SurgeonsViewModel> GetAvailableSurgeons(int surgeryShiftId)
        {
            var surgeons = _context.Doctors.Where(d => !d.IsDeleted).ToList();
            var currentShift = _context.SurgeryShifts.Find(surgeryShiftId);

            var affectedShifts = _context.SurgeryShifts
                .Where(s => !s.IsDeleted &&
                    s.Status.Name.Equals("Preoperative") &&
                    currentShift.EstimatedStartDateTime.Value.DayOfYear == s.EstimatedStartDateTime.Value.DayOfYear &&
                    !(s.EstimatedStartDateTime.Value >= currentShift.EstimatedEndDateTime.Value) ||
                    !(s.EstimatedEndDateTime.Value <= currentShift.EstimatedStartDateTime.Value)
                ).ToList();
            if (affectedShifts.Any())
            {
                var affectedSurgeons = new List<Doctor>();
                foreach (var affectedShift in affectedShifts)
                {
                    var stemp = affectedShift.SurgeryShiftSurgeons.Where(s => !s.IsDeleted).Select(s => s.Surgeon).ToList();
                    affectedSurgeons.AddRange(stemp);
                }
                affectedSurgeons = affectedSurgeons.DistinctBy(s => s.Id).ToList();
                if (affectedSurgeons.Any())
                {
                    var availableSurgeons = surgeons.ExceptBy(affectedSurgeons, a => (a.Id, affectedSurgeons.FirstOrDefault().Id)).ToList();

                    if (availableSurgeons.Any())
                    {
                        return availableSurgeons.Select(c => new SurgeonsViewModel()
                        {
                            Id = c.Id,
                            Name = c.FullName
                        }).ToList();
                    }
                    else
                    {
                        return new List<SurgeonsViewModel>();
                    }
                }
                else
                {
                    return surgeons.Select(c => new SurgeonsViewModel()
                    {
                        Id = c.Id,
                        Name = c.FullName
                    }).ToList();
                }
            }
            else
            {
                return surgeons.Select(s => new SurgeonsViewModel()
                {
                    Id = s.Id,
                    Name = s.FullName
                }).ToList();
            }

        }

        public bool UpdateSurgeon(UpdateSurgeonsViewModel updatedSurgeon)
        {
            try
            {
                var surgeonsInShift = _context.SurgeryShiftSurgeons.Where(s => !s.IsDeleted && s.SurgeryShiftId == updatedSurgeon.SurgeryShiftId).ToList();
                var surgeonMapping = surgeonsInShift.FirstOrDefault(m => m.SurgeonId == updatedSurgeon.OldSurgeonId);
                surgeonMapping.SurgeonId = updatedSurgeon.UpdatedSurgeonId;
                surgeonMapping.DateUpdated = DateTime.Now;
                _context.Update(surgeonMapping);
                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool AddSurgeon(AddSurgeonToShiftViewModel model)
        {
            try
            {
                var surgeonMapping = new SurgeryShiftSurgeon()
                {
                    SurgeryShiftId = model.SurgeryShiftId,
                    SurgeonId = model.SurgeonId
                };
                _context.Add(surgeonMapping);
                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool RemoveSurgeon(RemoveSurgeonFromShiftViewModel model)
        {
            try
            {
                var surgeonsInShift = _context.SurgeryShiftSurgeons.Where(s => !s.IsDeleted && s.SurgeryShiftId == model.SurgeryShiftId).ToList();
                var surgeonMapping = surgeonsInShift.FirstOrDefault(m => m.SurgeonId == model.SurgeonId);
                surgeonMapping.IsDeleted = true;
                surgeonMapping.DateUpdated = DateTime.Now;
                _context.Update(surgeonMapping);
                _context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        //Update emergency
        public bool UpdateSurgeryProfile(EditSurgeryShiftViewModel editForm)
        {
            var patient = _context.Patients.Where(p => p.IdentityNumber == editForm.EditIdentityNumber).FirstOrDefault();
            var surgeryShift = _context.SurgeryShifts.Find(editForm.ShiftId);
            if (!surgeryShift.IsNormalSurgeryTime && surgeryShift.ProposedStartDateTime == null)
            {
                if (patient == null)
                {
                    var insertedPatient = new Patient()
                    {
                        IdentityNumber = editForm.EditIdentityNumber,
                        FullName = editForm.EditPatientName,
                        Gender = editForm.EditGender.Value,
                        YearOfBirth = editForm.EditYob.Value
                    };
                    _context.Patients.Add(insertedPatient);

                    if (_context.SaveChanges() > 0)
                    {
                        int patientId = insertedPatient.Id;


                        //update surgery shift
                        surgeryShift.PatientId = patientId;
                        surgeryShift.SurgeryCatalogId = editForm.EditSurgeryId;

                        _context.Update(surgeryShift);
                        _context.SaveChanges();

                        return true;
                    }
                }
                else
                {
                    surgeryShift.PatientId = patient.Id;
                    surgeryShift.SurgeryCatalogId = editForm.EditSurgeryId;

                    _context.SaveChanges();

                    return true;
                }
            }

            return false;
        }

        public EditSurgeryShiftViewModel LoadEditSurgeryProfile(int shiftId)
        {
            var result = _context.SurgeryShifts.Find(shiftId);
            var surgeryShift = new EditSurgeryShiftViewModel();
            if (result != null)
            {
                if (result.Patient != null && result.SurgeryCatalog != null)
                {
                    surgeryShift = new EditSurgeryShiftViewModel
                    {
                        ShiftId = result.Id,
                        EditIdentityNumber = result.Patient.IdentityNumber,
                        EditGender = result.Patient.Gender,
                        EditPatientName = result.Patient.FullName,
                        EditYob = result.Patient.YearOfBirth,
                        EditSurgeryId = result.SurgeryCatalogId,
                        SurgeryCode = result.SurgeryCatalog.Code,
                        SurgeryName = result.SurgeryCatalog.Name
                    };
                }
                return surgeryShift;
            }
            return null;
        }

        public ICollection<SelectedSurgeryCatalogViewModel> GetSurgeryCatalogOnQuery(string searchName)
        {

            var catalogs = _context.SurgeryCatalogs.Where(s => s.Name.Contains(searchName) || s.Code.Contains(searchName))
                .Take(10).OrderBy(a => a.Code).ToList();
            var results = new List<SelectedSurgeryCatalogViewModel>();

            foreach (var catalog in catalogs)
            {
                results.Add(new SelectedSurgeryCatalogViewModel()
                {
                    Id = catalog.Id,
                    Code = catalog.Code,
                    Name = catalog.Name
                });
            }
            return results;
        }

    }
}
