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

        var mailSender = new MailSender(_logger);

        var tmdApi = new TmdApi(trigger.User, trigger.Pass, _logger);
        await tmdApi.LoginAsync();

        var current = await tmdApi.GetAccountBalanceAsync() with { AccountId = trigger.Id, LastChangedDateTime = DateTime.Now };

        var latest = await GetLatestAccountBalanceAsync(trigger.Id);

        _logger.LogInformation("Found latest account balance: {Balance}", latest);

        if (latest.Balance != current.Balance)
        {
            await mailSender.SendAsync(trigger.Email, $"Tomojdom.pl - zmiana salda ({trigger.ApartmentName})",
                $"Było: {latest.Balance}, jest: {current.Balance}");

            await SaveCurrentAccountBalanceAsync(current);
        }
    }

    // SELECT * FROM c WHERE c.accountId = 1 ORDER BY c.lastChangedDateTime DESC
    // SELECT * from c

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
        [TimerTrigger("%CronTriggerSchedule%")] TimerInfo timerInfo, FunctionContext context)
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

    private static async Task<AccountBalanceDto> GetLatestAccountBalanceAsync(int accountId) 
    {
        await using var connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString"));

        var latest = await connection.QueryFirstOrDefaultAsync<AccountBalanceDto>(
                         "SELECT * FROM dbo.AccountBalances WHERE AccountId = @AccountId ORDER BY [LastChangedDateTime] DESC",
                         new { AccountId = accountId })
                     ?? new AccountBalanceDto() { Balance = 0m, LastChangedDateTime = DateTime.MinValue };
    }

    private static async Task SaveCurrentAccountBalanceAsync(AccountBalanceDto current)
    {
        await using var connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString"));

        await connection.ExecuteAsync(
                $"INSERT INTO dbo.AccountBalances ([AccountId], [Balance], [LastChangedDateTime]) VALUES ({current.AccountId}, {current.Balance}, '{current.LastChangedDateTime:yyyy-MM-dd}')");
    }
}