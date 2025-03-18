
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DrugWarehouseManagement.Repository.Models
{
    public class Lot
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LotId { get; set; }
        public string LotNumber { get; set; } = null!;
        public int Quantity { get; set; }
        public int? TemporaryWarehouseId { get; set; }
        //TODO: Add ExpiryDate
        public DateOnly? ManufacturingDate { get; set; }
        public DateOnly ExpiryDate { get; set; }
        public int WarehouseId { get; set; } // Permanent Warehouse
        public int ProviderId { get; set; }
        public int ProductId { get; set; }

        public virtual Provider Provider { get; set; } = null!;
        public virtual Warehouse Warehouse { get; set; } = null!;
        public virtual Warehouse? TemporaryWarehouse { get; set; }
        public virtual Product Product { get; set; } = null!;
        public virtual ICollection<OutboundDetails> OutboundDetails { get; set; } = new List<OutboundDetails>(); //1-N
        public virtual List<LotTransferDetail> LotTransferDetails { get; set; } = null!;

    }
}
