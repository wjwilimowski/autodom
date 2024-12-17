using Autodom.Core.Dtos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodom.Core
{
    public class AutodomService
    {
        private readonly TmdApi _api;
        private readonly IMailSender _mailSender;
        private readonly CosmosDbService _cosmosDbService;
        private readonly ILogger<AutodomService> _logger;

        public AutodomService(TmdApi api, IMailSender mailSender, CosmosDbService cosmosDbService, ILogger<AutodomService> logger)
        {
            _api = api;
            _mailSender = mailSender;
            _cosmosDbService = cosmosDbService;
            _logger = logger;
        }

        public async Task GoAsync(AccountCheckTriggerDto trigger)
        {
            await _api.LoginAsync(trigger.User, trigger.Pass);

            var current = await _api.GetAccountBalanceAsync() with { AccountId = trigger.Id, LastChangedDateTime = DateTime.Now };

            var latest = await GetLatestAccountBalanceAsync(trigger.Id);

            _logger.LogInformation("Found latest account balance: {Balance}", latest);

            if (latest.Balance != current.Balance)
            {
                await _mailSender.SendAsync(trigger.Email, $"Tomojdom.pl - zmiana salda ({trigger.ApartmentName})",
                    $"Było: {latest.Balance}, jest: {current.Balance}");

                await SaveCurrentAccountBalanceAsync(current);
            }
        }

        private async Task<AccountBalanceDto> GetLatestAccountBalanceAsync(string accountId)
        {
            return await _cosmosDbService.GetLatestAccountBalanceAsync(accountId);
        }

        private async Task SaveCurrentAccountBalanceAsync(AccountBalanceDto current)
        {
            await _cosmosDbService.SaveCurrentAccountBalanceAsync(current);
        }
    }
}
