namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class AccountLoginResponse
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public string? Role { get; set; }
        public bool RequiresTwoFactor { get; set; }
    }
}
