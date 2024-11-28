using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Repository.Models
{
    public class Product : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductId { get; set; }
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
        public DateTime? ExpiryDate { get; set; }

        // Inventory Information
        public DateTime? ReorderPoint { get; set; }
        public int ReorderQuantity { get; set; }

        public virtual ICollection<Drug> Drugs { get; set; } = null!;
    }
}
