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

    [Function("AutodomTestFunction")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req, ExecutionContext context)
    {
        try
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: false).AddEnvironmentVariables()
                .Build();

            var user = config.GetValue<int>("TMD_USER");
            var pass = config.GetValue<string>("TMD_PASS")!;
            _logger.LogInformation("User: {User} Pass: {Pass}", user, pass);
            var tmdApi = new TmdApi(user, pass, _logger);

            await tmdApi.LoginAsync();


            return new OkResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in function: {message}", ex.Message);

            return new ObjectResult(new { Type = ex.GetType().FullName, ex.Message, ex.StackTrace})
            {
                StatusCode = 500
            };
        }
    }

    [Function(nameof(CheckAccountBalanceForChangesAndNotifyOwner))]
    public async Task CheckAccountBalanceForChangesAndNotifyOwner(
        [QueueTrigger("autodom-check-triggers-queue", Connection = "CheckTriggersQueueConnection")] AccountCheckTriggerDto trigger)
    {
        _logger.LogInformation("Received trigger: {Trigger}", trigger);
        await using var connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString"));

        var latest = await connection.QueryFirstAsync<AccountBalanceDto>(
            "SELECT * FROM dbo.AccountBalances WHERE AccountId = @AccountId ORDER BY [DateTime] DESC",
            new { AccountId = trigger.Id });

        _logger.LogInformation("Found latest account balance: {Balance}", latest);
    }

    [Function(nameof(TestQueueTrigger))]
    [QueueOutput("autodom-check-triggers-queue", Connection = "CheckTriggersQueueConnection")]
    public async Task<AccountCheckTriggerDto[]> TestQueueTrigger(
        [QueueTrigger("autodom-check-triggers-queue-test", Connection = "CheckTriggersQueueConnection")] string trigger)
    {
        _logger.LogInformation("Test trigger: {TestTrigger}", trigger);
        await using var connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString"));
        var accounts = await connection.QueryAsync<AccountCheckTriggerDto>("select * from dbo.Accounts");
        return accounts.ToArray();
    }

    public async Task CronTrigger()
    {
        await TriggerChecksAsync();
    }

    private async Task TriggerChecksAsync()
    {
        throw new NotImplementedException();
    }
}