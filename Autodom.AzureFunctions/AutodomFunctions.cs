using Autodom.Core;
using Autodom.Core.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Autodom.AzureFunctions
{
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
                var sendgridApiKey = config.GetValue<string>("SENDGRID_API_KEY")!;
                _logger.LogInformation("User: {User} Pass: {Pass}", user, pass);
                var tmdApi = new TmdApi(user, pass, _logger);

                await tmdApi.LoginAsync();

                var sender = new MailSender([], sendgridApiKey, _logger);
                await sender.SendAsync("wjwilimowski@gmail.com", "test");
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
        public async Task CheckAccountBalanceForChangesAndNotifyOwner([QueueTrigger("autodom-check-triggers-queue", Connection = "CheckTriggersQueueConnection")] AccountCheckTriggerDto trigger)
        {
            _logger.LogInformation("Received trigger: {Trigger}", trigger);
            await Task.CompletedTask;
        }

        [Function("TestHttpTrigger")]
        public async Task<IActionResult> TestHttpTrigger(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req, ExecutionContext context)
        {
            await TriggerChecksAsync();

            return new OkResult();
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
}
