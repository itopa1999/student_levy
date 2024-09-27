using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace backend.models
{
    public class AppUser : IdentityUser
    {
        public string? MatricNo { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public decimal Balance { get; set; } = 0;
        public DateOnly CreatedAt { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        public bool IsAdmin { get; set; } = false;
        public bool IsStudent { get; set; } = false;
        
        // Foreign Key to Department (a user belongs to one department)
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }

        public ICollection<Levy> Levies { get; set; } = new List<Levy>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public int? OtpId { get; set; }
        public Otp? Otp { get; set; }
    }

    public class Otp
    {
        public int Id { get; set; }
        public int Token { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public string? AppUserId { get; set; }
        public AppUser? AppUser { get; set; }
    }
}