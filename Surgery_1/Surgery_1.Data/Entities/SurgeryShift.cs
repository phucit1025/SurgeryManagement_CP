using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Surgery_1.Data.Entities
{
    public class SurgeryShift : BaseEntity
    {
        public SurgeryShift()
        {
            SurgeryShiftMedicalSupplies = new HashSet<SurgeryShiftMedicalSupply>();
            HealthCareReports = new HashSet<HealthCareReport>();
        }

        public DateTime ActualStartDateTime { get; set; }
        public DateTime ActualEndDateTime { get; set; }
        public DateTime EstimatedStartDateTime { get; set; }
        public DateTime EstimatedEndDateTime { get; set; }
        public int PatientId { get; set; }
        public int SurgeonId { get; set; }
        public int RoomId { get; set; }
        public int SurgeryInformationId { get; set; }
        public int StatusId { get; set; }

        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; }
        [ForeignKey("SurgeonId")]
        public virtual Surgeon Surgeon { get; set; }
        [ForeignKey("RoomId")]
        public virtual SurgeryRoom SurgeryRoom { get; set; }
        [ForeignKey("SurgeryInformationId")]
        public virtual SurgeryInformation SurgeryInformation { get; set; }
        [ForeignKey("Status")]
        public virtual Status Status { get; set; }

        public virtual ICollection<SurgeryShiftMedicalSupply> SurgeryShiftMedicalSupplies { get; set; }
        public virtual ICollection<HealthCareReport> HealthCareReports { get; set; }
    }
}
