using Surgery_1.Data.Context;
using Surgery_1.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Surgery_1.Services.Implementations
{
    class MedicalSupplyCofirmService : IMedicalSupplyConfirm
    {
        private readonly AppDbContext _context;
        public MedicalSupplyCofirmService(AppDbContext _context)
        {
            this._context = _context;
        }

        public void InsertSupplyRequest(int supplyId, int surgeryShiftId)
        {
            Data.Entities.SurgeryShiftMedicalSupply data = new Data.Entities.SurgeryShiftMedicalSupply();
            //TODO add data
            data.MedicalSupplyId = supplyId;
            data.SurgeryShiftId = surgeryShiftId;

            _context.SurgeryShiftMedicalSupplies.Add(data);
            _context.SaveChanges();
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
    }
}
