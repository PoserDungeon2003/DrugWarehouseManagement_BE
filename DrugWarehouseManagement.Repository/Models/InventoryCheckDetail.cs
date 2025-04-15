using DrugWarehouseManagement.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Repository.Models
{
    public class InventoryCheckDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InventoryCheckDetailId { get; set; }
        public int InventoryCheckId { get; set; }
        public int LotId { get; set; }

        public InventoryCheckStatus Status { get; set; }
        public int Quantity { get; set; }
        public int? CheckQuantity { get; set; }
        public string Reason { get; set; } = null!;
        public string? Notes { get; set; } // Ghi chú chi tiết
        public virtual Lot Lot { get; set; }
        public virtual InventoryCheck InventoryCheck { get; set; }
    }
}
