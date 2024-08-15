// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using System.Text.Json;

var configuration = new ConfigurationBuilder()
                .AddJsonFile("secrets.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

var user = int.Parse(configuration["TMD_USER"]!);
var pass = configuration["TMD_PASS"]!;

if (Directory.Exists("out"))
{
    Directory.Delete("out", recursive: true);
}
Directory.CreateDirectory("out");

using var api = new TmdApi(user, pass);
await api.LoginAsync();
var pdfs = await api.GetMonthlyPdfsAsync();
foreach (var item in pdfs.Where(x => x != null))
{
    Console.WriteLine(item);
    var path = "out\\" + item.Title.Replace('/', '-') + ".pdf";
    using var printout = await api.GetPrintoutAsync(item);
    using var fs = File.Create(path);
    await printout.CopyToAsync(fs);
}

class TmdApi : IDisposable
{
    private readonly HttpClient httpClient = new();
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
        httpClient.Dispose();
    }

    public async Task LoginAsync()
    {
        var response = await httpClient.PostAsync(new Uri("https://main.tomojdom.pl/login/OsLogInPass"), StringContent(new { User = _user, Pass = _pass}));

        var json = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<object[]>(json);

        _token = data[2].ToString();

        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_token}");
    }

    public async Task<List<MonthlyPdf>> GetMonthlyPdfsAsync()
    {
        var response = await httpClient.PostAsync(new Uri("https://taurus.tomojdom.pl/app/api/RozliczeniaSzczegolowe"), StringContent(new { WId = 15, Rok = _year}));

        var json = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<object[][]>(json)!.Select(Parse).ToList();
    }

    public async Task<Stream> GetPrintoutAsync(MonthlyPdf pdf)
    {
        var response = await httpClient.PostAsync(new Uri("https://taurus.tomojdom.pl/app/api/WydrukObciazenia"), StringContent(new { NTId = pdf.Id, Rok = _year, WId = 15 }));
        return await response.Content.ReadAsStreamAsync();
    }

    private static StringContent StringContent(object o)
    {
        return new StringContent(JsonSerializer.Serialize(o));
    }

    private static MonthlyPdf Parse(object[] item)
    {
        var mid = item[0].ToString()!;
        var month = DateTime.Parse(item[1].ToString()!);
        if (month.Year == 1000)
        {
            return null!;
        }
        var innerItem = ((JsonElement)item[2])![0];
        var date = DateTime.Parse(innerItem![0].ToString()!);
        var title = innerItem[1].ToString()!;
        var amount = innerItem[2].GetDecimal();
        var id = innerItem[3].GetInt32();
        
        return new MonthlyPdf
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

record MonthlyPdf
{
    public required DateTime Month { get; init; }
    public required DateTime Date { get; init; }
    public required string Title { get; init; }
    public required string MId { get; init; }
    public required int Id { get; init; }
    public required decimal Amount { get; init; }
}