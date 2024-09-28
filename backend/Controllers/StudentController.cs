using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using backend.Data;
using backend.Dtos;
using backend.Interfaces;
using backend.models;
using backend.Services;
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
        private readonly ReceiptPDFService _ReceiptPDF;

        public StudentController(
            IStudentRepository studentRepo,
            ApplicationDBContext context,
            UserManager<AppUser> userManager,
            ReceiptPDFService ReceiptPDF
        )
        {
            _studentRepo = studentRepo;
            _context = context;
            _userManager = userManager;
            _ReceiptPDF = ReceiptPDF;
        }

        [HttpGet("student/dashboard")]
        [Authorize]
        [Authorize(Policy = "IsStudent")]
        public async Task<IActionResult> StudentDashboard(){
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var claims = User.Claims.ToList(); // Get all claims associated with the user
            foreach (var claim in claims)
            {
                Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
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

        [HttpGet("student/transaction")]
        [Authorize]
        [Authorize(Policy = "IsStudent")]
        public async Task<IActionResult> StudentTransaction(){
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var student = await _userManager.FindByIdAsync(userId);
            var transactions = await _context.Transactions
            .Where(x=>x.AppUserId == student.Id)
            .Include(l=>l.AppUser)
            .Include(c=>c.Levy)
            .OrderByDescending(t => t.CreatedAt)
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

            var totalToBal = await _context.Levies
            .Where(x => x.ToBalance != 0 && x.AppUserId == student.Id)
            .SumAsync(x => x.ToBalance);

            var totalBilling = await _context.Levies
            .Where(x => x.AppUserId == student.Id)
            .SumAsync(x => x.Amount);

            var totalPay = await _context.Transactions
            .Where(x=>x.AppUserId == student.Id)
            .SumAsync(x => x.Amount);

            return Ok(new{
                totalToBal=totalToBal,
                totalBilling=totalBilling,
                totalPay=totalPay,
                transactions = transactions
            });
        }


        [HttpGet("get/levies/details")]
        [Authorize]
        [Authorize(Policy = "IsStudent")]
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

        [HttpGet("print/receipt/{id:int}")]
        [Authorize]
        [Authorize(Policy = "IsStudent")]
        public async Task<IActionResult> PrintReceipt([FromRoute] int id){
            var transaction = await _context.Transactions
            .Include(x=>x.Levy)
            .Include(x=>x.AppUser)
            .FirstOrDefaultAsync(x=>x.Id==id);
            if (transaction == null){
                return BadRequest(new{message="Receipt for this transaction not found"});
            }
            var htmlContent = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <style>
                    @media print {{
                        @page {{
                            size: 80mm 100mm; /* A6 size */
                            margin: 10mm; /* Adjust margins if necessary */
                        }}
                    }}
                    body {{
                        font-family: Arial, sans-serif;
                        margin: 0;
                        padding: 0;
                        width: 80mm; /* Set body width to 80mm */
                        height: 100mm; /* Set height to match page size */
                        position: relative;
                        overflow: hidden; /* Prevent overflow */
                    }}
                    h2,h3 {{
                        text-align: center;
                    }}
                    table {{
                        width: 100%;
                        border-collapse: collapse;
                    }}
                    th, td {{
                        padding: 5px;
                        text-align: left;
                        border-bottom: 1px solid #ddd;
                    }}
                    th {{
                        background-color: #f2f2f2;
                    }}
                    .signature {{
                        margin-top: 20px;
                        border-top: 1px solid #000; /* Signature line */
                        height: 50px; /* Height of the signature area */
                        text-align: center;
                        padding-top: 10px;
                    }}
                </style>
            </head>
            <body>
                <h2>Transaction Receipt</h2>
                <h3>School Levy</h3>
                <table>
                    <tr>
                        <th>Transaction ID</th>
                        <td>{transaction.Id}</td>
                    </tr>
                    <tr>
                        <th>Student Name</th>
                        <td>{transaction.AppUser?.FirstName} {transaction.AppUser?.LastName}</td>
                    </tr>
                    <tr>
                        <th>Levy Name</th>
                        <td>{transaction.Levy?.Name}</td>
                    </tr>
                    <tr>
                        <th>Amount</th>
                        <td>{transaction.Amount}</td>
                    </tr>
                    <tr>
                        <th>Description</th>
                        <td>{transaction.Description}</td>
                    </tr>
                    <tr>
                        <th>Method</th>
                        <td>{transaction.Method}</td>
                    </tr>
                    <tr>
                        <th>Created At</th>
                        <td>{transaction.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")}</td>
                    </tr>
                    <tr>
                        <th>Transaction Ref</th>
                        <td>{transaction.TransID}</td>
                    </tr>
                </table>
                <div class='signature'>
                    Signature: ___________________________
                </div>
            </body>
            </html>
            ";
            var pdfBytes = _ReceiptPDF.GenerateTransactionReceipt(htmlContent);
            return File(pdfBytes, "application/pdf", $"Transaction_{transaction.Id}_Receipt.pdf");

        }

        [HttpGet("student/clearance")]
        [Authorize]
        [Authorize(Policy = "IsStudent")]
        public async Task<IActionResult> StudentClearance(){
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var student = await _userManager.FindByIdAsync(userId);
            var departments = await _studentRepo.GetClearanceAsync(student.DepartmentId.Value, student.Id);
            if (departments == null){
                return BadRequest(new{message = "Department not found"});
            }
            return Ok(departments);

        }

        [HttpGet("print/clearance/receipt/{id:int}")]
        [Authorize]
        [Authorize(Policy = "IsStudent")]
        public async Task<IActionResult> PrintClearance([FromRoute] int id){
            var semester = await _context.Semesters.FindAsync(id);
            if (semester == null){
                return BadRequest(new{message="Semester Not Found"});
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var student = await _userManager.FindByIdAsync(userId);
            var levy =  _context.Levies.Where(x=>x.SemesterId == id && x.AppUserId == student.Id);
            var totalAmount = await levy.SumAsync(x => x.Amount);
            if (totalAmount == 0){
                return BadRequest(new{message="This Semester is on available for now."});
            }
            var isDefault = await levy.SumAsync(x=>x.ToBalance);
            if (isDefault != 0){
                return BadRequest(new{message="You have not completed semester levies"});
            }

            var htmlContent = $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Clearance Statement</title>
                <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Clearance Statement</title>
                <style>
                    body {{ font-family: Arial, sans-serif; padding: 20px; }}
                    .container {{ max-width: 600px; margin: auto; padding: 20px; border: 1px solid #ccc; }}
                    h2 {{ text-align: center; color: #333; font-size: 24px; margin-bottom: 20px; }}
                    p {{ font-size: 14px; line-height: 1.5; margin-bottom: 20px; }}
                    .clearance-status, .total-pay {{ text-align: center; padding: 10px; margin-bottom: 20px; font-weight: bold; font-size: 16px; border: 1px solid #ccc; }}
                    .clearance-status {{ background-color: #dff0d8; color: #3c763d; }}
                    .total-pay {{ background-color: #f9f9f9; color: #333; }}
                    .signature {{ text-align: center; margin-top: 30px; }}
                    .signature p {{ margin-bottom: 5px; }}
                    .signature-line {{ display: inline-block; margin-top: 10px; width: 200px; height: 1px; background-color: #000; }}
                </style>
            </head>
            <body>
            <div class='container'>
                <h2>Clearance Statement</h2>
                <p>This is to certify that <strong>{student?.FirstName} {student?.LastName}</strong>, with Student MatricNo <strong>{student?.MatricNo}</strong>, has successfully completed the payment of all required fees for the {semester.Name}</p>
                <p>
                    The student is hereby cleared of any financial obligations for the session and is eligible to participate in academic activities for the said session.
                </p>
                <div class='total-pay'>Total Pay: <strong>₦{totalAmount}</strong></div>
                <div class='clearance-status'>Clearance Status: COMPLETED</div>
                <div class='signature'><p>Authorized Signature</p><div class='signature-line'></div><p>Registrar/Financial Officer</p></div>
            </div>
            </body>
            </html>
        ";
        var pdfBytes = _ReceiptPDF.GenerateTransactionReceipt(htmlContent);
        return File(pdfBytes, "application/pdf", $"Clearance_{semester.Id}_Receipt.pdf");
    }
            


        // [HttpPost("payment/checkout/")]
        // [Authorize]
        // public async Task<IActionResult> CheckoutPayment()





    }
}