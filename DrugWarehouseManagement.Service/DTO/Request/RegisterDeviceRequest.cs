using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class RegisterDeviceRequest
    {
        [Required]
        public string DeviceName { get; set; } = null!;
        [Required]
        public string DeviceCode { get; set; } = null!;
        [Required]
        public string DeviceType { get; set; } = null!;
        public DateTime? ExpiryDate { get; set; }
    }
}
