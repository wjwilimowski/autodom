using System.Data;
using Autodom.Core;
using Autodom.Core.Dtos;
using Azure.Identity;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Sql;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Autodom.AzureFunctions;

public class AutodomFunctions
{
    private readonly ILogger<AutodomFunctions> _logger;
    private readonly CosmosDbService _cosmosDbService;
    private readonly AutodomService _autodomService;
    private readonly TmdApi _tmdApi;

    public AutodomFunctions(ILogger<AutodomFunctions> logger, CosmosDbService cosmosDbService, AutodomService autodomService, TmdApi tmdApi)
    {
        _logger = logger;
        _cosmosDbService = cosmosDbService;
        _autodomService = autodomService;
        _tmdApi = tmdApi;
    }


    [Function(nameof(CheckAccountBalanceForChangesAndNotifyOwner))]
    public async Task CheckAccountBalanceForChangesAndNotifyOwner(
        [QueueTrigger("autodom-check-triggers-queue", Connection = "CheckTriggersQueueConnection")] AccountCheckTriggerDto trigger)
    {
        _logger.LogInformation("Received trigger: {Trigger}", trigger);

        await _autodomService.GoAsync(trigger);
    }

    // SELECT * FROM c WHERE c.accountId = 1 ORDER BY c.lastChangedDateTime DESC
    // SELECT * from c

    [Function(nameof(TestQueueTrigger))]
    [QueueOutput("autodom-check-triggers-queue", Connection = "CheckTriggersQueueConnection")]
    public async Task<AccountCheckTriggerDto[]> TestQueueTrigger(
        [QueueTrigger("autodom-check-triggers-queue-test", Connection = "CheckTriggersQueueConnection")] string trigger)
    {
        _logger.LogInformation("Test trigger: {TestTrigger}", trigger);
        return await GetApartmentsAsync();
    }

    [Function(nameof(CronTrigger))]
    [QueueOutput("autodom-check-triggers-queue", Connection = "CheckTriggersQueueConnection")]
    public async Task<AccountCheckTriggerDto[]> CronTrigger(
        [TimerTrigger("%CronTriggerSchedule%")] TimerInfo timerInfo, FunctionContext context)
    {
        context.GetLogger(nameof(CronTrigger)).LogInformation("Cron trigger");
        return await GetApartmentsAsync();
    }


    private async Task<AccountCheckTriggerDto[]> GetApartmentsAsync()
    {
        return await _cosmosDbService.GetApartmentsAsync();
    }
}