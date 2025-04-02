using DrugWarehouseManagement.Common;
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
        public TwoFactorAuthenticatorSetupStatus TwoFactorAuthenticatorStatus { get; set; } = TwoFactorAuthenticatorSetupStatus.NotStarted;
        [ProtectedPersonalData]
        public byte[]? tOTPSecretKey { get; set; }
        public int? RoleId { get; set; }
        [ProtectedPersonalData]
        public string? OTPCode { get; set; }
        public string? BackupCode { get; set; }
        public AccountSettings? AccountSettings { get; set; }

        public virtual Role Role { get; set; } = null!;
        public virtual List<AuditLogs> AuditLogs { get; set; } = null!;
        public virtual List<Inbound> Inbounds { get; set; } = null!;
        public virtual List <Outbound> Outbounds { get; set; } = null!;
        public virtual List<LotTransfer> LotTransfers { get; set; } = null!;
        public virtual List<InboundRequest> InboundRequests { get; set; } = null!;
        public virtual List<InboundReport> InboundReports { get; set; } = null!;
        public virtual List<Asset> Assets { get; set; } = null!;
        public virtual List<Device> Devices { get; set; } = null!;
    }
}
