using DrugWarehouseManagement.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DrugWarehouseManagement.Repository.Models
{
    public class Warehouse
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WarehouseId { get; set; }
        public string WarehouseCode { get; set; } = null!;
        public string WarehouseName { get; set; } = null!;
        public string Address { get; set; } = null!;
        public WarehouseStatus Status { get; set; } = WarehouseStatus.Active;
        
        public virtual List<Lot> Lots { get; set; } = null!;
        public virtual List<Inbound> Inbounds { get; set; } = null!;
        public virtual List<LotTransfer> LotTransfers { get; set; } = null!;
    }
}
