using DrugWarehouseManagement.Common.Enums;
using Microsoft.EntityFrameworkCore;
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
    public class Drug : TimeStamp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DrugId { get; set; } 
        public string Code { get; set; } = null!;
        [Unicode(true)]
        public string Name { get; set; } = null!;
        [Unicode(true)]
        public string Description { get; set; } = null!;
        public string SKU { get; set; } = null!;
        [Unicode(true)]
        public string Categories { get; set; } = null!;
        [Unicode(true)]
        public string UnitOfMeasure { get; set; } = null!;
        public string Supplier { get; set; } = null!;
        public string ApprovalStatus { get; set; } = null!; // Approval status (e.g., trial, approved)
        public string ImageUrl { get; set; } = null!;
        public string Instructions { get; set; } = null!;
        public string StorageCondition { get; set; } = null!;
        public string SideEffects { get; set; } = null!;
        public Instant? ExpiryDate { get; set; }
        public ProductStatus Status { get; set; } = ProductStatus.Active;
        public string TemperatureRange { get; set; } = null!;
        public string HumidityRange { get; set; } = null!;
        public bool IsLightSensitive { get; set; } = false;
        // Inventory Information
        public Instant? ReorderPoint { get; set; }
        public int ReorderQuantity { get; set; }
    }
}
