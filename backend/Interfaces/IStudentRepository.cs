using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Dtos;
using backend.Helpers;
using backend.models;

namespace backend.Interfaces
{
    public interface IStudentRepository
    {
        Task<StuGetDepartmentDto?> GetDepartmentAsync(int id,  string appUserId);
        Task<StudentGetDepartmentDto?> GetClearanceAsync(int id,  string appUserId);
        Task<List<studentTransactionDto>?> GetAllTransactions(AppUser appUser, StudentTransactionQueryObjects transQuery);
    }
}