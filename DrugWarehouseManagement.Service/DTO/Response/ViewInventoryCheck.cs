using DrugWarehouseManagement.Common;
using DrugWarehouseManagement.Repository.Models;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class ViewInventoryCheck
    {
        public int InventoryCheckId { get; set; }
        public string Title { get; set; } = null!;
        public Instant CheckDate { get; set; }
        public Guid AccountId { get; set; }
        public string? Notes { get; set; } // Ghi chú tổng quan
        public WarehouseResponse Warehouse { get; set; } = null!;
        public List<InventoryCheckDetailReponse>? Details { get; set; }
    }

    public class InventoryCheckDetailReponse
    {
        public string LotNumber { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string SKU { get; set; } = null!;
        public string Status { get; set; } = null!;
        public int Quantity { get; set; }
        public int CheckQuantity { get; set; }
        public string Reason { get; set; } = null!;
        public string? Notes { get; set; } // Ghi chú chi tiết
    }
}
