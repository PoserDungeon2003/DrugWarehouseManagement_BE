namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class BaseResponse
    {
        public int Code { get; set; }
        public string Message { get; set; } = null!;
        public string? Details { get; set; }
        public object? Result { get; set; }
    }
}
