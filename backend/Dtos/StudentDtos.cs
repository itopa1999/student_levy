using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Dtos
{
    public class StuLeviesDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Amount { get; set; }
        public decimal ToBalance { get; set; }
        public string? SemesterName { get; set; }
        public int SemesterId { get; set; }
        public DateOnly CreatedAt { get; set; }
    }


    public class StudentSemesterDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public List<StuLeviesDto>? Levies { get; set; }
    }


    public class StuGetDepartmentDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? AcademicYear { get; set; }
        public string? ProgramType { get; set; }
        public List<StudentSemesterDto>? Semesters {get; set;} = new List<StudentSemesterDto>();

    }
}