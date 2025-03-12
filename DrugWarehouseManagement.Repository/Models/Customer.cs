using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DrugWarehouseManagement.Common;

namespace DrugWarehouseManagement.Repository.Models
{
    public class Customer : TimeStamp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public bool IsLoyal { get; set; }  = false;
        public string DocumentNumber { get; set; } = null!; // Số chứng từ của khách hàng
        public CustomerStatus Status { get; set; } = CustomerStatus.Active;

        public virtual ICollection<Outbound> Outbounds { get; set; } = new List<Outbound>();
    }
}
