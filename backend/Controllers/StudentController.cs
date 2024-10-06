using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using backend.Data;
using backend.Dtos;
using backend.Helpers;
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
        private readonly FlutterwaveService _flutterRepo;
        public readonly IUserRepository _userRepo;

        public StudentController(
            IStudentRepository studentRepo,
            ApplicationDBContext context,
            UserManager<AppUser> userManager,
            ReceiptPDFService ReceiptPDF,
            FlutterwaveService flutterRepo,
            IUserRepository userRepo
        )
        {
            _studentRepo = studentRepo;
            _context = context;
            _userManager = userManager;
            _ReceiptPDF = ReceiptPDF;
            _flutterRepo = flutterRepo;
            _userRepo = userRepo;
        }

        [HttpGet("student/dashboard")]
        [Authorize]
        [Authorize(Policy = "IsStudent")]
        public async Task<IActionResult> StudentDashboard(){
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return StatusCode(401,  new{message="Authorized Access"});
            }

            var student = await _userManager.FindByIdAsync(userId);
            decimal balance = student.Balance;
            var transactions = await _context.Transactions
            .Where(x=>x.AppUserId == student.Id)
            .Include(l=>l.AppUser)
            .Include(c=>c.Levy)
            .OrderByDescending(t => t.CreatedAt)
            .Take(10)
            .Select(t => new studentTransactionDto
            {
                Id = t.Id,
                Amount = (decimal)(double)t.Amount,
                TransID = t.TransID,
                Method = t.Method,
                Description = t.Description,
                LevyName = t.Levy.Name,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();
            return StatusCode(200, new{
                stu_balance=balance,
                transactions = transactions
            });
        }

        [HttpGet("student/transaction")]
        [Authorize]
        [Authorize(Policy = "IsStudent")]
        public async Task<IActionResult> StudentTransaction([FromQuery] StudentTransactionQueryObjects query){
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return StatusCode(401,  new{message="Authorized Access"});
            }
            var student = await _userManager.FindByIdAsync(userId);
            var transactions = await _studentRepo.GetAllTransactions(student, query);
            var totalToBal = await _context.Levies
            .Where(x => x.ToBalance != 0 && x.AppUserId == student.Id)
            .SumAsync(x => x.ToBalance);

            var totalBilling = await _context.Levies
            .Where(x => x.AppUserId == student.Id)
            .SumAsync(x => x.Amount);

            var totalPay = await _context.Transactions
            .Where(x=>x.AppUserId == student.Id)
            .SumAsync(x => x.Amount);

            return StatusCode(200, new{
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
                return StatusCode(401,  new{message="Authorized Access"});
            }
            var student = await _userManager.FindByIdAsync(userId);
            var departments = await _studentRepo.GetDepartmentAsync(student.DepartmentId.Value, student.Id);
            if (departments == null){
                return StatusCode(400, new{message = "Department not found"});
            }
            return StatusCode(200, departments);

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
                return StatusCode(400, new{message="Receipt for this transaction not found"});
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
                return StatusCode(401,  new{message="Authorized Access"});
            }
            var student = await _userManager.FindByIdAsync(userId);
            var departments = await _studentRepo.GetClearanceAsync(student.DepartmentId.Value, student.Id);
            if (departments == null){
                return StatusCode(400, new{message = "Department not found"});
            }
            return StatusCode(200, departments);

        }

        [HttpGet("print/clearance/receipt/{id:int}")]
        [Authorize]
        [Authorize(Policy = "IsStudent")]
        public async Task<IActionResult> PrintClearance([FromRoute] int id){
            var semester = await _context.Semesters.FindAsync(id);
            if (semester == null){
                return StatusCode(400, new{message="Semester Not Found"});
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return StatusCode(401,  new{message="Authorized Access"});
            }
            var student = await _userManager.FindByIdAsync(userId);
            var levy =  _context.Levies.Where(x=>x.SemesterId == id && x.AppUserId == student.Id);
            var totalAmount = await levy.SumAsync(x => x.Amount);
            if (totalAmount == 0){
                return StatusCode(400, new{message="This Semester is on available for now."});
            }
            var isDefault = await levy.SumAsync(x=>x.ToBalance);
            if (isDefault != 0){
                return StatusCode(400, new{message="You have not completed semester levies"});
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
            
    

        [HttpPost("payment/checkout/{id:int}")]
        [Authorize]
        [Authorize(Policy = "IsStudent")]
        public async Task<IActionResult> CheckoutPayment([FromBody] PayLevyDto payLevyDto, [FromRoute] int id){
            if (!ModelState.IsValid){
                return StatusCode(400, new{message=ModelState});
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return StatusCode(401,  new{message="Authorized Access"});
            }
            var student = await _userManager.FindByIdAsync(userId);
            var result = await _flutterRepo.InitializePayment(payLevyDto.Amount, student, id);
        
            return StatusCode(200, result);
        }

        [HttpGet("confirm/deposit/{amount}/{id}/{appUser}")]
        public  async Task<IActionResult> ConfirmDepositView([FromRoute] AppUser appUser,[FromRoute]int amount, [FromRoute] int id, [FromQuery] string status, [FromQuery] string tx_ref, [FromQuery] string transaction_id)
        {
            var isVerified = _studentRepo.VerifyStudentPayment(transaction_id, id);
            if (isVerified == null)
            {
                return StatusCode(400, new { message = "Payment verification failed", transaction_id });
            }
            if (isVerified?.Result?.Status != null && isVerified.Result.Status != "success" &&
                isVerified?.Result?.Message == "Transaction fetched successfully")
            {
                return StatusCode(400, new{message="Payment verification failed", transaction_id});
            }
            var levy = await _context.Levies
            .Include(x=>x.Semester)
            .Include(x=>x.AppUser)
            .FirstOrDefaultAsync(x=>x.Id == id);
            if (levy == null){
                return StatusCode(400, new{message="Levy not Found"});
            }
            var transaction = new Transaction{
                Amount = amount,
                Method = "Payment Gateway",
                Description = $"{levy.Name} payment for {levy.Semester.Name}",
                Payer = $"{levy.AppUser?.FirstName} {levy.AppUser?.LastName}",
                IsCompleted = true,
                AppUserId = levy.AppUserId,
                LevyId = levy.Id
            };
            await _context.Transactions.AddAsync(transaction);

            levy.ToBalance -= amount;
            if (appUser == null){
                return StatusCode(400, new{message="Student not found"});
            }

            appUser.Balance -= amount;

            await _context.SaveChangesAsync();

            return StatusCode(200, new { message = "Payment was successfully." });
        }



        [HttpGet("get/student/profile")]
        [Authorize]
        [Authorize(Policy = "IsStudent")]
        public async Task<IActionResult> StudentProfile(){
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return StatusCode(401,  new{message="Authorized Access"});
            }
            var student = await _userManager.FindByIdAsync(userId);
            if (student == null)
            {
                return StatusCode(401,  new{message="Authorized Access"});
            }
            var profile = await _studentRepo.GetStuDetailsAsync(userId);
            if (profile == null)
            {
                return StatusCode(400, new{message="No record found"});
            }
            return StatusCode(200, profile);


        }


        [HttpPost("download/transactions")]
        [Authorize]
        [Authorize(Policy = "IsStudent")]
        public async Task<IActionResult> DownloadStudentTransactions()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return StatusCode(401, new { message = "Authorized Access" });
            }

            var student = await _userManager.FindByIdAsync(userId);
            var transactions = await _context.Transactions
                .Where(x => x.AppUserId == student.Id)
                .Include(c => c.Levy)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            if (!transactions.Any())
            {
                return StatusCode(404, new { message = "No transactions found for the student." });
            }

            // Generate rows for each transaction
            var transactionRows = string.Join("", transactions.Select(t => $@"
                <tr>
                    <td>{t.Id}</td>
                    <td>{t.Amount}</td>
                    <td>{t.Method}</td>
                    <td>{t.Description}</td>
                    <td>{t.Payer}</td>
                    <td>{t.Levy?.Name}</td>
                    <td>{t.TransID}</td>
                    <td>{t.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")}</td>
                </tr>
            "));

            // Generate the full HTML content
            var htmlContent = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <style>
                    @media print {{
                        @page {{
                            size: auto;
                            margin: 10mm; /* Adjust margins if necessary */
                        }}
                    }}
                    body {{
                        font-family: Arial, sans-serif;
                        margin: 0;
                        padding: 0;
                        width: 100%; /* Set body width to 100% */
                        position: relative;
                    }}
                    h2 {{
                        text-align: center;
                    }}
                    table {{
                        width: 100%; /* Ensure table takes full width */
                        border-collapse: collapse;
                        margin-top: 20px;
                    }}
                    th, td {{
                        padding: 10px;
                        text-align: left;
                        border: 1px solid #ddd;
                    }}
                    th {{
                        background-color: #f2f2f2;
                    }}
                </style>
            </head>
            <body>
                <h2>Transaction Receipt</h2>
                <h3>Student: {student.FirstName} {student.LastName}</h3>
                <table>
                    <thead>
                        <tr>
                            <th>ID</th>
                            <th>Amount</th>
                            <th>Method</th>
                            <th>Description</th>
                            <th>Description</th>
                            <th>Levy Name</th>
                            <th>Transaction ID</th>
                            <th>Created At</th>

                        </tr>
                    </thead>
                    <tbody>
                        {transactionRows}
                    </tbody>
                </table>
                <div style='margin-top: 20px;'>
                    <p>Generated on {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}</p>
                </div>
            </body>
            </html>
            ";

            // Generate the PDF from the HTML
            var pdfBytes = _ReceiptPDF.GenerateTransactionReceipt(htmlContent);
            return File(pdfBytes, "application/pdf", $"Student_{student.Id}_Transactions.pdf");

        }




    }
}