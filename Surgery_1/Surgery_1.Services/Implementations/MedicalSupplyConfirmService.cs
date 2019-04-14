﻿using Surgery_1.Data.Context;
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
                r.ScheduleDate = r.ConfirmDate.Value.Date;
                if (!r.IsNormalSurgeryTime)
                {
                    if (r.ProposedStartDateTime.Value.Date >= r.ConfirmDate.Value.Date)
                    {
                        r.ScheduleDate = r.ProposedStartDateTime.Value.Date;
                    }
                    if (r.ProposedStartDateTime < DateTime.Now || r.ProposedEndDateTime < DateTime.Now)
                    {
                        r.IsNormalSurgeryTime = true;
                    }
                }
            }
            _context.SaveChanges();
        }

        public ICollection<MedicalSupplyRequestViewModel> GetAllMedicalSupplyRequest()
        {
            var result = new List<MedicalSupplyRequestViewModel>();
            var request = _context.SurgeryShifts.Where(a => a.IsAvailableMedicalSupplies == false).ToList();
            foreach (var r in request)
            {
                var supplies = _context.SurgeryShiftMedicalSupplies.Where(s => s.SurgeryShiftId == r.Id).ToList();
                var suppliesViewModels = GetMedicalSupplyRequestDetail(r.Id);
                result.Add(new MedicalSupplyRequestViewModel()
                {
                    Id = r.Id,
                    PatientName = r.Patient.FullName,
                    SpecialtyName = r.SurgeryCatalog.Specialty.Name,
                    SurgeryName = r.SurgeryCatalog.Name,
                    SurgeryCatalogId = r.SurgeryCatalogId.ToString(),
                    CreatedDate = r.DateCreated.ToString(),
                    MedicalSupplies = suppliesViewModels,
                });
            }
            return result;
        }

        public bool ConfirmedSupply(ICollection<MedicalSupplyIdConfirmViewModel> surgeryShift)
        {
            foreach(var s in surgeryShift)
            {
                var shift = _context.SurgeryShifts.Find(s.id);
                shift.IsAvailableMedicalSupplies = true;
                shift.ConfirmDate = DateTime.Now;
                if (shift.ProposedStartDateTime != null && shift.ProposedEndDateTime != null)
                {
                    shift.ScheduleDate = shift.ProposedStartDateTime.Value.Date;
                }
                else
                {
                    shift.ScheduleDate = shift.ConfirmDate.Value.Date;
                }
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
                    id = d.Id,
                    supplyId = d.MedicalSupplyId,
                    supplyName = d.MedicalSupply.Name,
                    quantity = d.Quantity
                }
                );
            }
            return detail;
        }
    }
}
