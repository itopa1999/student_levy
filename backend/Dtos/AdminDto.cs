using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using backend.models;

namespace backend.Dtos
{
    public class CreateDepartmentDto
    {
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? AcademicYear { get; set; } // 2021/2022
        [Required]
        public string? ProgramType { get; set; } // ND or HND

    }

    public class SemesterDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    } 

    public class StudentDto
    {
        public string? Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Username { get; set; }
        public string? MatricNo { get; set; }
        public DateOnly CreatedAt { get; set; }
    } 

    public class ListDepartmentDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? AcademicYear { get; set; }
        public string? ProgramType { get; set; }
        public List<SemesterDto>? Semesters {get; set;} = new List<SemesterDto>();
        public List<StudentDto>? Students {get; set;} = new List<StudentDto>();

    }

    public class UpdateDepartmentDto
    {
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? AcademicYear { get; set; }
        [Required]
        public string? ProgramType { get; set; }
    }

    public class CreateStudentDto
    {
        [Required]
        public string? FirstName { get; set; }
        [Required]
        public string? LastName { get; set; }
        [Required]
        public string? MatricNo { get; set; }
        [Required]
        public int DepartmentId { get; set; }
    }

    public class UpdateStudentDetailsDto
    {
        [Required]
        public string? FirstName { get; set; }
        [Required]
        public string? LastName { get; set; }
        [Required]
        public string? MatricNo { get; set; }
    }

    public class StudentDetailsDto
    {
        public string? Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Username { get; set; }
        public string? MatricNo { get; set; }
        public DateOnly CreatedAt { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
    }
    

}