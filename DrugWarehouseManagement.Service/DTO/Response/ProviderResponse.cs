using DrugWarehouseManagement.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class ProviderResponse
    {
        public int ProviderId { get; set; }
        public string ProviderName { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? TaxCode { get; set; }
        public string? Nationality { get; set; }
        public string Email { get; set; } = null!;
        public string DocumentNumber { get; set; } = null!;
        public ProviderStatus Status { get; set; }
    }

}
