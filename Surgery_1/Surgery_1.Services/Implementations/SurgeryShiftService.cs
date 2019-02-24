using Surgery_1.Data.Context;
using Surgery_1.Data.Entities;
using Surgery_1.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Surgery_1.Services.Implementations
{
    class SurgeryShiftService : ISurgeryShiftService
    {
        private readonly AppDbContext _context;
        public SurgeryShiftService(AppDbContext _context)
        {
            this._context = _context;
        }

        public bool ImportSurgeryShift()
        {
            var shift = new SurgeryShift();
            //TODO
            _context.SurgeryShifts.Add(shift);
            _context.SaveChanges();
            return true;
        }
    }
}
