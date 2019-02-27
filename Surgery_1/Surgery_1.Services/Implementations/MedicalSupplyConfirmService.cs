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
                r.ConfirmDate = DateTime.Now;
                if (r.ProposedStartDateTime != null && r.ProposedEndDateTime != null)
                {
                    r.ScheduleDate = new DateTime(r.ProposedStartDateTime.Value.Year, r.ProposedStartDateTime.Value.Month, r.ProposedStartDateTime.Value.Day);
                } else
                {
                    r.ScheduleDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
                }
                
            }
            _context.SaveChanges();
        }

        public bool ConfirmAll()
        {
            var request = _context.SurgeryShifts.Where(a => a.IsAvailableMedicalSupplies == false).ToList();
            foreach (var r in request)
            {
                r.IsAvailableMedicalSupplies = true;
                r.ConfirmDate = DateTime.Now;
                if (r.ProposedStartDateTime != null && r.ProposedEndDateTime != null)
                {
                    r.ScheduleDate = new DateTime(r.ProposedStartDateTime.Value.Year, r.ProposedStartDateTime.Value.Month, r.ProposedStartDateTime.Value.Day);
                }
                else
                {
                    r.ScheduleDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
                }

            }
            _context.SaveChanges();
            return true;
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

        public bool ConfirmedSupply(ICollection<MedicalSupplyIdConfirmViewModel> surgeryShift)
        {
            foreach(var s in surgeryShift)
            {
                _context.SurgeryShifts.Where(a => a.Id == s.id).FirstOrDefault().IsAvailableMedicalSupplies = true;
            }
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
