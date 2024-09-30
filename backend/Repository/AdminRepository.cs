using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Dtos;
using backend.Helpers;
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

        public async Task<Levy?> CreateStudentLevyAsync(Levy levy, string id)
        {
            var createlevy = new Levy{
                AppUserId = id,
                SemesterId = levy.SemesterId,
                Amount = levy.Amount,
                Name = levy.Name,
                ToBalance = levy.Amount
            };
            var student = await _userManager.FindByIdAsync(id);
            student.Balance += levy.Amount;


            await _context.Levies.AddAsync(createlevy);
            await _context.SaveChangesAsync();

            return createlevy;

        }

        public async Task<List<DefaultLeviesDto>?> DefaultingStudentAsync(DefaultStudentQuery query)
        {
            var defaultstudent = _context.Levies
            .Where(x=>x.ToBalance != 0)
            .Include(l=> l.AppUser)
            .Include(l=> l.Semester)
            .AsQueryable();
        
            if (!string.IsNullOrWhiteSpace(query.FilterOptions)){
                defaultstudent = defaultstudent.Where(x=>x.AppUser.FirstName.Contains(query.FilterOptions)
                || x.AppUser.LastName.Contains(query.FilterOptions)
                || x.AppUser.MatricNo.Contains(query.FilterOptions)
                || x.Semester.Name.Contains(query.FilterOptions)
                || x.Name.Contains(query.FilterOptions)
                );
            }
            var SkipNumber = (query.PageNumber - 1) * query.PageSize;
    
            return await defaultstudent
                .Select(x => x.ToDefaultLeviesDto())
                .Skip(SkipNumber)
                .Take(query.PageSize)
                .ToListAsync();

        }

        public async Task<ListDepartmentDto?> GetDepartmentAsync(int id, DepartmentQuery query)
        {
            var department = await _context.Departments
                .Include(d => d.Semesters)
                .Include(d => d.AppUsers)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
            {
                return null;
            }
            if (!string.IsNullOrEmpty(query.FilterOptions))
            {
                department.AppUsers = department.AppUsers
                    .Where(l => l.FirstName.Contains(query.FilterOptions)
                    || l.LastName.ToString().Contains(query.FilterOptions)
                    || l.MatricNo.ToString().Contains(query.FilterOptions)).ToList();
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

        public async Task<StudentDetailsDto?> GetStudentDetailsAsync(string id, LevyQuery query)
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
            if (!string.IsNullOrEmpty(query.FilterOptions))
            {
                student.Levies = student.Levies
                    .Where(l => l.Name.Contains(query.FilterOptions)
                    || l.ToBalance.ToString().Contains(query.FilterOptions)
                    || l.Amount.ToString().Contains(query.FilterOptions)).ToList();
                    
            }
            if (!string.IsNullOrEmpty(query.TransactionFilterOptions))
            {
                student.Transactions = student.Transactions
                    .Where(l => l.Id.ToString().Contains(query.TransactionFilterOptions)
                    || l.Amount.ToString().Contains(query.TransactionFilterOptions)
                    || l.Description.Contains(query.TransactionFilterOptions)
                    || l.Levy.Name.Contains(query.TransactionFilterOptions)
                    || l.TransID.Contains(query.TransactionFilterOptions)).ToList();
                    
            }
            if (!string.IsNullOrWhiteSpace(query.OrderOptions)){
                if (query.OrderOptions == "Amount"){
                    student.Transactions = student.Transactions.OrderByDescending(x=>x.Amount).ToList();
                }
                if (query.OrderOptions == "TranID"){
                    student.Transactions = student.Transactions.OrderByDescending(x=>x.Id).ToList();
                }
                if (query.OrderOptions == "Date"){
                    student.Transactions = student.Transactions.OrderByDescending(x=>x.CreatedAt).ToList();
                }
                
            }
            
            var studentDto= student.ToStudentDetailsDto();

            return studentDto;
        }

        public async Task<List<Department>> ListDepartmentAsync(DepartmentQuery query)
        {
            var departments =   _context.Departments.AsQueryable();
            if (!string.IsNullOrWhiteSpace(query.FilterOptions)){
                departments = departments.Where(x=>x.Name.Contains(query.FilterOptions)
                || x.ProgramType.Contains(query.FilterOptions)
                || x.AcademicYear.Contains(query.FilterOptions)
                );
            }
            return await departments.ToListAsync();
        }

        public async Task<List<StudentDto>> ListStudentAsync(StudentQuery query)
        {
            var students = _userManager.Users
            .Where(x=>x.IsStudent == true)
            .Include(l=> l.Department)
            .AsQueryable();
            if (!string.IsNullOrWhiteSpace(query.FilterOptions)){
                students = students.Where(x=>x.FirstName.Contains(query.FilterOptions)
                || x.LastName.Contains(query.FilterOptions)
                || x.MatricNo.Contains(query.FilterOptions)
                );
            }
            var studentDtos = await students
                .Select(x => x.ToStudentDto())
                .ToListAsync();
            var SkipNumber = (query.PageNumber - 1) * query.PageSize;
            return studentDtos.Skip(SkipNumber).Take(query.PageSize).ToList();
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


        public async Task<List<studentTransactionDto>?> GetAdminAllTransactions(TransactionQuery query)
        {
            var transactions = _context.Transactions
            .Include(l=>l.AppUser)
            .Include(c=>c.Levy)
            .OrderByDescending(t => t.CreatedAt)
            .AsQueryable();
            if (!string.IsNullOrWhiteSpace(query.FilterOptions)){
                transactions = transactions.Where(x=>x.TransID.Contains(query.FilterOptions)
                || x.Description.Contains(query.FilterOptions)
                || x.Levy.Name.Contains(query.FilterOptions)
                || x.Amount.ToString().Contains(query.FilterOptions)
                || x.Id.ToString().Contains(query.FilterOptions)
                || x.AppUser.FirstName.Contains(query.FilterOptions)
                || x.AppUser.LastName.Contains(query.FilterOptions)
                || x.AppUser.MatricNo.Contains(query.FilterOptions)
                );
            }
            if (!string.IsNullOrWhiteSpace(query.OrderOptions)){
                if (query.OrderOptions == "Amount"){
                    transactions = transactions.OrderByDescending(x=>x.Amount);
                }
                if (query.OrderOptions == "TranID"){
                    transactions = transactions.OrderByDescending(x=>x.Id);
                }
                if (query.OrderOptions == "Date"){
                    transactions = transactions.OrderByDescending(x=>x.CreatedAt);
                }
                
            }
            var transactionDto = transactions.Select(t => new studentTransactionDto
            {
                Id = t.Id,
                Amount = t.Amount,
                TransID = t.TransID,
                Method = t.Method,
                Description = t.Description,
                LevyName = t.Levy.Name,
                CreatedAt = t.CreatedAt,
                StudentFName=t.AppUser.FirstName,
                StudentLName = t.AppUser.LastName,
                StudentMatricNo = t.AppUser.MatricNo
            });
            var SkipNumber = (query.PageNumber - 1) * query.PageSize;
            return await transactionDto.Skip(SkipNumber).Take(query.PageSize).ToListAsync();
        
        }



    }
}