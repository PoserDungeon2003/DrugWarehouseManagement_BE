using DrugWarehouseManagement.Repository.Models;

namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class ViewAccount
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
        public string Status { get; set; } = null!;
        public bool TwoFactorEnabled { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool EmailConfirmed { get; set; }
        public AccountSettings? AccountSettings { get; set; }
    }
}
