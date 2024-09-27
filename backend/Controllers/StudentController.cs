using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using backend.Data;
using backend.Dtos;
using backend.Interfaces;
using backend.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers
{
    [Route("student/api")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        public readonly ApplicationDBContext _context;
        public readonly UserManager<AppUser> _userManager;
        public readonly IStudentRepository _studentRepo;

        public StudentController(
            IStudentRepository studentRepo,
            ApplicationDBContext context,
            UserManager<AppUser> userManager
        )
        {
            _studentRepo = studentRepo;
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("student/dashboard")]
        [Authorize]
        public async Task<IActionResult> StudentDashboard(){
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var student = await _userManager.FindByIdAsync(userId);
            var balance = student.Balance;
            var transactions = await _context.Transactions
            .Where(x=>x.AppUserId == student.Id)
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
                LevyName = t.Levy.Name,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();
            return Ok(new{
                stu_balance=balance,
                transactions = transactions
            });
        }

        [HttpGet("get/levies/details")]
        [Authorize]
        public async Task<IActionResult> GetLeviesDetails(){
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var student = await _userManager.FindByIdAsync(userId);
            var departments = await _studentRepo.GetDepartmentAsync(student.DepartmentId.Value, student.Id);
            if (departments == null){
                return BadRequest(new{message = "Department not found"});
            }
            return Ok(departments);

        }



    }
}