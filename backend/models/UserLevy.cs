using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.models
{
    


    public class Department
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? AcademicYear { get; set; } // 2021/2022
        public string? ProgramType { get; set; } // ND or HND


        // Navigation property for users in this department
        public ICollection<AppUser> AppUsers { get; set; } = new List<AppUser>();

        // One Department has many Semesters
        public ICollection<Semester> Semesters { get; set; } = new List<Semester>();

    }

    public class Semester
    {
        public int Id { get; set; }
        public string? Name { get; set; } // ND1 First Semester ...... base of department programType
        

        // Foreign Key to Department
        public int DepartmentId { get; set; }
        public Department? Department { get; set; }
        public ICollection<Levy> Levies { get; set; } = new List<Levy>();
        public ICollection<Clearance> Clearances { get; set; } = new List<Clearance>();
    }

    public class Levy
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Amount { get; set; }
        public decimal ToBalance { get; set; } = 0;
        public DateOnly CreatedAt { get; set; } = DateOnly.FromDateTime(DateTime.Now);


        public string? AppUserId { get; set; }
        public AppUser? AppUser { get; set; }

        public int SemesterId { get; set; }
        public Semester? Semester { get; set; }

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }

    public class Clearance
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsValid { get; set; }
        public string? PdfFilePath { get; set; }
        public string? AppUserId { get; set; }
        public AppUser? AppUser { get; set; }

        public int SemesterId { get; set; }
        public Semester? Semester { get; set; }
    }

    public class Transaction
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string? TransID { get; set; } = GenerateUUID();
        public string? Method { get; set; }
        public string? Description { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? AppUserId { get; set; }
        public AppUser? AppUser { get; set; }

        public int LevyId { get; set; }
        public Levy? Levy { get; set; }

        private static string GenerateUUID()
        {
            return Guid.NewGuid().ToString();
        }
    }


    


}