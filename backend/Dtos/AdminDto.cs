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
        public string? DepartmentName { get; set; }
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


    public class StudentLeviesDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Amount { get; set; }
        public decimal ToBalance { get; set; }
        public string? SemesterName { get; set; }
        public int SemesterId { get; set; }
        public DateOnly CreatedAt { get; set; }
    }

    public class StudentDetailsDto
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
        public List<StudentLeviesDto>? Levies {get; set;} = new List<StudentLeviesDto>();
        public List<studentTransactionDto>? Transactions {get; set;} = new List<studentTransactionDto>();
    }

    public class GetDepartmentDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class CreateLevyDto
    {
        [Required]
        public string? Name { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public int SemesterId { get; set; }
    }


    public class SemesterLevyDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Amount { get; set; }
        public DateOnly CreatedAt { get; set; }
    }
    

    public class GetSemesterDetailsDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? DepartmentName { get; set; }
        public List<SemesterLevyDto>? Levies {get; set;} = new List<SemesterLevyDto>();
        
        
    }

    public class PayStudentLevyDto
    {
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public string? Method { get; set; }
        [Required]
        public string? Description { get; set; }
        [Required]
        public string? AppUserId { get; set; }
        [Required]
        public int LevyId { get; set; }
       
    }

    public class studentTransactionDto
    {
        public int Id { get; set; }
        public Decimal Amount { get; set; }
        public string? Method { get; set; }
        public string? Description { get; set; }
        public string? TransID { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsCompleted { get; set; }
        public string? LevyName { get; set; }
        public string? StudentFName { get; set; }
        public string? StudentID { get; set; }
        public string? StudentLName { get; set; }
        public string? StudentMatricNo { get; set; }
    }


    public class DefaultLeviesDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Amount { get; set; }
        public decimal ToBalance { get; set; }
        public string? SemesterName { get; set; }
        public string? StudentFName { get; set; }
        public string? StudentLName { get; set; }
        public DateOnly CreatedAt { get; set; }
    }



    public class GetAddLevySemesterDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }


    public class GetAddLevyDepartmentDto
    {
        public string? Name { get; set; }
        public List<GetAddLevySemesterDto>? Semesters {get; set;} = new List<GetAddLevySemesterDto>();

    }
    

}