namespace E2ETests
{
    internal class TestConfigReader
    {
        // TODO: how to avoid relative path here? 
        public const string TEST_CONFIG_FILE = "../../../TestConfig.ini";

        private static Dictionary<string, string> ReadCredentials(string filePath)
        {
            var credentials = new Dictionary<string, string>();
            foreach (var line in File.ReadAllLines(filePath))
            {
                if (line.StartsWith("[") || string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                var parts = line.Split('=');
                if (parts.Length == 2)
                {
                    credentials[parts[0].Trim()] = parts[1].Trim();
                }
            }
            return credentials;
        }

        public static TestCredentials GetCredentials(string filePath)
        {
            var credentialsDict = ReadCredentials(filePath);
            var username = credentialsDict["username"];
            var password = credentialsDict["password"];

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException("Test credentials are not set in TestConfig.ini.");
            }

            return new TestCredentials { Username = username, Password = password };
        }
    }
}
