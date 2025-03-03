using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Repository.Models
{
    public class TransferOrderDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransferOrderDetailId { get; set; }
        public DateOnly ExpiryDate { get; set; }
        public string UnitType { get; set; } = null!;
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public int LotId { get; set; }
        public int TransferOrderId { get; set; }

        public virtual Lot Lot { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
        public virtual TransferOrder TransferOrder { get; set; } = null!;
    }
}
