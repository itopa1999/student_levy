using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Helpers
{
    public class StudentTransactionQueryObjects
    {
        public string? FilterOptions { get; set; }
        public string? OrderOptions { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        
    }


    public class DepartmentQuery
    {
        public string? FilterOptions { get; set; }
        // public int PageNumber { get; set; } = 1;
        // public int PageSize { get; set; } = 10;
    }

    public class LevyQuery
    {
        public string? FilterOptions { get; set; }
        public string? TransactionFilterOptions { get; set; }
        public string? OrderOptions { get; set; }
    }


    public class StudentQuery
    {
        public string? FilterOptions { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        
    }


    public class DefaultStudentQuery
    {
        public string? FilterOptions { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        
    }

    public class TransactionQuery
    {
        public string? FilterOptions { get; set; }
        public string? OrderOptions { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        
    }


}