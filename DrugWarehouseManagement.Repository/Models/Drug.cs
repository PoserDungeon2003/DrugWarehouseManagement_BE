using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Repository.Models
{
    public class Drug : BaseModel
    {
        [Key]
        public Guid DrugId { get; set; } = Guid.NewGuid();
        public Guid? ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
        public string TemperatureRange { get; set; } = null!;
        public string HumidityRange { get; set; } = null!;
        public bool IsLightSensitive { get; set; } = false;
    }
}
