using System.ComponentModel.DataAnnotations;

namespace DrugWarehouseManagement.Repository.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }
        public string RoleName { get; set; } = null!;

        public virtual List<Account> Accounts { get; set; } = null!;

    }
}
