using NodaTime;
using System.ComponentModel.DataAnnotations;

namespace DrugWarehouseManagement.Repository.Models
{
    public abstract class TimeStamp
    {
        [Required]
        public Instant CreatedAt { get; set; } = SystemClock.Instance.GetCurrentInstant();

        public Instant? UpdatedAt { get; set; }
    }
}
