using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace DrugWarehouseManagement.Repository.Models
{
    public class ReturnOutboundDetails : TimeStamp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReturnOutboundDetailsId { get; set; }
        // Tham chiếu đến OutboundDetail (để biết detail nào bị trả)
        public int OutboundDetailsId { get; set; }
        public int ReturnedQuantity { get; set; } // Số lượng bị trả về
        public string? Note { get; set; } // Ghi chú lý do trả hàng
        public virtual OutboundDetails OutboundDetails { get; set; } = null!;

    }
}
