using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Dtos;
using backend.Helpers;
using backend.models;

namespace backend.Interfaces
{
    public interface IAdminRepository
    {
        Task<Department> CreateDepartmentAsync(Department department);

        Task<List<Department>> ListDepartmentAsync(DepartmentQuery query);

        Task<ListDepartmentDto?> GetDepartmentAsync(int id, DepartmentQuery query);
        Task<Department?> UpdateDepartmentAsync(int id, UpdateDepartmentDto updateDepartmentDto);
        Task<AppUser> StudentDepartmentCreateAsync(AppUser appUser, int DepartmentId);
        Task<List<StudentDto>> ListStudentAsync(StudentQuery query);
        Task<StudentDto?> UpdateStudentDetailsAsync(string id, UpdateStudentDetailsDto studentDetailsDto);
        Task<StudentDetailsDto?> GetStudentDetailsAsync(string id, LevyQuery query);
        Task<Levy?> CreateLevyAsync(Levy levy);
        Task<GetSemesterDetailsDto?> GetSemesterDetailsAsync(int id);
        Task<Transaction> CreatePayStudentLevyAsync(Transaction transaction);
        Task<Levy?> CreateStudentLevyAsync(Levy levy, string id);
        Task<List<DefaultLeviesDto>> DefaultingStudentAsync(DefaultStudentQuery query);
        Task<List<studentTransactionDto>?> GetAdminAllTransactions(TransactionQuery query);
        Task<AdminProfileDto?> GetAdminDetailsAsync(string id);
        Task<List<AuditDo>> ListAuditAsync(AuditQuery query);
       

        
    }
}