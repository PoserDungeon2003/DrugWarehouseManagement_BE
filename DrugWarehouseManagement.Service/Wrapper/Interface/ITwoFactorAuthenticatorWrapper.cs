using Google.Authenticator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugWarehouseManagement.Service.Wrapper.Interface
{
    public interface ITwoFactorAuthenticatorWrapper
    {
        bool ValidateTwoFactorPIN(byte[] secretKey, string code, int timeStep);
        SetupCode GenerateSetupCode(string accountTitle, string email, byte[] secretKey);
    }

}
