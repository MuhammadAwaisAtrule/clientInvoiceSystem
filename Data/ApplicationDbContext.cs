using Client_Invoice_System.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Client_Invoice_System.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<OwnerProfile> Owners { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<ClientProfileCrossTable> ClientProfileCrosses { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<PaymentProfile> PaymentProfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
          

            // ✅ Owner & Payment Profile Relationship (Fixes Orphaned PaymentProfiles)
            modelBuilder.Entity<OwnerProfile>()
                .HasOne(o => o.PaymentProfile)
                .WithOne(p => p.Owner)
                .HasForeignKey<PaymentProfile>(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);  // ✅ Ensures PaymentProfile is deleted when Owner is deleted

            // ✅ Client & Resources Relationship (Fixes Orphaned Resources)
            modelBuilder.Entity<Client>()
                .HasMany(c => c.Resources)
                .WithOne(r => r.Client)
                .HasForeignKey(r => r.ClientId)
                .OnDelete(DeleteBehavior.Cascade);  // ✅ Ensures Resources are deleted when Client is deleted

            // ✅ Employee & Resources Relationship (Fixes Orphaned Resources)
            modelBuilder.Entity<Employee>()
                .HasMany(e => e.Resources)
                .WithOne(r => r.Employee)
                .HasForeignKey(r => r.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);  // ✅ Ensures Resources are deleted when Employee is deleted

            // ✅ Client Profile Cross Table (Many-to-Many)
            modelBuilder.Entity<ClientProfileCrossTable>()
                .HasKey(cpc => new { cpc.ClientId, cpc.EmployeeId });

            modelBuilder.Entity<ClientProfileCrossTable>()
                .HasOne(cpc => cpc.Client)
                .WithMany(c => c.ClientProfileCrosses)
                .HasForeignKey(cpc => cpc.ClientId)
                .OnDelete(DeleteBehavior.Cascade); // ✅ Ensures Cross Table records are deleted

            modelBuilder.Entity<ClientProfileCrossTable>()
                .HasOne(cpc => cpc.Employee)
                .WithMany()
                .HasForeignKey(cpc => cpc.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade); // ✅ Ensures Employee-related Cross Table records are deleted
        }


    }
}

