using NodaTime;

namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class QueryPaging
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }

        public Instant? DateFrom { get; set; }
        public Instant? DateTo { get; set; }
    }
}
