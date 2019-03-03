using System;
using System.Collections.Generic;
using System.Text;
using Surgery_1.Data.Entities;

namespace Surgery_1.Data.ViewModels
{
    public class SurgeryShiftViewModel
    {
        public int Id { get; set; }
        public string CatalogName { get; set; }
        public int PriorityNumber { get; set; }
        public List<string> SurgeonNames { get; set; }
        public string PatientName { get; set; }
        public DateTime EstimatedStartDateTime { get; set; }
        public DateTime EstimatedEndDateTime { get; set; }
    }


    public class SurgeryShiftDetailViewModel
    {
        public int Id { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public string PatientName { get; set; }
        public string Speciality { get; set; }
        public string SurgeryName { get; set; }
        public string SurgeryType { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }

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
        public float ExpectedSurgeryDuration { get; set; }
        public int PriorityNumber { get; set; }

        //Patient Info
        public string PatientID { get; set; }
        public string PatientName { get; set; }
        public int Gender { get; set; }
        public int YearOfBirth { get; set; }

        public int SurgeryCatalogID { get; set; }
        public String SurgeryShiftCode { get; set; }
        public int SurgeonId { get; set; }
        public DateTime? ProposedStartDateTime { get; set; }
        public DateTime? ProposedEndDateTime { get; set; }

    }

    public class ImportMedicalSupplyViewModel
    {
        public int MedicalSupplyId { get; set; }
        public String SurgeryShiftCode { get; set; }
    }

    public class ShiftScheduleChangeViewModel
    {
        public int Id { get; set; }
        public DateTime EstimatedStartDateTime { get; set; }
        public DateTime EstimatedEndDateTime { get; set; }
        public string ChangeLogDescription { get; set; }
        public int RoomId { get; set; }
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
    public class ShiftStatusChangeViewModel
    {
        public int ShiftId { get; set; }
        public string CurrentStatus { get; set; }
    }
}
