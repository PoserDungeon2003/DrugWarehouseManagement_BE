using DrugWarehouseManagement.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class ViewNotification
    {
        public int NotificationId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool IsRead { get; set; }
    }
}
