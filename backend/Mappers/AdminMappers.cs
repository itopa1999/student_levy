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
                DepartmentName = appUser.Department?.Name,
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


        public static StudentLeviesDto ToStudentLeviesDto(this Levy levy){
            return new StudentLeviesDto{
                Id = levy.Id,
                Name = levy.Name,
                Amount = levy.Amount,
                ToBalance = levy.ToBalance,
                SemesterId = levy.SemesterId,
                SemesterName = levy.Semester?.Name,
                CreatedAt = levy.CreatedAt
                
            };
        }


        public static studentTransactionDto ToStudentTransactionDto(this Transaction transaction){
            return new studentTransactionDto{
                Id = transaction.Id,
                Amount = transaction.Amount,
                Method = transaction.Method,
                Description = transaction.Description,
                TransID = transaction.TransID,
                LevyName = transaction.Levy?.Name,
                CreatedAt = transaction.CreatedAt,
                IsCompleted = transaction.IsCompleted
                
            };
        }




        public static StudentDetailsDto ToStudentDetailsDto(this AppUser appUser){
            return new StudentDetailsDto{
                Id = appUser.Id,
                FirstName = appUser.FirstName,
                LastName = appUser.LastName,
                Username = appUser.UserName,
                MatricNo = appUser.MatricNo,
                Balance = appUser.Balance,
                CreatedAt = appUser.CreatedAt,
                DepartmentId = appUser.DepartmentId,
                DepartmentName = appUser.Department?.Name,
                Levies = appUser.Levies.Select(x=>x.ToStudentLeviesDto()).ToList(),
                Transactions = appUser.Transactions.Select(x=>x.ToStudentTransactionDto()).ToList(),
                
            };
        }


        public static GetDepartmentDto ToGetDepartmentDto(this Department department){
            return new GetDepartmentDto{
                Id = department.Id,
                Name = department.Name
            };
        }

        public static Levy ToCreateLevyDto(this CreateLevyDto levyDto){
            return new Levy{
                Name = levyDto.Name,
                Amount = levyDto.Amount,
                SemesterId = levyDto.SemesterId
            };
        }


        public static SemesterLevyDto ToSemesterLevyDto(this Levy levy){
            return new SemesterLevyDto{
                Id = levy.Id,
                Amount = levy.Amount,
                CreatedAt = levy.CreatedAt,
                Name = levy.Name
            };
        }



        public static GetSemesterDetailsDto ToGetSemesterDetails(this Semester semester){
            return new GetSemesterDetailsDto{
                Id = semester.Id,
                Name = semester.Name,
                DepartmentName = semester.Department?.Name,
                Levies = semester.Levies.Select(x=>x.ToSemesterLevyDto()).ToList(),
            };
        }

        public static Transaction ToCreatePayStudentLevyDto(this PayStudentLevyDto payDto){
            return new Transaction{
                Amount = payDto.Amount,
                Description = payDto.Description,
                AppUserId = payDto.AppUserId,
                LevyId = payDto.LevyId,
                Method = payDto.Method
            };
        }


        public static DefaultLeviesDto ToDefaultLeviesDto(this Levy levy){
            return new DefaultLeviesDto{
                Id = levy.Id,
                Name = levy.Name,
                Amount = levy.Amount,
                ToBalance = levy.ToBalance,
                SemesterName = levy.Semester?.Name,
                StudentFName = levy.AppUser?.FirstName,
                StudentLName = levy.AppUser?.LastName,
                CreatedAt = levy.CreatedAt

            };
        }


    }
}