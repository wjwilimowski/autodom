﻿// See https://aka.ms/new-console-template for more information

using System.Text.Json;

namespace Autodom.Core
{
    public class TmdApi : IDisposable
    {
        private readonly HttpClient _httpClient = new();
        private string? _token;
        private readonly int _user;
        private readonly string _pass;
        private readonly int _year = DateTime.Now.Year;

        public TmdApi(int user, string pass)
        {
            _user = user;
            _pass = pass;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        public async Task LoginAsync()
        {
            var response = await _httpClient.PostAsync(new Uri("https://main.tomojdom.pl/login/OsLogInPass"), StringContent(new { User = _user, Pass = _pass }));

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<object[]>(json);

            _token = data[2].ToString();

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");
        }

        public async Task<List<BillDto>> GetBillsAsync()
        {
            var response = await _httpClient.PostAsync(new Uri("https://taurus.tomojdom.pl/app/api/RozliczeniaSzczegolowe"), StringContent(new { WId = 15, Rok = _year }));

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<object[][]>(json)!.Select(Parse).Where(x => x != null).OfType<BillDto>().ToList();
        }

        public async Task<Stream> GetPrintoutAsync(BillDto pdf)
        {
            var response = await _httpClient.PostAsync(new Uri("https://taurus.tomojdom.pl/app/api/WydrukObciazenia"), StringContent(new { NTId = pdf.Id, Rok = _year, WId = 15 }));
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