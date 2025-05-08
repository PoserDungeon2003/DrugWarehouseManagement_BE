using DrugWarehouseManagement.Service.Wrapper.Interface;
using Google.Authenticator;

namespace DrugWarehouseManagement.Service.Wrapper
{
    public class TwoFactorAuthenticatorWrapper : ITwoFactorAuthenticatorWrapper
    {
        private readonly TwoFactorAuthenticator _twoFactorAuthenticator;

        public TwoFactorAuthenticatorWrapper()
        {
            _twoFactorAuthenticator = new TwoFactorAuthenticator();
        }

        public SetupCode GenerateSetupCode(string issuer, string accountTitleNoSpaces, byte[] secretKey)
            => _twoFactorAuthenticator.GenerateSetupCode(issuer, accountTitleNoSpaces.Trim(), secretKey);

        public bool ValidateTwoFactorPIN(byte[] secretKey, string code, int timeStep)
            => _twoFactorAuthenticator.ValidateTwoFactorPIN(secretKey, code, timeStep);
    }
}
