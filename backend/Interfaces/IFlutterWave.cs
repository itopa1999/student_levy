using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Interfaces
{
    public interface IFlutterWave
    {
        Task<string> InitializePaymentAsync(decimal amount);
    }
}