﻿using DrugWarehouseManagement.Common;
using NodaTime;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class QueryPaging
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public string? DateFrom { get; set; } // ISO string format only
        public string? DateTo { get; set; } // ISO string format only
        public bool ShowInactive { get; set; } = false;
    }

    
    public class LotQueryPaging : QueryPaging
    {
        public int ProductId { get; set; }
        public int ProviderId { get; set; }
        public int WarehouseId { get; set; }
        public bool Availablle { get; set; } = false;
    }

    public class InboundRequestQueryPaging : QueryPaging
    {
        public InboundRequestStatus InboundRequestStatus { get; set; }
    }

    public class InboundtQueryPaging : QueryPaging
    {
        public InboundStatus InboundStatus { get; set; }
        public bool IsReportPendingExist { get; set; } = false;
    }
}
