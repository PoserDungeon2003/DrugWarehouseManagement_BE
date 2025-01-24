using DrugWarehouseManagement.Common.Enums;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Repository.Models
{
    public class AccountSettings
    {
        public string Language { get; set; } = "vi";
    }

    public class Account : TimeStamp
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
        public Instant? LastLogin { get; set; }
        public AccountStatus Status { get; set; } = AccountStatus.Active;
        public byte[]? tOTPSecretKey { get; set; }
        public int? RoleId { get; set; }
        public AccountSettings? AccountSettings { get; set; }

        public virtual Role Role { get; set; } = null!;
        public virtual List<AuditLogs> AuditLogs { get; set; } = null!;
    }
}
