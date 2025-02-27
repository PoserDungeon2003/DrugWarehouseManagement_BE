using DrugWarehouseManagement.Common.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DrugWarehouseManagement.Repository.Models
{
    public class Provider : TimeStamp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProviderId { get; set; }
        public string ProviderName { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? TaxCode { get; set; }
        public string? Nationality { get; set; }
        public string Email { get; set; } = null!;
        public ProviderStatus Status { get; set; } = ProviderStatus.Active;

        public virtual List<Inbound> Inbounds { get; set; } = null!;
        public virtual List<Product> Products { get; set; } = null!;

    }
}
