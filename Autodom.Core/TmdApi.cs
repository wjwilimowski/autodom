// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Autodom.Core
{
    public class TmdApi : IDisposable
    {
        private readonly HttpClient _httpClient = new();
        private string? _token;
        private readonly int _user;
        private readonly string _pass;
        private readonly int _year = DateTime.Now.Year;
        private readonly ILogger _logger;

        public TmdApi(int user, string pass, ILogger logger)
        {
            _user = user;
            _pass = pass;
            _logger = logger;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        public async Task LoginAsync()
        {
            var response = await _httpClient.PostAsync(new Uri("https://main.tomojdom.pl/login/OsLogInPass"), StringContent(new { User = _user, Pass = _pass }));

            var json = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Received login response: {loginResponseJson}", json);
            var data = JsonSerializer.Deserialize<object[]>(json);

            _token = data[2].ToString();

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");
        }

        private static StringContent StringContent(object o)
        {
            return new StringContent(JsonSerializer.Serialize(o));
        }
    }
}