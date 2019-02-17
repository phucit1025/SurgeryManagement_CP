using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Data.Entities
{
    public class Speciality : BaseEntity
    {
        public Speciality()
        {
            SurgeryCatalogs = new HashSet<SurgeryCatalog>();
        }

        public string Name { get; set; }

        public virtual ICollection<SurgeryCatalog> SurgeryCatalogs { get; set; }
    }
}
