using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using backend.Data;
using backend.Dtos;
using backend.Helpers;
using backend.Interfaces;
using backend.Mappers;
using backend.models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace backend.Repository
{
    public class StudentRepository : IStudentRepository
    {
        public readonly ApplicationDBContext _context;
        public readonly UserManager<AppUser> _userManager;
        private readonly string _secretKey;
        private readonly HttpClient _httpClient;
        public StudentRepository(
            ApplicationDBContext context,
            UserManager<AppUser> userManager,
            IConfiguration config,
            HttpClient httpClient
            )
        {
            _context = context;
            _userManager = userManager;
            _secretKey = config["Flutter:Secret_key"];
            _httpClient = httpClient;
        }

        public async Task<List<studentTransactionDto>?> GetAllTransactions(AppUser appUser, StudentTransactionQueryObjects transQuery)
        {
            var transactions = _context.Transactions
            .Where(x=>x.AppUserId==appUser.Id)
            .Include(l=>l.AppUser)
            .Include(c=>c.Levy)
            .OrderByDescending(t => t.CreatedAt)
            .AsQueryable();
            if (!string.IsNullOrWhiteSpace(transQuery.FilterOptions)){
                transactions = transactions.Where(x=>x.TransID.Contains(transQuery.FilterOptions)
                || x.Description.Contains(transQuery.FilterOptions)
                || x.Levy.Name.Contains(transQuery.FilterOptions)
                || x.Amount.ToString().Contains(transQuery.FilterOptions)
                || x.Id.ToString().Contains(transQuery.FilterOptions)
                );
            }
            if (!string.IsNullOrWhiteSpace(transQuery.OrderOptions)){
                if (transQuery.OrderOptions == "Amount"){
                    transactions = transactions.OrderByDescending(x=>x.Amount);
                }
                if (transQuery.OrderOptions == "TranID"){
                    transactions = transactions.OrderByDescending(x=>x.Id);
                }
                if (transQuery.OrderOptions == "Date"){
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
                StudentID=appUser.Id
            });
            var SkipNumber = (transQuery.PageNumber - 1) * transQuery.PageSize;
            return await transactionDto.Skip(SkipNumber).Take(transQuery.PageSize).ToListAsync();
        
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

        public async Task<StuDetailsDto?> GetStuDetailsAsync(string id)
        {
            var student = await _userManager.Users
            .Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.Id == id);
            if (student == null)
            {
                return null;
            }
            var studentDto= student.ToStuDetailsDto();

            return studentDto;
            
        }

        public async Task<FlutterwaveResponseDto> VerifyStudentPayment(string transaction_id, int id)
        {
            
            var flutterwaveSecretKey = _secretKey;
            var url = $"https://api.flutterwave.com/v3/transactions/{transaction_id}/verify";
            try
            {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", flutterwaveSecretKey);
            _httpClient.Timeout = TimeSpan.FromSeconds(30); // Set timeout

            Console.WriteLine("Sending request to Flutterwave API...");
            var response = await _httpClient.GetAsync(url);
            Console.WriteLine($"Response received.{response}");

            response.EnsureSuccessStatusCode(); // Throws if not 200-299

            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Content: {content}");

            // Deserialize and return the response
            return JsonConvert.DeserializeObject<FlutterwaveResponseDto>(content);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Request error: {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"The request timed out.{ex}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }

            return null; // Handle appropriately
            }
    }
}