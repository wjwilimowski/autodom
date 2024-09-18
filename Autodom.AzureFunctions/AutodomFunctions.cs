using System.Data;
using Autodom.Core;
using Autodom.Core.Dtos;
using Azure.Identity;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Sql;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Autodom.AzureFunctions;

public class AutodomFunctions
{
    private readonly ILogger<AutodomFunctions> _logger;

    public AutodomFunctions(ILogger<AutodomFunctions> logger)
    {
        _logger = logger;
    }


    [Function(nameof(CheckAccountBalanceForChangesAndNotifyOwner))]
    public async Task CheckAccountBalanceForChangesAndNotifyOwner(
        [QueueTrigger("autodom-check-triggers-queue", Connection = "CheckTriggersQueueConnection")] AccountCheckTriggerDto trigger)
    {
        _logger.LogInformation("Received trigger: {Trigger}", trigger);

        var config = new ConfigurationBuilder()
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var sendgridApiKey = config.GetValue<string>("SENDGRID_API_KEY")!;

        var mailSender = new MailSender(sendgridApiKey, _logger);

        await using var connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString"));

        var tmdApi = new TmdApi(trigger.User, trigger.Pass, _logger);
        await tmdApi.LoginAsync();


        var current = await tmdApi.GetAccountBalanceAsync() with { AccountId = trigger.Id, LastChangedDateTime = DateTime.Now };

        var latest = await connection.QueryFirstOrDefaultAsync<AccountBalanceDto>(
                         "SELECT * FROM dbo.AccountBalances WHERE AccountId = @AccountId ORDER BY [LastChangedDateTime] DESC",
                         new { AccountId = trigger.Id })
                     ?? new AccountBalanceDto() { Balance = 0m, LastChangedDateTime = DateTime.MinValue };

        _logger.LogInformation("Found latest account balance: {Balance}", latest);

        if (latest.Balance != current.Balance)
        {
            await mailSender.SendAsync(trigger.Email, $"Tomojdom.pl - zmiana salda ({trigger.User})",
                $"Było: {latest.Balance}, jest: {current.Balance}");

            await connection.ExecuteAsync(
                $"INSERT INTO dbo.AccountBalances ([AccountId], [Balance], [LastChangedDateTime]) VALUES ({current.AccountId}, {current.Balance}, '{current.LastChangedDateTime:yyyy-MM-dd}')");
        }
    }

    [Function(nameof(TestQueueTrigger))]
    [QueueOutput("autodom-check-triggers-queue", Connection = "CheckTriggersQueueConnection")]
    public async Task<AccountCheckTriggerDto[]> TestQueueTrigger(
        [QueueTrigger("autodom-check-triggers-queue-test", Connection = "CheckTriggersQueueConnection")] string trigger)
    {
        _logger.LogInformation("Test trigger: {TestTrigger}", trigger);
        return await GetAccountsAsync();
    }

    [Function(nameof(CronTrigger))]
    [QueueOutput("autodom-check-triggers-queue", Connection = "CheckTriggersQueueConnection")]
    public async Task<AccountCheckTriggerDto[]> CronTrigger(
        [TimerTrigger("0 0 6 * * *")] TimerInfo timerInfo, FunctionContext context)
    {
        context.GetLogger(nameof(CronTrigger)).LogInformation("Cron trigger");
        return await GetAccountsAsync();
    }

    private static async Task<AccountCheckTriggerDto[]> GetAccountsAsync()
    {
        await using var connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString"));
        var accounts = await connection.QueryAsync<AccountCheckTriggerDto>("select * from dbo.Accounts");

        return accounts.ToArray();
    }
}