using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Dtos;
using backend.models;

namespace backend.Interfaces
{
    public interface IAdminRepository
    {
        Task<Department> CreateDepartmentAsync(Department department);

        Task<List<Department>> ListDepartmentAsync();

        Task<ListDepartmentDto?> GetDepartmentAsync(int id);
        Task<Department?> UpdateDepartmentAsync(int id, UpdateDepartmentDto updateDepartmentDto);
        Task<AppUser> StudentDepartmentCreateAsync(AppUser appUser, int DepartmentId);
        Task<List<StudentDto>> ListStudentAsync();
        Task<StudentDto?> UpdateStudentDetailsAsync(string id, UpdateStudentDetailsDto studentDetailsDto);
        Task<StudentDetailsDto?> GetStudentDetailsAsync(string id);

        
    }
}