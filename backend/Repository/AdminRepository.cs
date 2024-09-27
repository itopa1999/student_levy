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

        public async Task<Levy?> CreateLevyAsync(Levy levy)
        {
            var students = await _userManager.Users.Where(x => x.IsStudent == true && x.Department.Semesters.Any(s => s.Id == levy.SemesterId))
                            .ToListAsync();
            if (students.Count == 0){
                return null;
            }
            var studentLevies = new List<Levy>();
            foreach (var student in students)
            {
                var studentLevy = new Levy
                {
                    AppUserId = student.Id,
                    SemesterId = levy.SemesterId,
                    Amount = levy.Amount,
                    Name = levy.Name,
                    ToBalance = levy.Amount
                };
                
                studentLevies.Add(studentLevy);
                student.Balance += levy.Amount;

                await _userManager.UpdateAsync(student);
            }

            await _context.Levies.AddRangeAsync(studentLevies);
            await _context.SaveChangesAsync();

            return levy;

        }

        public async Task<Transaction> CreatePayStudentLevyAsync(Transaction transaction)
        {
            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
            return transaction;
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

        public async Task<GetSemesterDetailsDto?> GetSemesterDetailsAsync(int id)
        {
            var semester = await _context.Semesters
            .Include(l => l.Levies)
            .Include(l => l.Department)
            .FirstOrDefaultAsync(x=>x.Id == id);
            if (semester == null)
            {
                return null;
            }
            var semesterDto = new GetSemesterDetailsDto
            {
                Id = semester.Id,
                Name = semester.Name,
                DepartmentName = semester.Department?.Name,
                // Use Distinct to filter out Levies with the same Name
                Levies = semester.Levies
                    .GroupBy(l => l.Name) // Group by the name
                    .Select(g => g.First()) // Select the first levy from each group
                    .Select(levy => new SemesterLevyDto
                    {
                        Id = levy.Id,
                        Name = levy.Name,
                        Amount = levy.Amount,
                        CreatedAt = levy.CreatedAt,
                    })
                    .ToList() // Convert to List<SemesterLevyDto>
            };
            return semesterDto;
        }

        public async Task<StudentDetailsDto?> GetStudentDetailsAsync(string id)
        {
            var student = await _userManager.Users
            .Include(x => x.Department)
            .Include(x=> x.Transactions)
            .Include(x=> x.Levies)
            .ThenInclude(l => l.Semester)
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
            .Include(l=> l.Department)
            .Select(x => x.ToStudentDto())
            .ToListAsync();

            return students;
        }

        public async Task<AppUser> StudentDepartmentCreateAsync(AppUser appUser, int DepartmentId)
        {
            var existingDepartment = await _context.Departments.FindAsync(DepartmentId);
            if (existingDepartment == null){
                
            }
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