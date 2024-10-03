using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Interfaces;
using backend.models;

namespace backend.Repository
{
    public class UserRepository : IUserRepository
    {
        public readonly ApplicationDBContext _context;
        public UserRepository(ApplicationDBContext context)
        {
            _context = context;
        }
        

        public async Task<Audit> CreateAuditAsync(string name, string action)
        {
            var audit = new Audit{
                Action = action,
                User = name
            };
            await _context.Audits.AddAsync(audit);
            await _context.SaveChangesAsync();
            return audit;
        }
    }
}