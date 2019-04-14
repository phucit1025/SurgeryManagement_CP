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
        private readonly string SUPPLYSTAFF = "MedicalSupplier";

        public SurgeryShiftService(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            this._context = context;
            this._userManager = userManager;
        }

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
            try
            {
                var preOperative = _context.SurgeryShifts.Where(s => s.Status.Name == "Preoperative" &&
                    (s.EstimatedStartDateTime != null && s.EstimatedEndDateTime != null) && s.TechId == null).ToList();
                foreach (var shift in preOperative)
                {
                    DateTime? startTime = shift.EstimatedStartDateTime, endTime = shift.EstimatedEndDateTime;
                    var techStaffs = GetAssignableTechnicalStaff(startTime, endTime).Result;
                    if (techStaffs.Any())
                    {
                        var tId = techStaffs.RandomSubset(1).ToList().FirstOrDefault().technicalStaffId;
                        shift.TechId = tId;
                    }
                }
                _context.SaveChanges();
                return true;
            }
            catch (Exception) { return false; }
        }

        public ICollection<SurgeryCatalogNamesViewModel> GetSurgeryName(ICollection<SurgeryCatalogIDsViewModel> ids)
        {
            var result = new List<SurgeryCatalogNamesViewModel>();
            foreach (var id in ids)
            {
                SurgeryCatalogNamesViewModel sname = new SurgeryCatalogNamesViewModel();
                sname.name = _context.SurgeryCatalogs.Where(a => a.Id == id.id).FirstOrDefault().Name;
                result.Add(sname);
            }
            return result;
        }

        public bool ImportSurgeryShift(ICollection<ImportSurgeryShiftViewModel> surgeryShifts)
        {
            try
            {
                foreach (var s in surgeryShifts)
                {
                    var shift = new SurgeryShift();
                    shift.IsAvailableMedicalSupplies = false;
                    shift.StatusId = _context.Statuses.Where(x => x.Name.Equals(ConstantVariable.PRE_STATUS)).FirstOrDefault().Id;
                    shift.PriorityNumber = s.Priority;
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
                    shift.PatientId = patient.Id;
                    shift.SurgeryCatalogId = s.SurgeryCatalogID;
                    shift.SurgeryShiftCode = s.SurgeryShiftCode;
                    shift.ProposedStartDateTime = s.ProposedStartDateTime;
                    shift.ProposedEndDateTime = s.ProposedEndDateTime;
                    if (shift.ProposedStartDateTime != null && shift.ProposedEndDateTime != null)
                    {
                        shift.ExpectedSurgeryDuration = (float)(shift.ProposedEndDateTime.Value - shift.ProposedEndDateTime.Value).TotalHours;
                    }
                    else
                    {
                        shift.ExpectedSurgeryDuration = s.ExpectedSurgeryDuration;
                    }
                    shift.TreatmentDoctorId = s.DoctorId;
                    if (s.ProposedStartDateTime != null && s.ProposedEndDateTime != null)
                    {
                        shift.IsNormalSurgeryTime = false; //Cờ để phân biệt mổ chỉ định vs mổ bình thường, mặc định là true
                    }
                    _context.SurgeryShifts.Add(shift);
                    _context.SaveChanges();

                    var shiftId = shift.Id;
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
                var notification = new Notification
                {
                    Content = "There are " + countNoti + " new medical supplies request need to be confirmed",
                    RoleToken = SUPPLYSTAFF
                };
                _context.Notifications.Add(notification);
                _context.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public bool ImportSurgeryShiftMedicalSupply(ICollection<ImportMedicalSupplyViewModel> medicalSupply)
        {
            try
            {
                foreach (var tmp in medicalSupply)
                {
                    var shiftSupply = new SurgeryShiftMedicalSupply();
                    var surgeryShift = _context.SurgeryShifts.Where(a => a.SurgeryShiftCode == tmp.SurgeryShiftCode).FirstOrDefault();
                    if (surgeryShift == null)
                        continue;
                    shiftSupply.SurgeryShiftId = surgeryShift.Id;
                    shiftSupply.MedicalSupplyId = tmp.Code;
                    shiftSupply.Quantity = tmp.Quantity;
                    _context.SurgeryShiftMedicalSupplies.Add(shiftSupply);
                }
                _context.SaveChanges();
                return true;
            }
            catch (Exception) { return false; }
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
            //Load Surgeons
            var surgeons = _context.SurgeryShiftSurgeons.Where(a => a.SurgeryShiftId == surgeryShiftId);
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

        public bool UpdateMedicalSupply(ICollection<ShiftMedicalSupplyViewModel> medicalSupply)
        {
            try
            {
                foreach (var tmp in medicalSupply)
                {
                    var shiftSupply = _context.SurgeryShiftMedicalSupplies.Where(a => a.SurgeryShiftId == tmp.SurgeryShiftId)
                         .Where(a => a.MedicalSupplyId == tmp.MedicalSupplyId).FirstOrDefault();
                    shiftSupply.Quantity = tmp.Quantity;
                }
                _context.SaveChanges();
                return true;
            }
            catch (Exception) { return false; }
        }
    }
}
