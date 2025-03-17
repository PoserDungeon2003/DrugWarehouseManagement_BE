namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class AccountLoginResponse
    {
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}
