using System.ComponentModel.DataAnnotations;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class AccountLoginRequest
    {
        [Required]
        public string Username { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
        public string? OTPCode { get; set; }
        public string? BackupCode { get; set; }
        public bool? ForgotBackupCode { get; set; } = false;
    }
}
