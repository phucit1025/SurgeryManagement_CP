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
        public int surgeryShiftId { get; set; }
        public int oldSurgeonId { get; set; }
        public int updatedSurgeonId { get; set; }
    }
}
