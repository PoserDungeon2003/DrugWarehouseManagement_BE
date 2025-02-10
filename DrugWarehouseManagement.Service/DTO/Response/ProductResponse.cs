namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class ProductResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string ProductCode { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string MadeFrom { get; set; } = null!;
        public int ProviderId { get; set; }
    }
}
