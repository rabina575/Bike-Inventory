using System.Security.Cryptography;

namespace BikeServicing.Data
{
    public static class Utils
    {
        private const char _segmentDelimiter = ':';

        // To create a password hash
        public static string HashSecret(string input)
        {
            var saltSize = 16;
            var iterations = 100_000;
            var keySize = 32;
            HashAlgorithmName algorithm = HashAlgorithmName.SHA256;
            byte[] salt = RandomNumberGenerator.GetBytes(saltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(input, salt, iterations, algorithm, keySize);

            return string.Join(
                _segmentDelimiter,
                Convert.ToHexString(hash),
                Convert.ToHexString(salt),
                iterations,
                algorithm
            );
        }

        // To verify password.
        public static bool VerifyHash(string input, string hashString)
        {
            string[] segments = hashString.Split(_segmentDelimiter);
            byte[] hash = Convert.FromHexString(segments[0]);
            byte[] salt = Convert.FromHexString(segments[1]);
            int iterations = int.Parse(segments[2]);
            HashAlgorithmName algorithm = new(segments[3]);
            byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(
                input,
                salt,
                iterations,
                algorithm,
                hash.Length
            );

            return CryptographicOperations.FixedTimeEquals(inputHash, hash);
        }

        // To create a directory for json files
        public static string GetAppDirectoryPath()
        {
            return @"D:\College\3rdYrCw\App\BikeServicing\BikeServicing\wwwroot\json";
        }

        public static string GetAppUsersFilePath()
        {
            return @"D:\College\3rdYrCw\App\BikeServicing\BikeServicing\wwwroot\json\Users.json";
        }
        public static string GetItemsFilePath()
        {
            return @"D:\College\3rdYrCw\App\BikeServicing\BikeServicing\wwwroot\json\Items.json";
        }

        public static string GetItemsRecordFilePath()
        {
            return @"D:\College\3rdYrCw\App\BikeServicing\BikeServicing\wwwroot\json\Records.json";
        }
    }
}
