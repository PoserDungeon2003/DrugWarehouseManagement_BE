using NodaTime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Repository.Models
{
    public class Lot
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LotId { get; set; }
        public string LotNumber { get; set; } = null!;
        public int TemporaryWarehouse { get; set; }
		//TODO: Add ExpiryDate
		public Instant ExpiryDate { get; set; }
        public int WarehouseId { get; set; } // Permanent Warehouse

        public virtual Warehouse Warehouse { get; set; } = null!;
		public virtual ICollection<OutboundDetails> OutboundDetails { get; set; } = new List<OutboundDetails>(); //1-N

	}
}
