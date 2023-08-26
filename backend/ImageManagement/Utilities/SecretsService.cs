namespace ImageManagement.Utilities
{
    using Amazon.SecretsManager;
    using Amazon.SecretsManager.Model;
    public class SecretsService
    {
        private readonly string _secretName;
        private readonly string _region;

        public SecretsService(string secretName, string region)
        {
            _secretName = secretName;
            _region = region;
        }

        public async Task<string> GetSecretAsync()
        {
            string secret = "";

            var client = new AmazonSecretsManagerClient(Amazon.RegionEndpoint.GetBySystemName(_region));

            var request = new GetSecretValueRequest
            {
                SecretId = _secretName
            };

            GetSecretValueResponse response = null;
            try
            {
                response = await client.GetSecretValueAsync(request);

                if (response.SecretString != null)
                {
                    secret = response.SecretString;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving secret: {ex.Message}");
            }

            return secret;
        }
    }
}
