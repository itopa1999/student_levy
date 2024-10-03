using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Transactions;
using backend.Data;
using backend.Dtos;
using backend.Helpers;
using backend.Interfaces;
using backend.Mappers;
using backend.models;
using CsvHelper;
using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Route("admin/api")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        public readonly ApplicationDBContext _context;
        public readonly UserManager<AppUser> _userManager;
        public readonly IAdminRepository _adminRepo;
        public readonly IUserRepository _userRepo;
        public AdminController(
            ApplicationDBContext context,
            UserManager<AppUser> userManager,
            IAdminRepository adminRepo,
            IUserRepository userRepo
        )
        {
            _context = context;
            _userManager = userManager;
            _adminRepo = adminRepo;
            _userRepo = userRepo;
        }

        [HttpPost("create/department/")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentDto departmentDto){
            if (!ModelState.IsValid){
                return StatusCode(400, new{message=ModelState});
            }
            bool isDuplicate = await _context.Departments
                .AnyAsync(d => d.Name == departmentDto.Name && 
                            d.AcademicYear == departmentDto.AcademicYear &&
                            d.ProgramType == departmentDto.ProgramType);

            if (isDuplicate)
            {
                return StatusCode(400, new { message = "A department with the same details already exists." });
            }
            var department = departmentDto.ToCreateDepartmentDto();
            var createdDepartment =await _adminRepo.CreateDepartmentAsync(department);
            List<Semester> semesters = new List<Semester>
            {
                new Semester { Name = $"{createdDepartment.Name} {createdDepartment.ProgramType} 1 First Semester {createdDepartment.AcademicYear}", DepartmentId = createdDepartment.Id },
                new Semester { Name = $"{createdDepartment.Name} {createdDepartment.ProgramType} 1 Second Semester {createdDepartment.AcademicYear}", DepartmentId = createdDepartment.Id },
                new Semester { Name = $"{createdDepartment.Name} {createdDepartment.ProgramType} 2 First Semester {createdDepartment.AcademicYear}", DepartmentId = createdDepartment.Id },
                new Semester { Name = $"{createdDepartment.Name} {createdDepartment.ProgramType} 2 Second Semester {createdDepartment.AcademicYear}", DepartmentId = createdDepartment.Id }
            };

            // Add and save semesters
            await _context.Semesters.AddRangeAsync(semesters);
            await _context.SaveChangesAsync();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);
            var action = $"Created a Department: {department.Name}";
            var name = $"{user.FirstName} {user.LastName}";
            await _userRepo.CreateAuditAsync(name, action);
            return StatusCode(201, new {message="Department created.",value = createdDepartment.ToDepartmentDto()});
        
        }

        [HttpGet("list/department/")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> ListDepartment([FromQuery] DepartmentQuery query){
            var departments = await _adminRepo.ListDepartmentAsync(query);
            return StatusCode(200, departments);
             
        }

        [HttpGet("get/department/details/{id:int}/")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> GetDepartmentDetails([FromRoute] int id, [FromQuery] DepartmentQuery query){
            var departments = await _adminRepo.GetDepartmentAsync(id, query);
            if (departments == null){
                return StatusCode(400, new{message = "Department not found"});
            }
            return StatusCode(200, departments);
             
        }

        [HttpPut("update/department/details/{id:int}/")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> UpdateDepartmentDetails([FromRoute] int id, [FromBody] UpdateDepartmentDto updateDepartmentDto){
            if (!ModelState.IsValid){
                return StatusCode(400, new{message=ModelState});
            }
            var existingRecord = await _context.Departments.FindAsync(id);
            if (existingRecord == null)
            {
                return StatusCode(404, new { message = "Record not found." });
            }
            bool isDuplicate = await _context.Departments
                .AnyAsync(r => r.Name == updateDepartmentDto.Name &&
                            r.AcademicYear == updateDepartmentDto.AcademicYear &&
                            r.ProgramType == updateDepartmentDto.ProgramType);

            if (isDuplicate)
            {
                return StatusCode(400, new { message = "An identical record already exists." });
            }
            var departments = await _adminRepo.UpdateDepartmentAsync(id, updateDepartmentDto);
            if (departments == null){
                return StatusCode(400, new{message = "Department not found"});
            
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);
            var action = $"Updated a Department: {departments.Name}";
            var name = $"{user.FirstName} {user.LastName}";
            await _userRepo.CreateAuditAsync(name, action);
            return StatusCode(200, departments);
             
        }

        [HttpPost("create/student/")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> CreateStudent([FromBody] CreateStudentDto createStudentDto){
            if (!ModelState.IsValid){
                return StatusCode(400, new{message=ModelState});
            }
            var isdepartment = await _context.Departments.FindAsync(createStudentDto.DepartmentId);
            if (isdepartment == null){
                return StatusCode(400, new{message= "Department not found"});
            }
            try{
                var user = new AppUser{
                    FirstName = createStudentDto.FirstName,
                    LastName = createStudentDto.LastName,
                    UserName = createStudentDto.MatricNo,
                    MatricNo = createStudentDto.MatricNo,
                    IsAdmin = false,
                    IsStudent=true,
                    CreatedAt = DateOnly.FromDateTime(DateTime.Now)
                };
                var password = "Pass1234";
                var userModel = await _userManager.CreateAsync(user, password);
                if (userModel.Succeeded){
                    await _adminRepo.StudentDepartmentCreateAsync(user, createStudentDto.DepartmentId);
                    var role = await _userManager.AddToRoleAsync(user, "Student");
                    if (role.Succeeded){
                        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                        var admin = await _userManager.FindByIdAsync(userId);
                        var action = $"Added a Student: {user.FirstName} {user.LastName}";
                        var name = $"{admin.FirstName} {admin.LastName}";
                        await _userRepo.CreateAuditAsync(name, action);
                    return StatusCode(201, new {message = $"Student Account Successfully Created for {createStudentDto.FirstName}"});
                }else{return StatusCode(500, new {message = role.Errors});}
                
              }else{return StatusCode(500, new {message = userModel.Errors});}
                

            }catch(Exception e){return StatusCode(400, new{message= e});}

        }

    [HttpPost("upload/students/")]
    [Authorize]
    [Authorize(Policy = "IsAdmin")]
    public async Task<IActionResult> UploadStudent(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return StatusCode(400, new{message="No file uploaded."});

        List<CreateStudentDto> students = new List<CreateStudentDto>();
        var extension = Path.GetExtension(file.FileName).ToLower();
        int departmentId = 1; // Replace with the actual department ID you want to assign
        int successfulImports = 0;
        int duplicateCount = 0;

        try
        {
            if (extension == ".csv")
            {
                using (var reader = new StreamReader(file.OpenReadStream()))
                using (var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)))
                {
                    var records = csv.GetRecords<CreateStudentDto>();
                    students.AddRange(records.Select(record => new CreateStudentDto
                    {
                        FirstName = record.FirstName?.Trim(),
                        LastName = record.LastName?.Trim(),
                        MatricNo = record.MatricNo?.Trim()
                    }));
                }
            }
            else if (extension == ".xlsx")
            {
                using (var stream = file.OpenReadStream())
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        while (reader.Read())
                        {
                            var student = new CreateStudentDto
                            {
                                FirstName = reader.GetValue(0)?.ToString().Trim(),
                                LastName = reader.GetValue(1)?.ToString().Trim(),
                                MatricNo = reader.GetValue(2)?.ToString().Trim()
                            };
                            students.Add(student);
                        }
                    }
                }
            }
            else
            {
                return StatusCode(400, new{message="Unsupported file format. Please upload a CSV or Excel file."});
            }

            foreach (var userDto in students)
            {
                // Check if a user with the same MatricNo already exists
                var existingUser = await _userManager.FindByNameAsync(userDto.MatricNo);
                if (existingUser != null)
                {
                    duplicateCount++;
                    continue; // Skip this entry if a duplicate is found
                }

                var appUser = new AppUser
                {
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    MatricNo = userDto.MatricNo,
                    UserName = userDto.MatricNo,
                    DepartmentId = departmentId, // Assigning the department ID
                    IsAdmin = false,
                    IsStudent = true,
                };

                var password = "Pass1234"; // Consider generating a random password for security
                var studentModel = await _userManager.CreateAsync(appUser, password);
                if (studentModel.Succeeded)
                {
                    await _userManager.AddToRoleAsync(appUser, "Student");
                    successfulImports++;
                }
            }

            return StatusCode(200, new 
            {
                Message = $"{successfulImports} out of {students.Count} students imported successfully.",
                Duplicates = duplicateCount
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }




        [HttpGet("get/students/details/{id}/")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> GetStudentDetails(string id, [FromQuery] LevyQuery query){
            var student = await _adminRepo.GetStudentDetailsAsync(id, query);
            if (student == null){
                return StatusCode(400, new{message = "student not found"});
            
            }
            return StatusCode(200, student);
        }

        [HttpGet("list/students/")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> ListStudents([FromQuery] StudentQuery query){
            var students = await _adminRepo.ListStudentAsync(query);
            if (students == null){
                return StatusCode(204, new{message="No Content Found" });
            }
            return StatusCode(200, students);
        }



        [HttpPut("update/student/details/{id}/")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> UpdateStudentDetails([FromRoute] string id, [FromBody] UpdateStudentDetailsDto studentDetailsDto){
            if (!ModelState.IsValid){
                return StatusCode(400, new{message=ModelState});
            }
            var student = await _adminRepo.UpdateStudentDetailsAsync(id, studentDetailsDto);
            if (student == null)
            {
                return StatusCode(400, new { message = "Student matricNo already exists" });
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var admin = await _userManager.FindByIdAsync(userId);
            var action = $"Updated a Student: {student.FirstName} {student.LastName}";
            var name = $"{admin.FirstName} {admin.LastName}";
            await _userRepo.CreateAuditAsync(name, action);
            return StatusCode(200, student);
        }

        [HttpGet("admin/dashboard/")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> Dashboard(){
            var t_student = await _userManager.Users.Where(x=>x.IsStudent == true).CountAsync();
            var t_department = await _context.Departments.CountAsync();
            var transactions = await _context.Transactions
            .Include(l=>l.AppUser)
            .Include(c=>c.Levy)
            .OrderByDescending(t => t.CreatedAt)
            .Take(10)
            .Select(t => new studentTransactionDto
            {
                Id = t.Id,
                Amount = t.Amount,
                TransID = t.TransID,
                Method = t.Method,
                Description = t.Description,
                Payer = t.Payer,
                StudentFName = t.AppUser.FirstName,
                StudentLName = t.AppUser.LastName,
                LevyName = t.Levy.Name,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();
            
            return StatusCode(200, new{
                t_department = t_department,
                t_student = t_student,
                transactions = transactions
            });
        }

        [HttpGet("get/department/")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        
        public async Task<IActionResult> GetDetails(){
            var departments = await _context.Departments.ToListAsync();
            if (departments == null || !departments.Any()){
                return StatusCode(204, new{message="No Content Found" });
            }
            
            var departmentDtos = departments.Select(d => d.ToGetDepartmentDto()).ToList();
            return StatusCode(200, departmentDtos);
        }


        [HttpPost("change/student/password/{id}/")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> ChangePassword([FromBody] AdminStudentChangePasswordDto passwordDto, [FromRoute] string id){
            if (!ModelState.IsValid){
                return StatusCode(400, new{message=ModelState});
            }
            if (passwordDto.Password1 != passwordDto.Password2){
                return StatusCode(400, new{message="password mismatch"});
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return StatusCode(400, new { message = "User not found" });
            }
            string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            IdentityResult result = await _userManager.ResetPasswordAsync(user, resetToken, passwordDto.Password1);
            if (result.Succeeded){
                Console.WriteLine($"new password: {passwordDto.Password1}");
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var admin = await _userManager.FindByIdAsync(userId);
                var action = $"Updated a password for Student: {user.FirstName} {user.LastName}";
                var name = $"{admin.FirstName} {admin.LastName}";
                await _userRepo.CreateAuditAsync(name, action);
                return StatusCode(200, new{message=$"your password has been reset, here is your new password {passwordDto.Password1}"});
            }
            return StatusCode(400, new { message = result.Errors });
        }


        [HttpPost("create/levy")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> CreateLevy([FromBody] CreateLevyDto levyDto){
            if (!ModelState.IsValid){
                return StatusCode(400, new{message=ModelState});
            }
            if (levyDto.Amount <= 0 ){
                return StatusCode(400, new{message="Amount must be greater than 0"});
            }
            var checkedSemester = await _context.Semesters.FindAsync(levyDto.SemesterId);
            if (checkedSemester == null){
                return StatusCode(400, new{message=$"semester not found for this iD: {levyDto.SemesterId}"});
            }
            var existingLevy = await _context.Levies.FirstOrDefaultAsync(x=>x.Name == levyDto.Name && x.SemesterId == levyDto.SemesterId);
            if (existingLevy == null){
                var levy = levyDto.ToCreateLevyDto();
                var createdLevy = await _adminRepo.CreateLevyAsync(levy);
                if (createdLevy == null){
                    return StatusCode(400, new{message="No student to assign levies for semester"});
                }
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var admin = await _userManager.FindByIdAsync(userId);
                var action = $"Add levy {createdLevy.Name}";
                var name = $"{admin.FirstName} {admin.LastName}";
                await _userRepo.CreateAuditAsync(name, action);
                return StatusCode(200, new {message=$"{levyDto.Name} successfully added"});

            }return StatusCode(400, new{message=$"levy already exists for this Name: {levyDto.Name}"});
        }

        [HttpGet("get/levy/details/{id:int}")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> GetSemesterDetails([FromRoute] int id){
            var checkSemester = await _context.Semesters.FindAsync(id);
            if (checkSemester == null){
                return StatusCode(400, new{message="Semester not found"});
            }
            var semesterDetails = await _adminRepo.GetSemesterDetailsAsync(id);
            if (semesterDetails == null){
                return StatusCode(400, new{message="Semester not found"});
            }
            return StatusCode(200, semesterDetails);

        }

        [HttpPost("pay/student/levy")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> PayStudentLevy([FromBody] PayStudentLevyDto payDto){
            if (!ModelState.IsValid){
                return StatusCode(400, new{message=ModelState});
            }
            if (payDto.Amount <= 0 ){
                return StatusCode(400, new{message="Amount must be greater than 0"});
            }
            var student = await _userManager.FindByIdAsync(payDto.AppUserId);
            if (student == null ){
                return StatusCode(400, new{message="Student not found"});
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return StatusCode(401,  new{message="Authorized Access"});
            }
            var admin = await _userManager.FindByIdAsync(userId);
            var levy = await _context.Levies.FindAsync(payDto.LevyId);
            if (levy == null ){
                return StatusCode(400, new{message="Levy not found"});
            }
            if (levy.ToBalance == 0 ){
                return StatusCode(400, new{message="This Levy has been paid completely"});
            }
            if (levy.ToBalance < payDto.Amount ){
                return StatusCode(400, new{message="Please make sure amount is no greater than toBalance"});
            }
            if (student.Balance == 0){
                return StatusCode(400, new{message="Student has no levies to pay"});
            }
            var payment = payDto.ToCreatePayStudentLevyDto();
            var paid = await _adminRepo.CreatePayStudentLevyAsync(payment);
            if (paid == null){
               return StatusCode(400, new{message="Student has no levies to pay"}); 
            }
            paid.Payer = $"{admin.FirstName} {admin.LastName}";
            await _context.SaveChangesAsync();
            student.Balance -= paid.Amount;
            await _userManager.UpdateAsync(student);

            levy.ToBalance -= paid.Amount;
            await _context.SaveChangesAsync();

            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var admins = await _userManager.FindByIdAsync(adminId);
            var action = $"Paid levy {paid.Levy?.Name} for Student: {paid.AppUser?.FirstName} {paid.AppUser?.LastName}";
            var name = $"{admins.FirstName} {admins.LastName}";
            await _userRepo.CreateAuditAsync(name, action);

            return StatusCode(200, new{message=$"Payment has been successfully toBalance: {levy.ToBalance}"});

        }


        [HttpGet("defaulting/students")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> DefaultStudent([FromQuery] DefaultStudentQuery query) {
            var defaultstudent = await _adminRepo.DefaultingStudentAsync(query);

            var totalDefault = await _context.Levies
            .Where(x => x.ToBalance != 0)
            .SumAsync(x => x.ToBalance);

            return StatusCode(200, new{
                totalDefault=totalDefault,
                defaulting = defaultstudent
            });
        }

        [HttpGet("get/student/semester/{id}")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> GetStudentSemester([FromRoute] string id){
            var student = await _userManager.FindByIdAsync(id);
            if (student == null ){
                return StatusCode(400, new{message="Student not found"});
            }
            var semester =  _context.Semesters.Where(x=>x.DepartmentId == student.DepartmentId);
            var semesterDto = semester.Select(d => d.ToGetAddSemesterDto()).ToList();

            return StatusCode(200, semesterDto);
        }

        
        [HttpPost("add/levy/student/{id}")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> AddLevyStudent([FromRoute] string id, [FromBody] CreateLevyDto createLevyDto){
            if (!ModelState.IsValid){
                return StatusCode(400, new{message=ModelState});
            }
            if (createLevyDto.Amount <= 0 ){
                return StatusCode(400, new{message="Amount must be greater than 0"});
            }
            var checkedSemester = await _context.Semesters.FindAsync(createLevyDto.SemesterId);
            if (checkedSemester == null){
                return StatusCode(400, new{message=$"semester not found for this iD: {createLevyDto.SemesterId}"});
            }
            var student = await _userManager.FindByIdAsync(id);
            if (student == null ){
                return StatusCode(400, new{message="Student not found"});
            }
            var existingLevy = await _context.Levies.FirstOrDefaultAsync(x=>x.Name == createLevyDto.Name && x.SemesterId == createLevyDto.SemesterId);
            if (existingLevy == null){
                var levy = createLevyDto.ToCreateLevyDto();
                var createdLevy = await _adminRepo.CreateStudentLevyAsync(levy, id);
                if (createdLevy == null){
                    return StatusCode(400, new{message="No student to assign levies for semester"});
                }
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var admin = await _userManager.FindByIdAsync(userId);
                var action = $"Add levy {createdLevy.Name} for student {student.FirstName} {student.LastName}";
                var name = $"{admin.FirstName} {admin.LastName}";
                await _userRepo.CreateAuditAsync(name, action);
                return StatusCode(201, new {message=$"{createLevyDto.Name} successfully added for {student.FirstName}"});

            }return StatusCode(400, new{message=$"levy already exists for this Name: {createLevyDto.Name}"});
        }

        [HttpGet("list/transactions/")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> ListTransactions([FromQuery] TransactionQuery query){
            var transactions = await _adminRepo.GetAdminAllTransactions(query);
            var totalToBal = await _context.Levies
            .Where(x => x.ToBalance != 0)
            .SumAsync(x => x.ToBalance);

            var totalBilling = await _context.Levies
            .SumAsync(x => x.Amount);

            var totalPay = await _context.Transactions
            .SumAsync(x => x.Amount);

            return StatusCode(200, new{
                totalToBal=totalToBal,
                totalBilling=totalBilling,
                totalPay=totalPay,
                transactions = transactions
            });
        }


        [HttpGet("admin/profile/details")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> AdminDetails(){
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return StatusCode(401,  new{message="Authorized Access"});
            }
            var admin = await _userManager.FindByIdAsync(userId);
            if (admin == null)
            {
                return StatusCode(401,  new{message="Authorized Access"});
            }

            var profile = await _adminRepo.GetAdminDetailsAsync(userId);
            if (profile == null)
            {
                return StatusCode(400, new{message="No record found"});
            }
            return StatusCode(200, profile);

        }

        [HttpGet("get/audit")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> GetAudit([FromQuery] AuditQuery query){
            var audit = await _adminRepo.ListAuditAsync(query);
            if(audit == null){
                return StatusCode(400, new{message="No record found"});
            }
            return StatusCode(200, audit);

        }


        [HttpPost("download/students")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> DownloadStudentsCsv()
        {

            var students = await _userManager.Users
                .Where(x => x.IsStudent)
                .Include(x=>x.Department)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            var studentDtos = students.Select(AdminMappers.ToDownloadStudentDto).ToList();

            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream, new System.Text.UTF8Encoding(), leaveOpen: true);
            var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteRecords(studentDtos);
            await writer.FlushAsync();
            memoryStream.Position = 0;

            var result = new FileStreamResult(memoryStream, "text/csv")
            {
                FileDownloadName = "students.csv"
            };

            csv.Dispose();
            writer.Dispose();

            return result;
        }



        [HttpPost("download/transactions")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> DownloadTransactionsCsv(){
            var transactions = _context.Transactions
            .Include(l=>l.AppUser)
            .Include(c=>c.Levy)
            .OrderByDescending(t => t.CreatedAt);

            var transactionDto = transactions.Select(t => new studentTransactionDto
            {
                Id = t.Id,
                Amount = t.Amount,
                TransID = t.TransID,
                Method = t.Method,
                Description = t.Description,
                Payer = t.Payer,
                LevyName = t.Levy.Name,
                CreatedAt = t.CreatedAt,
                StudentFName=t.AppUser.FirstName,
                StudentLName = t.AppUser.LastName,
                StudentMatricNo = t.AppUser.MatricNo
            }).ToList();

            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream, new System.Text.UTF8Encoding(), leaveOpen: true);
            var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteRecords(transactionDto);
            await writer.FlushAsync();
            memoryStream.Position = 0;

            var result = new FileStreamResult(memoryStream, "text/csv")
            {
                FileDownloadName = "transactions.csv"
            };

            csv.Dispose();
            writer.Dispose();

            return result;
 
        }


        [HttpPost("download/default/students")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> DownloadDefaultStudentsCsv(){
        var defaultstudent = _context.Levies
            .Where(x=>x.ToBalance != 0)
            .Include(l=> l.AppUser)
            .Include(l=> l.Semester)
            .OrderByDescending(t => t.Id);

        var defaultstudentDto = defaultstudent.Select(x => x.ToDefaultLeviesDto());

        var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream, new System.Text.UTF8Encoding(), leaveOpen: true);
            var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteRecords(defaultstudentDto);
            await writer.FlushAsync();
            memoryStream.Position = 0;

            var result = new FileStreamResult(memoryStream, "text/csv")
            {
                FileDownloadName = "defaultStudents.csv"
            };

            csv.Dispose();
            writer.Dispose();

            return result;

        }


        [HttpPost("download/audits")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> DownloadAuditCsv(){
            var audit = _context.Audits.OrderByDescending(t => t.CreatedAt);
            var auditDto = audit.Select(x => x.ToAuditDo());
            
            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream, new System.Text.UTF8Encoding(), leaveOpen: true);
            var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteRecords(auditDto);
            await writer.FlushAsync();
            memoryStream.Position = 0;

            var result = new FileStreamResult(memoryStream, "text/csv")
            {
                FileDownloadName = "audit.csv"
            };

            csv.Dispose();
            writer.Dispose();

            return result;

        }


        [HttpPost("download/student/transactions/{id}")]
        // [Authorize]
        // [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> DownloadStudentTransactionsCsv([FromRoute] string id){
            var student = await _userManager.FindByIdAsync(id);
            if (student == null){
                return StatusCode(400, new{message="Student not found"});
            }
            var transactions = _context.Transactions
            .Where(x=>x.AppUserId==student.Id)
            .Include(l=>l.AppUser)
            .Include(c=>c.Levy)
            .OrderByDescending(t => t.CreatedAt);

            var transactionDto = transactions.Select(t=>t.ToStudentTransactionDto());

            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream, new System.Text.UTF8Encoding(), leaveOpen: true);
            var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteRecords(transactionDto);
            await writer.FlushAsync();
            memoryStream.Position = 0;

            var result = new FileStreamResult(memoryStream, "text/csv")
            {
                FileDownloadName = "studentTransaction.csv"
            };

            csv.Dispose();
            writer.Dispose();

            return result;



        }






    }

}