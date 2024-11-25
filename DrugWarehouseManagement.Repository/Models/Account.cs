using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Repository.Models
{
    public class Account : BaseModel
    {
        [Key]
        public Guid AccountId { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public virtual Role Role { get; set; } = null!;
    }
}
