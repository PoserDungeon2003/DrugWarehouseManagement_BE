using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DrugWarehouseManagement.Common.Enums;
using NodaTime;

namespace DrugWarehouseManagement.Repository.Models
{
    public abstract class TimeStamp
    {
        [Required]
        public Instant CreatedAt { get; set; } = SystemClock.Instance.GetCurrentInstant();

        public Instant? UpdatedAt { get; set; }
    }
}
