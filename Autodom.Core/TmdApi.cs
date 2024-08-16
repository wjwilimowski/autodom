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

        public async Task<List<BillDto>> GetBillsAsync()
        {
            var response = await _httpClient.PostAsync(new Uri("https://taurus.tomojdom.pl/app/api/RozliczeniaSzczegolowe"), StringContent(new { WId = 15, Rok = _year }));

            var json = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Received bills response: {billsResponseJson}", json);

            return JsonSerializer.Deserialize<object[][]>(json)!.Select(Parse).Where(x => x != null).OfType<BillDto>().ToList();
        }

        public async Task<Stream> GetPdfAsStreamAsync(BillDto bill)
        {
            var response = await _httpClient.PostAsync(new Uri("https://taurus.tomojdom.pl/app/api/WydrukObciazenia"), StringContent(new { NTId = bill.Id, Rok = _year, WId = 15 }));
            return await response.Content.ReadAsStreamAsync();
        }

        private static StringContent StringContent(object o)
        {
            return new StringContent(JsonSerializer.Serialize(o));
        }

        private static BillDto? Parse(object[] item)
        {
            var mid = item[0].ToString()!;
            var month = DateTime.Parse(item[1].ToString()!);
            if (month.Year == 1000)
            {
                return null;
            }
            var innerItem = ((JsonElement)item[2])![0];
            var date = DateTime.Parse(innerItem![0].ToString()!);
            var title = innerItem[1].ToString()!;
            var amount = innerItem[2].GetDecimal();
            var id = innerItem[3].GetInt32();

            return new BillDto
            {
                Id = id,
                MId = mid,
                Month = month,
                Date = date,
                Amount = amount,
                Title = title
            };
        }
    }
}