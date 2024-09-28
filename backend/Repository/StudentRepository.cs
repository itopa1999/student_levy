using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Dtos;
using backend.Interfaces;
using backend.Mappers;
using backend.models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository
{
    public class StudentRepository : IStudentRepository
    {
        public readonly ApplicationDBContext _context;
        public readonly UserManager<AppUser> _userManager;
        public StudentRepository(
            ApplicationDBContext context,
            UserManager<AppUser> userManager
            )
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<StudentGetDepartmentDto?> GetClearanceAsync(int id, string appUserId)
        {
            var department = await _context.Departments
            .Include(d => d.Semesters)
            .Include(d => d.AppUsers)
            .ThenInclude(d => d.Levies)
            .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
            {
                return null;
            }
            var semestersWithBalances = department.Semesters.Select(semester =>
            {
                var studentLevies = semester.Levies
                    .Where(levy => levy.AppUserId == appUserId)
                    .ToList();
                var totalToBalance = studentLevies.Sum(levy => levy.ToBalance);

                // Map the semester and include the total balance
                return new StuSemesterDto
                {
                    SemesterId = semester.Id,
                    SemesterName = semester.Name,
                    TotalToBalance = totalToBalance, // Total balance for the semester
                    // Levies = studentLevies.Select(levy => new StuLevyDto
                    // {
                    //     LevyId = levy.Id,
                    //     Name = levy.Name,
                    //     Amount = levy.Amount,
                    //     ToBalance = levy.ToBalance,
                    //     CreatedAt = levy.CreatedAt
                    // }).ToList()
                };
            }).ToList();

            // Map the final department data into a DTO
            var departmentDto = new StudentGetDepartmentDto
            {
                DepartmentId = department.Id,
                ProgramType = department.ProgramType,
                AcademicYear = department.AcademicYear,
                DepartmentName = department.Name,
                Semesters = semestersWithBalances // Include the semesters with calculated balances
            };

            return departmentDto;

        }



        public async Task<StuGetDepartmentDto?> GetDepartmentAsync(int id, string appUserId)
        {
            var department = await _context.Departments
                .Include(d => d.Semesters)
                .Include(d => d.AppUsers)
                .ThenInclude(d => d.Levies)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
            {
                return null;
            }

            department.Semesters = department.Semesters.Select(semester =>
            {
                semester.Levies = semester.Levies
                    .Where(levy => levy.AppUserId == appUserId)
                    .ToList();
                return semester;
            }).ToList();

            return department.ToStuDepartmentDto();
        }
    }
}