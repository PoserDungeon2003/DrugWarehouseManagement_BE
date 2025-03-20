using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Repository.Models
{
    public class Device : TimeStamp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DeviceId { get; set; }
        public string DeviceName { get; set; } = null!;
        public string DeviceCode { get; set; } = null!;
        public string DeviceType { get; set; } = null!;
        [ProtectedPersonalData]
        public string ApiKey { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public bool IsRevoked { get; set; } = false;
        public Guid AccountId { get; set; }

        public virtual Account Account { get; set; } = null!;
    }
}
