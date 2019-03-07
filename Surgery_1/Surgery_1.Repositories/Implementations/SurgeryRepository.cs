using Surgery_1.Data.Context;
using Surgery_1.Data.Entities;
using Surgery_1.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Surgery_1.Repositories.Implementations
{
    public class SurgeryRepository : ISurgeryRepository
    {
        private readonly AppDbContext _context;

        public SurgeryRepository(AppDbContext context)
        {
            _context = context;
        }

        public Doctor GetSurgeon()
        {
            throw new NotImplementedException();
        }
    }
}
