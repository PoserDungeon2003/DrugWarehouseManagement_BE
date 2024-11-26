using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Repository.Models
{
    public class InventoryUpdate
    {
        public int Quantity { get; set; } // Updated quantity of the product in stock
        public decimal CostPrice { get; set; } // Cost price of the received goods
        public DateTime? ExpirationDate { get; set; } // Expiration date of the received drugs
        public string Location { get; set; } // Storage location
    }
}
