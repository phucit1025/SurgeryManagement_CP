using Surgery_1.Data.Context;
using Surgery_1.Data.ViewModels;
using Surgery_1.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Surgery_1.Services.Implementations
{
    public class MedicalSupplyConfirmService : IMedicalSupplyConfirmService
    {
        private readonly AppDbContext _context;
        public MedicalSupplyConfirmService(AppDbContext _context)
        {
            this._context = _context;
        }

        public void ConfirmedAllSupplyRequest()
        {
            var request = _context.SurgeryShifts.Where(a => a.IsAvailableMedicalSupplies == false).ToList();
            foreach (var r in request)
            {
                r.IsAvailableMedicalSupplies = true;
            }
            _context.SaveChanges();
        }

        public ICollection<MedicalSupplyRequestViewModel> GetAllMedicalSupplyRequest()
        {
            var result = new List<MedicalSupplyRequestViewModel>();
            var request = _context.SurgeryShifts.Where(a => a.IsAvailableMedicalSupplies == false).ToList();
            foreach (var r in request)
            {
                result.Add(new MedicalSupplyRequestViewModel()
                {
                    Id = r.Id,
                    PatientName = r.Patient.FullName,
                    SurgeryName = r.SurgeryCatalog.Name,
                    SurgeryCatalogId = r.SurgeryCatalogId.ToString(),
                    CreatedDate = r.DateCreated.ToString()
                });
            }
            return result;
        }

        public bool ConfirmedSupply(int surgeryShiftId)
        {
            var shift = _context.SurgeryShifts.Where(a => a.Id == surgeryShiftId).Single();
            if (shift == null)
                return false;
            shift.IsAvailableMedicalSupplies = true;
            _context.SaveChanges();
            return true;
        }

        public ICollection<MedicalSupplyRequestDetailViewModel> GetMedicalSupplyRequestDetail(int surgeryShitfId)
        {
            var detail = new List<MedicalSupplyRequestDetailViewModel>();
            var requestDetail = _context.SurgeryShiftMedicalSupplies.Where(a => a.SurgeryShiftId == surgeryShitfId).ToList();
            foreach (var d in requestDetail)
            {
                detail.Add(new MedicalSupplyRequestDetailViewModel()
                {
                    code = d.Id,
                    name = d.MedicalSupply.Name
                    //,quantity = d.quantity
                }
                );
            }
            return detail;
        }
    }
}
