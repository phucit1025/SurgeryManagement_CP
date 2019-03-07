using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
            SurgeryShiftSurgeons = new HashSet<SurgeryShiftSurgeon>();
        }

        public DateTime? ActualStartDateTime { get; set; }
        public DateTime? ActualEndDateTime { get; set; }
        public DateTime? EstimatedStartDateTime { get; set; }
        public DateTime? EstimatedEndDateTime { get; set; }
        //Thời gian kiến nghị của bác sĩ
        public DateTime? ProposedStartDateTime { get; set; }
        public DateTime? ProposedEndDateTime { get; set; }

        public bool IsNormalSurgeryTime { get; set; } = true;

        public string SurgeryShiftCode { get; set; }
        public DateTime? ConfirmDate { get; set; }
        public DateTime? ScheduleDate { get; set; }

        public string PostRoomName { get; set; }
        public string PostBedName { get; set; }

        //Thời gian hoàn thành ca mổ dự kiến
        public float ExpectedSurgeryDuration { get; set; }

        public string UsedProcedure { get; set; }
        public string UsedAnesthesiaMethod { get; set; }

        public bool IsAvailableMedicalSupplies { get; set; }
        public string SurgeryProcedureContent { get; set; }
        public int PriorityNumber { get; set; }
        public int? PriorityNumberAlt { get; set; }

        public int? PatientId { get; set; }
        public int? SurgeryRoomId { get; set; }
        public int? SurgeryCatalogId { get; set; }
        public int? StatusId { get; set; }
        public int? EkipId { get; set; }

        public string EkipIndex { get; set; }

        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; }
        [ForeignKey("SurgeryRoomId")]
        public virtual SurgeryRoom SurgeryRoom { get; set; }
        [ForeignKey("SurgeryCatalogId")]
        public virtual SurgeryCatalog SurgeryCatalog { get; set; }
        [ForeignKey("StatusId")]
        public virtual Status Status { get; set; }
        [ForeignKey("EkipId")]
        public virtual Ekip Ekip { get; set; }
        [ForeignKey("TreatmentDoctorId")]
        public virtual Doctor TreatmentDoctor { get; set; }

        public virtual ICollection<SurgeryShiftMedicalSupply> SurgeryShiftMedicalSupplies { get; set; }
        public virtual ICollection<HealthCareReport> HealthCareReports { get; set; }
        public virtual ICollection<TreatmentReport> TreatmentReports { get; set; }
        public virtual ICollection<SurgeryShiftSurgeon> SurgeryShiftSurgeons { get; set; }
    }
}
