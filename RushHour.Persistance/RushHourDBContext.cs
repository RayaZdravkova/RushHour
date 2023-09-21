using Microsoft.EntityFrameworkCore;
using RushHour.Domain.Enums;
using RushHour.Persistance.Entities;

namespace RushHour.Persistance
{
    public class RushHourDBContext : DbContext
    {
        public RushHourDBContext(DbContextOptions<RushHourDBContext> options)
            :base(options) 
        {
        }

        public DbSet<Account> Accounts { get; set; }    
        public DbSet<Activity> Activities { get; set; }    
        public DbSet<Appointment> Appointments { get; set; }    
        public DbSet<Client> Clients { get; set; }    
        public DbSet<Employee> Employees { get; set; }    
        public DbSet<Provider> Providers { get; set; }    
        public DbSet<ActivityEmployee> ActivityEmployees { get; set; }    

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Provider>()
               .HasIndex(p => p.Name)
               .IsUnique();

            modelBuilder.Entity<Provider>()
               .HasIndex(p => p.BusinessDomain)
               .IsUnique();

            modelBuilder.Entity<Account>()
               .HasIndex(a => a.Email)
               .IsUnique();

            modelBuilder.Entity<Client>()
               .HasOne(c => c.Account);

            modelBuilder.Entity<Employee>()
               .HasOne(e => e.Account);

            modelBuilder.Entity<Employee>()
               .HasOne(e => e.Provider);

            modelBuilder.Entity<Activity>()
               .HasOne(a => a.Provider);

            modelBuilder.Entity<Appointment>()
               .HasOne(a => a.Employee)
               .WithMany()
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
               .HasOne(a => a.Client);

            modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Activity);

            modelBuilder.Entity<ActivityEmployee>()
            .HasOne(a => a.Employee)
            .WithMany()
            .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ActivityEmployee>()
            .HasOne(a => a.Activity);

            modelBuilder.Entity<ActivityEmployee>()
             .HasKey(pc => new { pc.ActivityId, pc.EmployeeId });

            modelBuilder.Entity<Account>().HasData(new Account { Id = 1, Email = "admin@prime.com", FullName = "Administrator", Username = "admin", Password = "703883388914CE4175C73C7ED3EBF62E0C3D44515F90CB6A06F32110E0443EE942EB9A0ED19495111501A2D4B16A9A7F1F6192A3B6B858C94309C12600F7D79C",
                Salt = new byte[]{64, 215, 132, 195, 127, 34, 219, 158, 179, 138, 120, 204, 209, 204, 255, 137, 254, 167, 60, 72, 91, 226, 66, 110, 131, 126, 79, 222, 229, 43, 172, 64, 219, 19, 250, 167, 204, 158, 209, 124, 233, 20, 42, 108, 13, 126, 136, 109, 175, 46, 237, 17, 192, 122, 155, 218, 204, 187, 62, 151, 237, 205, 102, 3},
                Role = Roles.ADMIN 
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
