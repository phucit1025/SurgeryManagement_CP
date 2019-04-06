using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Surgery_1.Data.Entities
{
    public class Specialty : BaseEntity
    {
        public Specialty()
        {
            SurgeryCatalogs = new HashSet<SurgeryCatalog>();
        }

        public string Name { get; set; }

        [ForeignKey("SpecialityGroupId")]
        public virtual SpecialtyGroup SpecialityGroup { get; set; }

        public virtual ICollection<SurgeryCatalog> SurgeryCatalogs { get; set; }
    }
}
