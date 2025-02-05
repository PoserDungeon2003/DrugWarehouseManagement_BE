using Google.Authenticator;

namespace DrugWarehouseManagement.Service.Wrapper.Interface
{
    public interface ITwoFactorAuthenticatorWrapper
    {
        bool ValidateTwoFactorPIN(byte[] secretKey, string code, int timeStep);
        SetupCode GenerateSetupCode(string accountTitle, string email, byte[] secretKey);
    }

}
