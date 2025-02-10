using System.ComponentModel.DataAnnotations;

namespace DrugWarehouseManagement.Repository.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        public string RoleName { get; set; } = null!;

        public virtual List<Account> Accounts { get; set; } = null!;

    }
}
