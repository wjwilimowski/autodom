using Autodom.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Autodom.AzureFunctions
{
    public class AutodomTestFunction
    {
        private readonly ILogger<AutodomTestFunction> _logger;

        public AutodomTestFunction(ILogger<AutodomTestFunction> logger)
        {
            _logger = logger;
        }

        [Function("AutodomTestFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req, ExecutionContext context)
        {
            var config = new ConfigurationBuilder().AddJsonFile("local.settings.json", optional: true, reloadOnChange: false).AddEnvironmentVariables().Build();
            var user = config.GetValue<int>("Values:TMD_USER");
            var pass = config.GetValue<string>("Values:TMD_PASS")!;
            var tmdApi = new TmdApi(user, pass);

            await tmdApi.LoginAsync();

            var bills = await tmdApi.GetBillsAsync();
            foreach (var bill in bills)
            {
                _logger.LogInformation("{Bill}", bill.ToString());
            }

            return new OkObjectResult(new { Bills = bills });
        }
    }
}
