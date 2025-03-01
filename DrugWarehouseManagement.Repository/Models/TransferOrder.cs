using DrugWarehouseManagement.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Repository.Models
{
    public class TransferOrder : TimeStamp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransferOrderId { get; set; }
        public string TransferOrderCode { get; set; } = null!;
        public TransferOrderStatus TransferOrderStatus { get; set; } = TransferOrderStatus.Pending;
        public int FromWareHouseId { get; set; }
        public int ToWareHouseId { get; set; }
        public Guid AccountId { get; set; }

        [ForeignKey("FromWareHouseId")]
        public virtual Warehouse FromWareHouse { get; set; } = null!;
        [ForeignKey("ToWareHouseId")]
        public virtual Warehouse ToWareHouse { get; set; } = null!;
        public virtual List<TransferOrderDetail> TransferOrderDetails { get; set; } = null!;
        public virtual Account Account { get; set; } = null!;
    }
}
