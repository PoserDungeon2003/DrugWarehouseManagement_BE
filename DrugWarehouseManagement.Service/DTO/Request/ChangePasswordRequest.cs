using System.ComponentModel.DataAnnotations;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class ChangePasswordRequest
    {
        [Required]
        public string OldPassword { get; set; } = null!;
        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "Password must be at least 8 characters and contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
        public string NewPassword { get; set; } = null!;
    }
}
