using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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



    public class StudentGetDepartmentDto
    {
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? AcademicYear { get; set; }
        public string? ProgramType { get; set; }
        public List<StuSemesterDto> Semesters { get; set; } = new List<StuSemesterDto>();
    }

    public class StuSemesterDto
    {
        public int SemesterId { get; set; }
        public string? SemesterName { get; set; }
        public decimal TotalToBalance { get; set; }
        // public List<StuLevyDto> Levies { get; set; } = new List<StuLevyDto>();
    }

    public class StuLevyDto
    {
        public int LevyId { get; set; }
        public string? Name { get; set; }
        public decimal Amount { get; set; }
        public decimal ToBalance { get; set; }
        public DateOnly CreatedAt { get; set; }
    }


    public class PayLevyDto
    {
        [Required]
        public decimal Amount { get; set; }
        
       
    }


    public class StuDetailsDto
    {
        public string? Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Username { get; set; }
        public string? MatricNo { get; set; }
        public decimal Balance { get; set; }
        public DateOnly CreatedAt { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public string? DepartmentYear { get; set; }
        public string? DepartmentType { get; set; }
    }


    public class FlutterwaveResponseDto
    {
        public string? Status { get; set; }
        public string? Message { get; set; }
        public Data? Data { get; set; }

    }

    public class Data
    {
        public string? Id { get; set; }
        public string? Status { get; set; }
        public string? user_id { get; set; }
    
    }


}