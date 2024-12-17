using Autodom.Core.Dtos;
using Microsoft.Extensions.Logging;

namespace Autodom.Core
{
    public class AutodomService
    {
        private readonly ITmdApi _api;
        private readonly IMailSender _mailSender;
        private readonly CosmosDbService _cosmosDbService;
        private readonly ILogger<AutodomService> _logger;

        public AutodomService(ITmdApi api, IMailSender mailSender, CosmosDbService cosmosDbService, ILogger<AutodomService> logger)
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

            var latest = await _cosmosDbService.GetLatestAccountBalanceAsync(trigger.Id);

            _logger.LogInformation("Found latest account balance: {Balance}", latest);

            if (latest.Balance != current.Balance)
            {
                await _mailSender.SendAsync(trigger.Email, $"Tomojdom.pl - zmiana salda ({trigger.ApartmentName})",
                    $"Było: {latest.Balance}, jest: {current.Balance}");

                await _cosmosDbService.SaveCurrentAccountBalanceAsync(current);
            }
        }
    }
}
