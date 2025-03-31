using NodaTime;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public abstract class QueryPaging
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public string? DateFrom { get; set; } // ISO string format only
        public string? DateTo { get; set; } // ISO string format only
    }
}
