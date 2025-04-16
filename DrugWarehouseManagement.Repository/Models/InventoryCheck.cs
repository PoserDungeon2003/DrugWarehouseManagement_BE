using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace DrugWarehouseManagement.Repository.Models
{
    public class InventoryCheck
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InventoryCheckId { get; set; }
        public string Title { get; set; } = null!;
        public Instant CheckDate { get; set; } = SystemClock.Instance.GetCurrentInstant();
        public Guid AccountId { get; set; }
        public int WarehouseId { get; set; }
        public string? Notes { get; set; } // Ghi chú tổng quan

        public virtual Warehouse Warehouse { get; set; } = null!;
        public virtual Account Account { get; set; } = null!;
        public virtual ICollection<InventoryCheckDetail> InventoryCheckDetails { get; set; } = new List<InventoryCheckDetail>();
    }
}
