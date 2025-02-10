namespace DrugWarehouseManagement.Service.DTO.Response
{
    public class SetupTwoFactorAuthenticatorResponse
    {
        public string ImageUrlQrCode { get; set; } = null!;
        public string ManualEntryKey { get; set; } = null!;
    }
}
