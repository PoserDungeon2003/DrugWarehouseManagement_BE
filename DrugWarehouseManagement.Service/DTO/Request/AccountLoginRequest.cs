using System.ComponentModel.DataAnnotations;

namespace DrugWarehouseManagement.Service.Request
{
    public class AccountLoginRequest
    {
        [Required]
        public string Username { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
        public string? tOtpCode { get; set; }
    }
}
