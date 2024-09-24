using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.models;

namespace backend.Interfaces
{
    public interface IUserRepository
    {
        string CreateAdminUser(AppUser appUser);
    }
}