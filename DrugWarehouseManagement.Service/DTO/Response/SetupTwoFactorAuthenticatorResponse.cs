using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class SetupTwoFactorAuthenticatorResponse
    {
        public string ImageUrlQrCode { get; set; } = null!;
        public string ManualEntryKey { get; set; } = null!;
    }
}
