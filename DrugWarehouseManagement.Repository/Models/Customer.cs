using DrugWarehouseManagement.Common.Enums;
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
    public class Customer : TimeStamp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
       
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string WardCode { get; set; }
        public string DistrictCode { get; set; }
        public string ProvinceCode { get; set; }
        public AccountStatus Status { get; set; } = AccountStatus.Active;
        public Instant? LastOrder { get; set; }

        public virtual List<Outbound> OutboundEntities { get; set; } = null!;

    }
}
