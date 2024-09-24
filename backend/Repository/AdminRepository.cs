using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Dtos;
using backend.Interfaces;
using backend.Mappers;
using backend.models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository
{
    public class AdminRepository : IAdminRepository
    {
        public readonly ApplicationDBContext _context;
        public readonly UserManager<AppUser> _userManager;
        public AdminRepository(
            ApplicationDBContext context,
            UserManager<AppUser> userManager
            )
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<Department> CreateDepartmentAsync(Department department)
        {
            // Add the department to the database
            await _context.Departments.AddAsync(department);
            
            // Save changes to ensure the DepartmentId is generated
            await _context.SaveChangesAsync();

            return department;
        }

        public async Task<ListDepartmentDto?> GetDepartmentAsync(int id)
        {
            var department = await _context.Departments
                .Include(d => d.Semesters)
                .Include(d => d.AppUsers)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
            {
                return null;
            }

            var departmentDto = department.ToDepartmentDto();

            return departmentDto;
        }

        public async Task<StudentDetailsDto?> GetStudentDetailsAsync(string id)
        {
            var student = await _userManager.Users
            .Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.Id == id);

            if (student == null)
            {
                return null;
            }
            
            var studentDto= student.ToStudentDetailsDto();

            return studentDto;
        }

        public async Task<List<Department>> ListDepartmentAsync()
        {
            return await _context.Departments.ToListAsync();
        }

        public async Task<List<StudentDto>> ListStudentAsync()
        {
            var students =  await _userManager.Users
            .Where(x=>x.IsStudent == true)
            .Select(x => x.ToStudentDto())
            .ToListAsync();

            return students;
        }

        public async Task<AppUser> StudentDepartmentCreateAsync(AppUser appUser, int DepartmentId)
        {
            appUser.DepartmentId = DepartmentId;
            await _context.SaveChangesAsync();
            return appUser;
        }

        public async Task<Department?> UpdateDepartmentAsync(int id, UpdateDepartmentDto updateDepartmentDto)
        {


            var checkedDepartment = await _context.Departments
                .Include(d => d.Semesters) 
                .FirstOrDefaultAsync(d => d.Id == id);

            if (checkedDepartment == null)
            {
                return null;
            }

            checkedDepartment.AcademicYear = updateDepartmentDto.AcademicYear;
            checkedDepartment.Name = updateDepartmentDto.Name;
            checkedDepartment.ProgramType = updateDepartmentDto.ProgramType;

            if (checkedDepartment.Semesters != null && checkedDepartment.Semesters.Any())
            {
                _context.Semesters.RemoveRange(checkedDepartment.Semesters);
            }

            var newSemesters = new List<Semester>
            {
                new Semester { Name = $"{checkedDepartment.Name} {checkedDepartment.ProgramType} 1 First Semester {checkedDepartment.AcademicYear}", DepartmentId = checkedDepartment.Id },
                new Semester { Name = $"{checkedDepartment.Name} {checkedDepartment.ProgramType} 1 Second Semester {checkedDepartment.AcademicYear}", DepartmentId = checkedDepartment.Id },
                new Semester { Name = $"{checkedDepartment.Name} {checkedDepartment.ProgramType} 2 First Semester {checkedDepartment.AcademicYear}", DepartmentId = checkedDepartment.Id },
                new Semester { Name = $"{checkedDepartment.Name} {checkedDepartment.ProgramType} 2 Second Semester {checkedDepartment.AcademicYear}", DepartmentId = checkedDepartment.Id }
            };

            await _context.Semesters.AddRangeAsync(newSemesters);

            await _context.SaveChangesAsync();

            return checkedDepartment;
        }

        public async Task<StudentDto?> UpdateStudentDetailsAsync(string id, UpdateStudentDetailsDto studentDetailsDto)
        {
            var student = await _userManager.FindByIdAsync(id);
            if (student == null){
                return null;
            }
            student.FirstName = studentDetailsDto.FirstName;
            student.LastName = studentDetailsDto.LastName;
            student.MatricNo = studentDetailsDto.MatricNo;
            student.UserName = studentDetailsDto.MatricNo;

            var result = await _userManager.UpdateAsync(student);

            if (result.Succeeded)
            {
                return student.ToStudentDto();
            }
            else
            {
                return null;
            }
        }
    }
}