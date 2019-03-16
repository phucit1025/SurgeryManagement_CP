using Surgery_1.Data.Context;
using Surgery_1.Data.Entities;
using Surgery_1.Data.ViewModels;
using Surgery_1.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Surgery_1.Data.ViewModels.PostOpSurgeryShiftViewModel;

namespace Surgery_1.Services.Implementations
{
    public class SurgeryShiftService : ISurgeryShiftService
    {
        private readonly AppDbContext _context;
        public SurgeryShiftService(AppDbContext _context)
        {
            this._context = _context;
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

        public void ImportSurgeryShift(ICollection<ImportSurgeryShiftViewModel> surgeryShift)
        {
            foreach (var s in surgeryShift)
            {
                //var status = _context.Statuses;
                var shift = new SurgeryShift();
                shift.IsDeleted = false;
                shift.DateCreated = DateTime.Now;
                shift.IsAvailableMedicalSupplies = false;
                var status = _context.Statuses.Where(x => x.Name.Equals("Preoperative")).FirstOrDefault();
                shift.StatusId = status.Id;
                shift.ExpectedSurgeryDuration = s.ExpectedSurgeryDuration;
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
                shift.PatientId = patient.Id;
                shift.SurgeryCatalogId = s.SurgeryCatalogID;
                shift.SurgeryShiftCode = s.SurgeryShiftCode;
                shift.ProposedStartDateTime = s.ProposedStartDateTime;
                shift.ProposedEndDateTime = s.ProposedEndDateTime;
                shift.TreatmentDoctorId = s.DoctorId;
                if (s.ProposedStartDateTime != null && s.ProposedEndDateTime != null)
                {
                    shift.IsNormalSurgeryTime = false; //Cờ để phân biệt mổ chỉ định vs mổ bình thường, mặc định là true
                }
                _context.SurgeryShifts.Add(shift);
            }
            _context.SaveChanges();
        }

        public void ImportSurgeryShiftMedicalSupply(ICollection<ImportMedicalSupplyViewModel> medicalSupply)
        {
            foreach (var tmp in medicalSupply)
            {
                var shiftSupply = new SurgeryShiftMedicalSupply();
                var surgeryShift = _context.SurgeryShifts.Where(a => a.SurgeryShiftCode == tmp.SurgeryShiftCode).FirstOrDefault();
                if (surgeryShift == null)
                    continue;
                shiftSupply.SurgeryShiftId = surgeryShift.Id;
                shiftSupply.MedicalSupplyId = tmp.MedicalSupplyId;
                shiftSupply.Quantity = tmp.Quantity;
                _context.SurgeryShiftMedicalSupplies.Add(shiftSupply);
            }
            _context.SaveChanges();
        }

        public void AddMedicalSupply(ICollection<AddMedicalSupplyViewModel> medicalSupply)
        {
            foreach (var tmp in medicalSupply)
            {
                var existed = _context.SurgeryShiftMedicalSupplies.Where(a => a.SurgeryShiftId == tmp.SurgeryShiftId)
                    .Where(a => a.MedicalSupplyId == tmp.MedicalSupplyId).FirstOrDefault();
                if (existed != null)
                {
                    existed.Quantity += tmp.Quantity;
                }
                else
                {
                    var shiftSupply = new SurgeryShiftMedicalSupply();

                    shiftSupply.SurgeryShiftId = tmp.SurgeryShiftId;
                    shiftSupply.MedicalSupplyId = tmp.MedicalSupplyId;
                    shiftSupply.Quantity = tmp.Quantity;
                    _context.SurgeryShiftMedicalSupplies.Add(shiftSupply);
                }
            }
            _context.SaveChanges();
        }

        public ICollection<MedicalSupplyInfoViewModel> GetSuppliesUsedInSurgery(int surgeryShiftId)
        {
            List<MedicalSupplyInfoViewModel> list = new List<MedicalSupplyInfoViewModel>();
            var supplies = _context.SurgeryShiftMedicalSupplies.Where(a => a.SurgeryShiftId == surgeryShiftId).ToList();
            UtilsService util = new UtilsService(_context);
            List<MedicalSupplyInfoViewModel> u = new List<MedicalSupplyInfoViewModel>();
            u = (List<MedicalSupplyInfoViewModel>)util.GetMedicalSupply();
            foreach (var supply in supplies)
            {
                MedicalSupplyInfoViewModel s = new MedicalSupplyInfoViewModel();
                s.medicalSupplyId = supply.MedicalSupplyId;
                s.medicalSupplyName = u[supply.MedicalSupplyId].medicalSupplyName;
                s.quantity = supply.Quantity;
                list.Add(s);
            }
            return list;
        }
    }
}
