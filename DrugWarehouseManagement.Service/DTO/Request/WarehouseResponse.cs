namespace DrugWarehouseManagement.Service.DTO.Request
{
    public class WarehouseResponse
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = null!;
        public string Address { get; set; } = null!;
        public int Status { get; set; }
    }
}
