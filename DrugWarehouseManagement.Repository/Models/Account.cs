using DrugWarehouseManagement.Common.Enums;
using Microsoft.EntityFrameworkCore;
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
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Unicode(true)]
        public string FullName { get; set; } = null!;
        [MaxLength(15, ErrorMessage = "Max length is 15")]
        public string PhoneNumber { get; set; } = null!;
        public string Password { get; set; } = null!;
        public DateTime? LastLogin { get; set; }
        public int? RoleId { get; set; }
        public Status Status { get; set; } = Status.Active;

        public virtual Role Role { get; set; } = null!;
    }
}
