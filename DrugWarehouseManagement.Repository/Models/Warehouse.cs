using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Repository.Models
{
    public class Warehouse
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = null!;
        public string Address { get; set; } = null!;

        public virtual List<Lot> Lots { get; set; } = null!;
    }
}
