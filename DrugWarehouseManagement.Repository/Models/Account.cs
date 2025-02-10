using DrugWarehouseManagement.Common.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DrugWarehouseManagement.Repository.Models
{
    public class AccountSettings
    {
        public string PreferredLanguage { get; set; } = "vi";
    }

    public class Account : IdentityUser<Guid>
    {
        [Unicode(true)]
        public string FullName { get; set; } = null!;
        public AccountStatus Status { get; set; } = AccountStatus.Active;
        [ProtectedPersonalData]
        public byte[]? tOTPSecretKey { get; set; }
        public int? RoleId { get; set; }
        [ProtectedPersonalData]
        public string? OTPCode { get; set; }
        public AccountSettings? AccountSettings { get; set; }

        public virtual Role Role { get; set; } = null!;
        public virtual List<AuditLogs> AuditLogs { get; set; } = null!;
    }
}
