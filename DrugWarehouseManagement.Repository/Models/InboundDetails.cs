using DrugWarehouseManagement.Common.Enums;
using NodaTime;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DrugWarehouseManagement.Repository.Models
{
    public class InboundDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InboundDetailsId { get; set; }
        public string LotNumber { get; set; } = null!;
        public Instant ExpiryDate { get; set; }
        public int Quantity { get; set; }
        public int ActualQuantity { get; set; }
        public string UnitType { get; set; } = null!;
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public int InboundId { get; set; }
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public InboundDetailStatus Status { get; set; } = InboundDetailStatus.Pending;

        public virtual Inbound Inbounds { get; set; } = null!;
        public virtual Product Products { get; set; } = null!;
        public virtual Warehouse Warehouse { get; set; } = null!;

    }
}
