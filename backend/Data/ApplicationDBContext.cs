using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
    public class ApplicationDBContext : IdentityDbContext<AppUser>
    {
        public ApplicationDBContext(DbContextOptions dbContextOptions)
        : base (dbContextOptions)
        {
            
        } 

        public DbSet<Otp> Otps {get; set;}
        public DbSet<Department> Departments {get; set;}
        public DbSet<Semester> Semesters {get; set;}
        public DbSet<Levy> Levies {get; set;}
        public DbSet<Audit> Audits {get; set;}
        public DbSet<Transaction> Transactions {get; set;}

        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUser>()
                .HasOne(a => a.Otp)
                .WithOne(o => o.AppUser)
                .HasForeignKey<Otp>(o => o.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-many relationship between Department and AppUser (a user belongs to one department)
            modelBuilder.Entity<AppUser>()
            .HasOne(u => u.Department)
            .WithMany(d => d.AppUsers)
            .HasForeignKey(u => u.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);  // Restrict delete if department is deleted

            // One-to-many relationship between Department and Semester
            modelBuilder.Entity<Semester>()
                .HasOne(s => s.Department)
                .WithMany(d => d.Semesters)
                .HasForeignKey(s => s.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);




            List<IdentityRole> roles = new List<IdentityRole>{
                new IdentityRole{
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole{
                    Name = "Student",
                    NormalizedName = "STUDENT"
                }
            };
            modelBuilder.Entity<IdentityRole>().HasData(roles);

            
        }

        }
    }
