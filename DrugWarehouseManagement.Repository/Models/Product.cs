using DrugWarehouseManagement.Common.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DrugWarehouseManagement.Repository.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string ProductCode { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string MadeFrom { get; set; } = null!;
        public ProductStatus Status { get; set; } = ProductStatus.Active;
        
        public virtual List<OutboundDetails> OutboundDetails { get; set; } = null!;
        public virtual List<Inbound> Inbound { get; set; } = null!;
        public virtual List<TransferOrderDetail> TransferOrderDetails { get; set; } = null!;
    }
}
