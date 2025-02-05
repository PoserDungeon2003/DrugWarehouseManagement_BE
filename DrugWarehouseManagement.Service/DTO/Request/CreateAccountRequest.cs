using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class CreateAccountRequest
    {
        [Unicode(true)]
        [Required]
        public string FullName { get; set; } = null!;
        [Required]
        public string UserName { get; set; } = null!;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Required]
        [MaxLength(15, ErrorMessage = "Max length is 15")]
        [MinLength(10, ErrorMessage = "Min length is 10")]
        [RegularExpression(@"^[\d+]*$", ErrorMessage = "Invalid phone number")]
        public string PhoneNumber { get; set; } = null!;
        [Required]
        public int RoleId { get; set; }
    }
}
