using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class UpdateAccountRequest
    {
        public string? UserName { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        [Unicode(true)]
        public string? FullName { get; set; }
        [MaxLength(15, ErrorMessage = "Max length is 15")]
        [MinLength(10, ErrorMessage = "Min length is 10")]
        [RegularExpression(@"^[\d+]*$", ErrorMessage = "Invalid phone number")]
        public string? PhoneNumber { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
    }
}
