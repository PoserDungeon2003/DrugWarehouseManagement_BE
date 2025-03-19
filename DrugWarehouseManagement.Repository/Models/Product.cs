using DrugWarehouseManagement.Common;
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
        public string SKU { get; set; } = null!;
        public string MadeFrom { get; set; } = null!;
        public ProductStatus Status { get; set; } = ProductStatus.Active;
        
        public virtual List<LotTransferDetail> LotTransferDetails { get; set; } = null!;
        public virtual List<InboundRequestDetails> InboundRequestDetails { get; set; } = null!;
        public virtual List<InboundDetails> InboundDetails { get; set; } = null!;
        public List<Categories> Categories { get; set; } = null!;
    }
}
