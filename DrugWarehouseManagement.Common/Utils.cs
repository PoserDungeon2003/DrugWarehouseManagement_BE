using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;

namespace DrugWarehouseManagement.Common
{
    public static class Utils
    {
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string GenerateRandomPassword(int length = 8)
        {
            const string upperCaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowerCaseLetters = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            //const string specialCharacters = "!@#$%^&*()-_+=<>?";

            // Combine all character groups
            string allCharacters = upperCaseLetters + lowerCaseLetters + digits;
            //+ specialCharacters;

            var random = new Random();
            var passwordBuilder = new StringBuilder();

            // Ensure the password includes at least one character from each group
            passwordBuilder.Append(upperCaseLetters[random.Next(upperCaseLetters.Length)]);
            passwordBuilder.Append(lowerCaseLetters[random.Next(lowerCaseLetters.Length)]);
            passwordBuilder.Append(digits[random.Next(digits.Length)]);
            //passwordBuilder.Append(specialCharacters[random.Next(specialCharacters.Length)]);

            // Fill the remaining length with random characters from all groups
            for (int i = 4; i < length; i++)
            {
                passwordBuilder.Append(allCharacters[random.Next(allCharacters.Length)]);
            }

            // Shuffle the characters to ensure randomness
            var passwordArray = passwordBuilder.ToString().ToCharArray();
            for (int i = passwordArray.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (passwordArray[i], passwordArray[j]) = (passwordArray[j], passwordArray[i]);
            }

            return new string(passwordArray);
        }

        public static string Generate2FABackupCode(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            StringBuilder result = new StringBuilder();
            byte[] byteBuffer = new byte[length];

            RandomNumberGenerator.Fill(byteBuffer);

            for (int i = 0; i < length; i++)
            {
                result.Append(chars[byteBuffer[i] % chars.Length]);
            }

            return result.ToString();
        }

        public static string BuildDownloadAssetUrl(string prefix, string id, string type, string fileName)
        {
            // prefix: inbound, outbound, etc...
            // type: report, request, image, etc...
            // file name can be mapped to folder. Ex: folder/file.pdf
            // Ex: inbound/1/report/[folder-name]/1.pdf
            return $"{prefix}/{id}/{type}/{fileName}";
        }

        public static string FormatDateOnly(DateOnly? date)
        {
            return date?.ToString("dd/MM/yyyy") ?? "N/A";
        }
    }
}
