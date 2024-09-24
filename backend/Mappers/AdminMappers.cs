using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Dtos;
using backend.models;

namespace backend.Mappers
{
    public static class AdminMappers
    {
        public static Department ToCreateDepartmentDto(this CreateDepartmentDto departmentDto){
            return new Department{
                Name = departmentDto.Name,
                AcademicYear = departmentDto.AcademicYear,
                ProgramType = departmentDto.ProgramType
            };
        }


        public static SemesterDto ToSemesterDto(this Semester semester){
            return new SemesterDto{
                Id = semester.Id,
                Name = semester.Name,
            };
        }

        public static StudentDto ToStudentDto(this AppUser appUser){
            return new StudentDto{
                Id = appUser.Id,
                FirstName = appUser.FirstName,
                LastName = appUser.LastName,
                Username = appUser.UserName,
                MatricNo = appUser.MatricNo,
                CreatedAt = appUser.CreatedAt
                
            };
        }


        public static ListDepartmentDto ToDepartmentDto(this Department department){
            return new ListDepartmentDto{
                Id = department.Id,
                Name = department.Name,
                AcademicYear = department.AcademicYear,
                ProgramType = department.ProgramType,
                Semesters = department.Semesters.Select(x=>x.ToSemesterDto()).ToList(),
                Students = department.AppUsers.Select(x=>x.ToStudentDto()).ToList()
                
            };
        }


        public static StudentDetailsDto ToStudentDetailsDto(this AppUser appUser){
            return new StudentDetailsDto{
                Id = appUser.Id,
                FirstName = appUser.FirstName,
                LastName = appUser.LastName,
                Username = appUser.UserName,
                MatricNo = appUser.MatricNo,
                CreatedAt = appUser.CreatedAt,
                DepartmentId = appUser.DepartmentId,
                DepartmentName = appUser.Department?.Name
                
            };
        }


    }
}