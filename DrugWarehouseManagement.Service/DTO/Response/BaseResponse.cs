using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class BaseResponse
    {
        public int Code { get; set; }
        public string Message { get; set; } = null!;
        public string? Details { get; set; }
    }
}
