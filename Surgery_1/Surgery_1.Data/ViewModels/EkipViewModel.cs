using System;
namespace Surgery_1.Data.ViewModels
{
    public class EkipMemberViewModel
    {
        public string Name { get; set; }
        public string WorkJob { get; set; }
    }

    public class SurgeonsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class UpdateSurgeonsViewModel
    {
        public int SurgeryShiftId { get; set; }
        public int OldSurgeonId { get; set; }
        public int UpdatedSurgeonId { get; set; }
    }

    public class AddSurgeonToShiftViewModel
    {
        public int SurgeryShiftId { get; set; }
        public int SurgeonId { get; set; }
    }

    public class RemoveSurgeonFromShiftViewModel
    {
        public int SurgeryShiftId { get; set; }
        public int SurgeonId { get; set; }
    }
}
