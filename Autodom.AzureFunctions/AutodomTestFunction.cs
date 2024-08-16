using Autodom.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
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
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            var user = int.Parse(Environment.GetEnvironmentVariable("TMD_USER")!);
            var pass = Environment.GetEnvironmentVariable("TMD_PASS")!;
            var tmdApi = new TmdApi(user, pass);

            await tmdApi.LoginAsync();

            var bills = await tmdApi.GetMonthlyPdfsAsync();
            foreach (var bill in bills)
            {
                _logger.LogInformation("{Bill}", bill.ToString());
            }

            return new OkObjectResult(new { Bills = bills });
        }
    }
}
