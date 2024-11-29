using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class AccountLoginResponse
    {
        public string Token { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}
