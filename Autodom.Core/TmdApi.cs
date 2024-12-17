// See https://aka.ms/new-console-template for more information

using Autodom.Core.Dtos;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Autodom.Core
{

    public interface ITmdApi
    {
        Task LoginAsync(int user, string pass);

        Task<AccountBalanceDto> GetAccountBalanceAsync();
    }

    public class TmdApi : IDisposable, ITmdApi
    {
        private readonly HttpClient _httpClient = new();
        private string? _token;
        private readonly int _year = DateTime.Now.Year;
        private readonly ILogger _logger;

        public TmdApi(ILogger<TmdApi> logger)
        {
            _logger = logger;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        public async Task LoginAsync(int user, string pass)
        {
            var response = await _httpClient.PostAsync(new Uri("https://main.tomojdom.pl/login/OsLogInPass"), StringContent(new { User = user, Pass = pass }));

            var json = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Received login response: {loginResponseJson}", json);
            var data = JsonSerializer.Deserialize<object[]>(json);

            _token = data[2].ToString();

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");
        }

        public async Task<AccountBalanceDto> GetAccountBalanceAsync()
        {
            var response = await _httpClient.PostAsync(new Uri("https://taurus.tomojdom.pl/app/api/Rozliczenia"), new StringContent("15"));
            
            var json = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Got balance response {StatusCode}: {BalanceResponseJson}", response.StatusCode, json);

            var balance = TmdResponseParser.ParseAccountBalanceDto(json);

            return balance;
        }

        private static StringContent StringContent(object o)
        {
            return new StringContent(JsonSerializer.Serialize(o));
        }
    }
}

public class TmdResponseParser
{
    public static AccountBalanceDto ParseAccountBalanceDto(string json)
    {
        var balance = JsonConvert.DeserializeObject<JArray>(json)[0].Value<JArray>()[1].Value<decimal>();

        return new AccountBalanceDto
        {
            Balance = balance
        };
    }
}