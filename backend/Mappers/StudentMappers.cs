using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Dtos;
using backend.models;

namespace backend.Mappers
{
    public static class StudentMappers
    {
        public static StuLeviesDto ToStuLeviesDto(this Levy levy){
            return new StuLeviesDto{
                Id = levy.Id,
                Name = levy.Name,
                Amount = levy.Amount,
                ToBalance = levy.ToBalance,
                SemesterId = levy.SemesterId,
                SemesterName = levy.Semester?.Name,
                CreatedAt = levy.CreatedAt
                
            };
        }


        public static StudentSemesterDto ToStuSemesterDto(this Semester semester){
            return new StudentSemesterDto{
                Id = semester.Id,
                Name = semester.Name,
                Levies = semester.Levies.Select(x=>x.ToStuLeviesDto()).ToList(),
            };
        }


        public static StuGetDepartmentDto ToStuDepartmentDto(this Department department){
            return new StuGetDepartmentDto{
                Id = department.Id,
                Name = department.Name,
                AcademicYear = department.AcademicYear,
                ProgramType = department.ProgramType,
                Semesters = department.Semesters.Select(x=>x.ToStuSemesterDto()).ToList(),
                
            };

        }
    }
}