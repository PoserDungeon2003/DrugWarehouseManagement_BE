using DrugWarehouseManagement.Common;
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
        public DateOnly? ManufacturingDate { get; set; }
        public DateOnly? ExpiryDate { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int? OpeningStock { get; set; }
        public decimal TotalPrice { get; set; }
        public int InboundId { get; set; }
        public InboundDetailStatus Status { get; set; } = InboundDetailStatus.Pending;
        public virtual Inbound Inbound { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}
