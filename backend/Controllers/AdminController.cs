using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Dtos;
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

        [HttpPost("create/department")]
        // [Authorize]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentDto departmentDto){
            if (!ModelState.IsValid){
                return BadRequest(ModelState);
            }
            var existingDepartment = await _context.Departments.FirstOrDefaultAsync(x=>x.Name == departmentDto.Name);
            if (existingDepartment == null){
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
                return StatusCode(201, new {message="Department created."});
            }return StatusCode(400, new{message = $"Department name {existingDepartment.Name} already exists"});
            

        }

        [HttpGet("list/department")]
        public async Task<IActionResult> ListDepartment(){
            var departments = await _adminRepo.ListDepartmentAsync();
            return Ok(departments);
             
        }

        [HttpGet("get/department/details/{id:int}")]
        public async Task<IActionResult> GetDepartmentDetails([FromRoute] int id){
            var departments = await _adminRepo.GetDepartmentAsync(id);
            return Ok(departments);
             
        }

        [HttpPut("update/department/details/{id:int}")]
        public async Task<IActionResult> UpdateDepartmentDetails([FromRoute] int id, [FromBody] UpdateDepartmentDto updateDepartmentDto){
            if (!ModelState.IsValid){
                return BadRequest(ModelState);
            }
            var departments = await _adminRepo.UpdateDepartmentAsync(id, updateDepartmentDto);
            return Ok(departments);
             
        }

        [HttpPost("create/student")]
        public async Task<IActionResult> CreateStudent([FromBody] CreateStudentDto createStudentDto){
            if (!ModelState.IsValid){
                return BadRequest(ModelState);
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

    [HttpPost("upload/students")]
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




        [HttpGet("get/students/details/{id}")]
        public async Task<IActionResult> GetStudentDetails(string id){
            var student = await _adminRepo.GetStudentDetailsAsync(id);
            if (student == null){
                return NoContent();
            }
            return Ok(student);
        }

        [HttpGet("list/students")]
        public async Task<IActionResult> ListStudents(){
            var students = await _adminRepo.ListStudentAsync();
            if (students == null){
                return NoContent();
            }
            return Ok(students);
        }



        [HttpPut("update/student/details/{id}")]
        public async Task<IActionResult> UpdateStudentDetails([FromRoute] string id, [FromBody] UpdateStudentDetailsDto studentDetailsDto){
            if (!ModelState.IsValid){
                return BadRequest(ModelState);
            }
            var student = await _adminRepo.UpdateStudentDetailsAsync(id, studentDetailsDto);
            return Ok(student);
        }


        

    }
}