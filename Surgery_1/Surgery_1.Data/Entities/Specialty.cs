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
        public int? SpecialtyGroupId { get; set; }

        [ForeignKey("SpecialtyGroupId")]
        public virtual SpecialtyGroup SpecialtyGroup { get; set; }

        public virtual ICollection<SurgeryCatalog> SurgeryCatalogs { get; set; }
    }
}
