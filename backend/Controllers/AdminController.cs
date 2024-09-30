using System;
using System.Collections.Generic;
using System.Linq;
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
        public AdminController(
            ApplicationDBContext context,
            UserManager<AppUser> userManager,
            IAdminRepository adminRepo
        )
        {
            _context = context;
            _userManager = userManager;
            _adminRepo = adminRepo;
        }

        [HttpPost("create/department/")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentDto departmentDto){
            if (!ModelState.IsValid){
                return BadRequest(ModelState);
            }
            bool isDuplicate = await _context.Departments
                .AnyAsync(d => d.Name == departmentDto.Name && 
                            d.AcademicYear == departmentDto.AcademicYear &&
                            d.ProgramType == departmentDto.ProgramType);

            if (isDuplicate)
            {
                return BadRequest(new { message = "A department with the same details already exists." });
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
            return StatusCode(201, new {message="Department created.",value = createdDepartment.ToDepartmentDto()});
        
        }

        [HttpGet("list/department/")]
        // [Authorize]
        // [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> ListDepartment([FromQuery] DepartmentQuery query){
            var departments = await _adminRepo.ListDepartmentAsync(query);
            return Ok(departments);
             
        }

        [HttpGet("get/department/details/{id:int}/")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> GetDepartmentDetails([FromRoute] int id, [FromQuery] DepartmentQuery query){
            var departments = await _adminRepo.GetDepartmentAsync(id, query);
            if (departments == null){
                return BadRequest(new{message = "Department not found"});
            }
            return Ok(departments);
             
        }

        [HttpPut("update/department/details/{id:int}/")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> UpdateDepartmentDetails([FromRoute] int id, [FromBody] UpdateDepartmentDto updateDepartmentDto){
            if (!ModelState.IsValid){
                return BadRequest(ModelState);
            }
            var existingRecord = await _context.Departments.FindAsync(id);
            if (existingRecord == null)
            {
                return NotFound(new { message = "Record not found." });
            }
            bool isDuplicate = await _context.Departments
                .AnyAsync(r => r.Name == updateDepartmentDto.Name &&
                            r.AcademicYear == updateDepartmentDto.AcademicYear &&
                            r.ProgramType == updateDepartmentDto.ProgramType);

            if (isDuplicate)
            {
                return BadRequest(new { message = "An identical record already exists." });
            }
            var departments = await _adminRepo.UpdateDepartmentAsync(id, updateDepartmentDto);
            if (departments == null){
                return BadRequest(new{message = "Department not found"});
            
            }
            return Ok(departments);
             
        }

        [HttpPost("create/student/")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> CreateStudent([FromBody] CreateStudentDto createStudentDto){
            if (!ModelState.IsValid){
                return BadRequest(ModelState);
            }
            var isdepartment = await _context.Departments.FindAsync(createStudentDto.DepartmentId);
            if (isdepartment == null){
                return BadRequest(new{message= "Department not found"});
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
            return BadRequest("No file uploaded.");

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
                return BadRequest("Unsupported file format. Please upload a CSV or Excel file.");
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

            return Ok(new 
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
                return BadRequest(new{message = "student not found"});
            
            }
            return Ok(student);
        }

        [HttpGet("list/students/")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> ListStudents([FromQuery] StudentQuery query){
            var students = await _adminRepo.ListStudentAsync(query);
            if (students == null){
                return NoContent();
            }
            return Ok(students);
        }



        [HttpPut("update/student/details/{id}/")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> UpdateStudentDetails([FromRoute] string id, [FromBody] UpdateStudentDetailsDto studentDetailsDto){
            if (!ModelState.IsValid){
                return BadRequest(ModelState);
            }
            var student = await _adminRepo.UpdateStudentDetailsAsync(id, studentDetailsDto);
            if (student == null)
            {
                return BadRequest(new { message = "Student matricNo already exists" });
            }
            return Ok(student);
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
        [Authorize(Policy = "IsAdmin")]
        
        public async Task<IActionResult> GetDetails(){
            var departments = await _context.Departments.ToListAsync();
            if (departments == null || !departments.Any()){
                return NoContent();
            }
            
            var departmentDtos = departments.Select(d => d.ToGetDepartmentDto()).ToList();
            return Ok(departmentDtos);
        }


        [HttpPost("change/student/password/{id}/")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> ChangePassword([FromBody] AdminStudentChangePasswordDto passwordDto, [FromRoute] string id){
            if (!ModelState.IsValid){
                return BadRequest(ModelState);
            }
            if (passwordDto.Password1 != passwordDto.Password2){
                return StatusCode(400, new{message="password mismatch"});
            }
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return BadRequest(new { message = "User not found" });
            }
            string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            IdentityResult result = await _userManager.ResetPasswordAsync(user, resetToken, passwordDto.Password1);
            if (result.Succeeded){
                Console.WriteLine($"new password: {passwordDto.Password1}");
                return StatusCode(200, new{message=$"your password has been reset, here is your new password {passwordDto.Password1}"});
            }
            return BadRequest(new { message = result.Errors });
        }


        [HttpPost("create/levy")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> CreateLevy([FromBody] CreateLevyDto levyDto){
            if (!ModelState.IsValid){
                return BadRequest(ModelState);
            }
            if (levyDto.Amount <= 0 ){
                return BadRequest(new{message="Amount must be greater than 0"});
            }
            var checkedSemester = await _context.Semesters.FindAsync(levyDto.SemesterId);
            if (checkedSemester == null){
                return BadRequest(new{message=$"semester not found for this iD: {levyDto.SemesterId}"});
            }
            var existingLevy = await _context.Levies.FirstOrDefaultAsync(x=>x.Name == levyDto.Name && x.SemesterId == levyDto.SemesterId);
            if (existingLevy == null){
                var levy = levyDto.ToCreateLevyDto();
                var createdLevy = await _adminRepo.CreateLevyAsync(levy);
                if (createdLevy == null){
                    return BadRequest(new{message="No student to assign levies for semester"});
                }
                return Ok(new {message=$"{levyDto.Name} successfully added"});

            }return BadRequest(new{message=$"levy already exists for this Name: {levyDto.Name}"});
        }

        [HttpGet("get/levy/details/{id:int}")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> GetSemesterDetails([FromRoute] int id){
            var checkSemester = await _context.Semesters.FindAsync(id);
            if (checkSemester == null){
                return BadRequest(new{message="Semester not found"});
            }
            var semesterDetails = await _adminRepo.GetSemesterDetailsAsync(id);
            if (semesterDetails == null){
                return BadRequest(new{message="Semester not found"});
            }
            return Ok(semesterDetails);

        }

        [HttpPost("pay/student/levy")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> PayStudentLevy([FromBody] PayStudentLevyDto payDto){
            if (!ModelState.IsValid){
                return BadRequest(ModelState);
            }
            if (payDto.Amount <= 0 ){
                return BadRequest(new{message="Amount must be greater than 0"});
            }
            var student = await _userManager.FindByIdAsync(payDto.AppUserId);
            if (student == null ){
                return BadRequest(new{message="Student not found"});
            }
            var levy = await _context.Levies.FindAsync(payDto.LevyId);
            if (levy == null ){
                return BadRequest(new{message="Levy not found"});
            }
            if (levy.ToBalance == 0 ){
                return BadRequest(new{message="This Levy has been paid completely"});
            }
            if (levy.ToBalance < payDto.Amount ){
                return BadRequest(new{message="Please make sure amount is no greater than toBalance"});
            }
            if (student.Balance == 0){
                return BadRequest(new{message="Student has no levies to pay"});
            }
            var payment = payDto.ToCreatePayStudentLevyDto();
            var paid = await _adminRepo.CreatePayStudentLevyAsync(payment);
            if (paid == null){
               return BadRequest(new{message="Student has no levies to pay"}); 
            }
            student.Balance -= paid.Amount;
            await _userManager.UpdateAsync(student);

            levy.ToBalance -= paid.Amount;
            await _context.SaveChangesAsync();

            return Ok(new{message=$"Payment has been successfully toBalance: {levy.ToBalance}"});

        }


        [HttpGet("defaulting/students")]
        // [Authorize]
        // [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> DefaultStudent([FromQuery] DefaultStudentQuery query) {
            var defaultstudent = await _adminRepo.DefaultingStudentAsync(query);

            var totalDefault = await _context.Levies
            .Where(x => x.ToBalance != 0)
            .SumAsync(x => x.ToBalance);

            return Ok(new{
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
                return BadRequest(new{message="Student not found"});
            }
            var semester =  _context.Semesters.Where(x=>x.DepartmentId == student.DepartmentId);
            var semesterDto = semester.Select(d => d.ToGetAddSemesterDto()).ToList();

            return Ok(semesterDto);
        }

        
        [HttpPost("add/levy/student/{id}")]
        [Authorize]
        [Authorize(Policy = "IsAdmin")]
        public async Task<IActionResult> AddLevyStudent([FromRoute] string id, [FromBody] CreateLevyDto createLevyDto){
            if (!ModelState.IsValid){
                return BadRequest(ModelState);
            }
            if (createLevyDto.Amount <= 0 ){
                return BadRequest(new{message="Amount must be greater than 0"});
            }
            var checkedSemester = await _context.Semesters.FindAsync(createLevyDto.SemesterId);
            if (checkedSemester == null){
                return BadRequest(new{message=$"semester not found for this iD: {createLevyDto.SemesterId}"});
            }
            var student = await _userManager.FindByIdAsync(id);
            if (student == null ){
                return BadRequest(new{message="Student not found"});
            }
            var existingLevy = await _context.Levies.FirstOrDefaultAsync(x=>x.Name == createLevyDto.Name && x.SemesterId == createLevyDto.SemesterId);
            if (existingLevy == null){
                var levy = createLevyDto.ToCreateLevyDto();
                var createdLevy = await _adminRepo.CreateStudentLevyAsync(levy, id);
                if (createdLevy == null){
                    return BadRequest(new{message="No student to assign levies for semester"});
                }
                return Ok(new {message=$"{createLevyDto.Name} successfully added for {student.FirstName}"});

            }return BadRequest(new{message=$"levy already exists for this Name: {createLevyDto.Name}"});
        }

        [HttpGet("list/transactions/students")]
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

            return Ok(new{
                totalToBal=totalToBal,
                totalBilling=totalBilling,
                totalPay=totalPay,
                transactions = transactions
            });
        }

    }
}