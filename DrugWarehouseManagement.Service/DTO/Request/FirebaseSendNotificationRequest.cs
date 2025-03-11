using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class FirebaseSendNotificationRequest
    {
        [Required]
        public string DeviceToken { get; set; } = null!;
        [Required]
        public string Title { get; set; } = null!;
        [Required]
        public string Body { get; set; } = null!;
        [Required]
        public Dictionary<string, string> Data { get; set; } = null!;
    }
}
