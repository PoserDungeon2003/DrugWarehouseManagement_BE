using DrugWarehouseManagement.Common;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class ViewInboundReport
    {
        public int InboundReportId { get; set; }
        public string ProblemDescription { get; set; }
        public string Status { get; set; }
        public string ReportDate { get; set; }
        public List<AssetResponse> Assets { get; set; }
    }
}
