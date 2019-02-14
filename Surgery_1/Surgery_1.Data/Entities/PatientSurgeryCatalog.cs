using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Surgery_1.Data.Entities
{
    public class PatientSurgeryCatalog : BaseEntity
    {
        public int PatientId { get; set; }
        public int SurgeryCatalogId { get; set; }

        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; }
        [ForeignKey("SurgeryCatalogId")]
        public virtual SurgeryCatalog SurgeryCatalog { get; set; }
    }
}
