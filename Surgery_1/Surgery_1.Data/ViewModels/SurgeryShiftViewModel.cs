using System;
using System.Collections.Generic;
using System.Text;
using Surgery_1.Data.Entities;

namespace Surgery_1.Data.ViewModels
{
    public class SurgeryShiftViewModel
    {
        public int Id { get; set; }
        public string SpecialtyName { get; set; }
        public string CatalogName { get; set; }
        public string CatalogCode { get; set; }
        public int? PriorityNumber { get; set; }
        public List<string> SurgeonNames { get; set; }
        public string PatientName { get; set; }
        public DateTime EstimatedStartDateTime { get; set; }
        public DateTime EstimatedEndDateTime { get; set; }
        public DateTime? ActualStartDateTime { get; set; }
        public DateTime? ActualEndDateTime { get; set; }
        public string StatusName { get; set; }
        public bool IsEmergency { get; set; }
    }


    public class SurgeryShiftDetailViewModel
    {
        public int Id { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public string PatientName { get; set; }
        public string Specialty { get; set; }
        public string SurgeryName { get; set; }
        public string SurgeryType { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? ActualStartTime { get; set; }
        public DateTime? ActualEndTime { get; set; }
        public string treatmentDoctorName { get; set; }
        public bool IsEmergency { get; set; } = false;
        public string StatusName { get; set; }

        //Surgery Info
        //public List<EkipMemberViewModel> EkipMembers { get; set; }
        public string Procedure { get; set; }
    }

    public class PostOpSurgeryShiftViewModel
    {
        public int Id { get; set; }
        public string CatalogName { get; set; }
        public string PatientName { get; set; }
        public string PostOpBed { get; set; }
        public int PatientGender { get; set; }
        public int PatientAge { get; set; }
    }

    //Import surgery profile view models
    public class ImportSurgeryShiftViewModel
    {
        public float ExpectedDuration { get; set; }
        public int PriorityNumber { get; set; }

        //Patient Info
        public string PatientID { get; set; }
        public string PatientName { get; set; }
        public int Gender { get; set; }
        public int YearOfBirth { get; set; }

        public int SurgeryCatalogID { get; set; }
        public String SurgeryShiftCode { get; set; }
        public int DoctorId { get; set; }
        public DateTime? ProposedStartDateTime { get; set; }
        public DateTime? ProposedEndDateTime { get; set; }

        public ICollection<ImportMedicalSupplyViewModel> DetailMedical { get; set; }
    }

    public class ImportMedicalSupplyViewModel
    {
        public int Code { get; set; }
        public String SurgeryShiftCode { get; set; }
        public int Quantity { get; set; }
    }

    public class ShiftScheduleChangeViewModel
    {
        public int Id { get; set; }
        public DateTime EstimatedStartDateTime { get; set; }
        public DateTime EstimatedEndDateTime { get; set; }
        public string ChangeLogDescription { get; set; }
        public int SlotRoomId { get; set; }
    }

    public class ShiftChangeViewModel
    {
        public int Id { get; set; }
        public string ChangeLogDescription { get; set; }
        public int NewPriority { get; set; }
    }

    public class SurgeryCatalogNamesViewModel
    {
        public string name { get; set; }
    }

    public class SurgeryCatalogIDsViewModel
    {
        public int id { get; set; }
    }
    public class ShiftStatusChangeViewModel
    {
        public int ShiftId { get; set; }
        public string CurrentStatus { get; set; }
    }

    public class SwapShiftResultViewModel
    {
        public string Message { get; set; }
        public bool Succeed { get; set; } = false;
        public List<AffectedShiftResultViewModel> AffectedShifts { get; set; } = new List<AffectedShiftResultViewModel>();
    }

    public class AffectedShiftViewModel
    {
        public int ShiftId { get; set; }
        public DateTime EstimatedStart { get; set; }
        public DateTime EstimatedEnd { get; set; }
    }

    public class AffectedShiftResultViewModel
    {
        public int ShiftId { get; set; }
        public DateTime OldStart { get; set; }
        public DateTime OldEnd { get; set; }
        public DateTime? NewStart { get; set; }
        public DateTime? NewEnd { get; set; }
        public string OldRoomName { get; set; }
        public string NewRoomName { get; set; }
    }

    public class SwapShiftViewModel
    {
        public int FirstShiftId { get; set; }
        public int SecondShiftId { get; set; }
    }


    public class SurgeryProcedureViewModel
    {
        public int SurgeryShiftId { get; set; }
        public string Procedure { get; set; }
    }

    public class SwapShiftToRoomViewModel
    {
        public int ShiftId { get; set; }
        public int RoomId { get; set; }
        public bool ForcedSwap { get; set; }
    }

    public class EmerSurgeryShift
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsForceAdd { get; set; }
        public bool IsEmergency { get; set; } = false;
        public int? SlotRoomId { get; set; }
    }

    public class ImportViewModel
    {
        public ICollection<ImportSurgeryShiftViewModel> surgeryShifts;
    }

    public class AssignSurgeryEkip
    {
        public int EkipId { get; set; }
        public float SumDuration { get; set; }
    }

    public class EditSurgeryShiftViewModel
    {
        public int ShiftId { get; set; }
        public string EditIdentityNumber { get; set; }
        public string EditPatientName { get; set; }
        public int? EditGender { get; set; }
        //public List<string> SurgeonNames { get; set; }
        public int? EditYob { get; set; }
        public int? EditSurgeryId { get; set; }
        public string SurgeryCode { get; set; }
        public string SurgeryName { get; set; }
    }

    public class SmsShiftViewModel
    {
        public int Id { get; set; }
        public DateTime EstimatedStartDateTime { get; set; }
        public DateTime EstimatedEndDateTime { get; set; }
        public int SlotRoomId { get; set; }
        public string SurgeonPhone { get; set; }
        public int TechnicalId { get; set; }
    }

    public class HealthcareSurgeryShiftViewModel
    {
        public int ShiftId { get; set; }
        public string PatientName { get; set; }
        public string SurgeryName { get; set; }
        public int WoundCondition { get; set; }
        public string WoundConditionDescription { get; set; }
        public int DrugAllergy { get; set; }
        public string DrugAllergyDescription { get; set; }
        public DateTime ClosestDate { get; set; }
    }

    public class PostOpViewModel
    {
        public int Id { get; set; }
        public string PatientName { get; set; }
        public int Gender { get; set; }
        public string Duration { get; set; }
        public string SurgeryName { get; set; }
        public string StatusName { get; set; }
        public string SurgeonName { get; set; }
    }
}
