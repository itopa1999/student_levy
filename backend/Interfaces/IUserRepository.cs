using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.models;

namespace backend.Interfaces
{
    public interface IUserRepository
    {
       
        Task<Audit> CreateAuditAsync(string name, string action);
    }
}