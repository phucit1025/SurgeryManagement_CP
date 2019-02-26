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

        public void ImportSurgeryShift(ICollection<ImportSurgeryShiftViewModel> surgeryShift)
        {
            var list = surgeryShift;
            foreach (var s in surgeryShift)
            {
                var shift = new SurgeryShift();
                shift.IsDeleted = false;
                shift.DateCreated = DateTime.Now;
                shift.IsAvailableMedicalSupplies = false;
                //shift.StatusId = 1;

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
                _context.SurgeryShifts.Add(shift);
            }
            _context.SaveChanges();
        }

        public void ImportSurgeryShiftMedicalSupply(ICollection<ImportMedicalSupplyViewModel> medicalSupply)
        {
            foreach (var tmp in medicalSupply)
            {
                var shiftSupply = new SurgeryShiftMedicalSupply();
                int surgeryShiftId = _context.SurgeryShifts.Where(a => a.SurgeryShiftCode == tmp.SurgeryShiftCode).Single().Id;
                shiftSupply.SurgeryShiftId = surgeryShiftId;
                shiftSupply.MedicalSupplyId = tmp.MedicalSupplyId;
                _context.SurgeryShiftMedicalSupplies.Add(shiftSupply);
            }
            _context.SaveChanges();
        }

    }
}
