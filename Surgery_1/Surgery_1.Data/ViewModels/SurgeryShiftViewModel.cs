﻿using System;
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
        public DateTime? ActualStartDateTime { get; set; }
        public DateTime? ActualEndDateTime { get; set; }
        public string StatusName { get; set; }
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
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

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
        public int DoctorId { get; set; }
        public DateTime? ProposedStartDateTime { get; set; }
        public DateTime? ProposedEndDateTime { get; set; }

    }

    public class ImportMedicalSupplyViewModel
    {
        public int MedicalSupplyId { get; set; }
        public String SurgeryShiftCode { get; set; }
        public int Quantity { get; set; }
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
    }
    public class ShiftStatusChangeViewModel
    {
        public int ShiftId { get; set; }
        public string CurrentStatus { get; set; }
    }

    public class SwapShiftResultViewModel
    {
        public bool Succeed { get; set; } = false;
        public List<AffectedShiftResultViewModel> AffectedShifts { get; set; } = new List<AffectedShiftResultViewModel>();
    }

    public class AffectedShiftResultViewModel
    {
        public int ShiftId { get; set; }
        public DateTime OldStart { get; set; }
        public DateTime OldEnd { get; set; }
        public DateTime NewStart { get; set; }
        public DateTime NewEnd { get; set; }
        public string OldRoomName { get; set; }
        public string NewRoomName { get; set; }
    }

    public class SwapShiftViewModel
    {
        public int FirstShiftId { get; set; }
        public int SecondShiftId { get; set; }
    }
}
