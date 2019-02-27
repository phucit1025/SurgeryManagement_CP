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
        public string EstimatedStartDateTime { get; set; }
        public string EstimatedEndDateTime { get; set; }
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
        public List<EkipMemberViewModel> EkipMembers { get; set; }
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
    
    //Import surgery profile view models
    public class ImportSurgeryShirftViewModel
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
        public int SurgoenId { get; set; }
        public DateTime? ProposedStartDateTime { get; set; }
        public DateTime? ProposedEndDateTime { get; set; }

    }

    public class ImportMedicalSupplyViewModel
    {
        public int MedicalSupplyId { get; set; }
        public String SurgeryShiftCode { get; set; }
    }
}
