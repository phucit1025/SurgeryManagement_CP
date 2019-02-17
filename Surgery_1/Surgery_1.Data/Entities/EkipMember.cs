using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Surgery_1.Data.Entities
{
    public class EkipMember : BaseEntity
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string WorkJob { get; set; }
        public int EkipId { get; set; }

        [ForeignKey("EkipId")]
        public virtual Ekip Ekip { get; set; }
    }
}
