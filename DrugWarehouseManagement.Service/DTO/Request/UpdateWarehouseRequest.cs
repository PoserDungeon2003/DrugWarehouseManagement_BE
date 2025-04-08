namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class UpdateWarehouseRequest
    {
        public string? WarehouseName { get; set; }

        public string? Address { get; set; }
        public string? DocumentNumber { get; set; }
        public string? WarehouseCode { get; set; }
        public string? Status { get; set; }

    }
}
