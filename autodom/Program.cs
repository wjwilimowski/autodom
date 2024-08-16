﻿// See https://aka.ms/new-console-template for more information

using Autodom.Core;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
                .AddJsonFile("secrets.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

var user = int.Parse(configuration["TMD_USER"]!);
var pass = configuration["TMD_PASS"]!;
var emailsToNotify = configuration.GetSection("EMAILS").GetChildren().Where(kv => bool.Parse(kv.Value!)).Select(kv => kv.Key).ToList();

var mailSender = new PdfMailSender(emailsToNotify);
using var api = new TmdApi(user, pass);
await api.LoginAsync();
var pdfs = await api.GetMonthlyPdfsAsync();

foreach (var item in pdfs.Where(x => x != null))
{
    Console.WriteLine(item);
    using var printout = await api.GetPrintoutAsync(item);
    using var ms = new MemoryStream();
    await printout.CopyToAsync(ms);
    await mailSender.SendAsync(item, ms.ToArray());
}
