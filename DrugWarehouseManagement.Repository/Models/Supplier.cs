using DrugWarehouseManagement.Common.Enums;
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
    public class Supplier : TimeStamp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SupplierId { get; set; } // Primary Key
        public string SupplierName { get; set; } = string.Empty; // Name of the supplier
        public string ContactPerson { get; set; } = string.Empty; // Primary contact person
        public string Phone { get; set; } = string.Empty; // Phone number
        public string Email { get; set; } = string.Empty; // Email address
        public string Address { get; set; } = string.Empty; // Full address
        public string City { get; set; } = string.Empty; // City
        public string State { get; set; } = string.Empty; // State or province
        public string Country { get; set; } = string.Empty; // Country
        public string TaxId { get; set; } = string.Empty; // Tax identification number
        public string? RegistrationNumber { get; set; } = string.Empty; // Business registration number
        public SupplierStatus Status { get; set; } = SupplierStatus.Active; // Status of the supplier   
        public string? Notes { get; set; } = string.Empty; // Additional notes
        public Instant? LastOrderDate { get; set; } // Date of the last order

    }
}
