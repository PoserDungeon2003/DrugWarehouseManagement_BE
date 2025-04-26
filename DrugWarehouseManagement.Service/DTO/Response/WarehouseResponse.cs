namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class WarehouseResponse
    {
        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; } = null!;
        public string Address { get; set; } = null!;
        public int Status { get; set; }
        public string DocumentNumber { get; set; } = null!;
    }
}
