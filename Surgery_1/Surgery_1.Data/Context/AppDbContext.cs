using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Surgery_1.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surgery_1.Data.Context
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext() : base((new DbContextOptionsBuilder())
            .UseLazyLoadingProxies()
            .UseSqlServer(@"Data Source=HOANH-TUNG\SQLEXPRESS;Initial Catalog=Surgery_CP_App;persist security info=True;Integrated Security=False;TrustServerCertificate=False;uid=sa;password=password;Trusted_Connection=False;MultipleActiveResultSets=true;")
            .Options)
        {

        }

        #region Database Tables
        public DbSet<HealthCareReport> HealthCareReports { get; set; }
        public DbSet<TreatmentReport> TreatmentReports { get; set; }
        public DbSet<MedicalSupply> MedicalSupplies { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Specialty> Specialties { get; set; }
        public DbSet<Status> Statuses { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<SurgeryCatalog> SurgeryCatalogs { get; set; }
        public DbSet<SurgeryRoom> SurgeryRooms { get; set; }
        public DbSet<SlotRoom> SlotRooms { get; set; }
        public DbSet<SurgeryShift> SurgeryShifts { get; set; }
        public DbSet<SurgeryShiftMedicalSupply> SurgeryShiftMedicalSupplies { get; set; }
        public DbSet<Ekip> Ekips { get; set; }
        public DbSet<EkipMember> EkipMembers { get; set; }
        public DbSet<SurgeryShiftSurgeon> SurgeryShiftSurgeons { get; set; }
        public DbSet<Drug> Drugs { get; set; }
        public DbSet<TreatmentReportDrug> TreatmentReportDrugs { get; set; }
        public DbSet<UserInfo> UserInfo { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<SpecialtyGroup> SpecialtyGroups { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder builder)
        {

            base.OnModelCreating(builder);
        }
    }
}
