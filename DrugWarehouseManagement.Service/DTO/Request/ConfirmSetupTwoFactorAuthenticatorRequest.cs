using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class ConfirmSetupTwoFactorAuthenticatorRequest
    {
        [Required]
        public string OTPCode { get; set; } = null!;
    }
}
