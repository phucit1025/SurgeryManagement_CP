using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Surgery_1.Data.Entities
{
    public class SurgeryCatalog : BaseEntity
    {
        public SurgeryCatalog()
        {
            PatientSurgeryCatalogs = new HashSet<PatientSurgeryCatalog>();
        }

        public string Code { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Note { get; set; }
        public string Price { get; set; }
        public string Procedure { get; set; }
        public int ExpectedSurgeryDuration { get; set; }
        public string AnesthesiaMethod { get; set; }
        public int SpecialityId { get; set; }

        [ForeignKey("SpecialityId")]
        public virtual Speciality Speciality { get; set; }
        public virtual ICollection<PatientSurgeryCatalog> PatientSurgeryCatalogs { get; set; }
    }
}
