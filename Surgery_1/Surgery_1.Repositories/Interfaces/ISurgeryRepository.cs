using Surgery_1.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Repositories.Interfaces
{
    public interface ISurgeryRepository: IBaseSurgeryRepository
    {
        Doctor GetSurgeon();
    }
}
