using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Dtos;
using backend.models;

namespace backend.Interfaces
{
    public interface IStudentRepository
    {
        Task<StuGetDepartmentDto?> GetDepartmentAsync(int id,  string appUserId);
    }
}