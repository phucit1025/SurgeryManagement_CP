using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
        public int? SpecialityGroupId { get; set; }

        [ForeignKey("SpecialityGroupId")]
        public virtual SpecialityGroup SpecialityGroup { get; set; }

        public virtual ICollection<SurgeryCatalog> SurgeryCatalogs { get; set; }
    }
}
