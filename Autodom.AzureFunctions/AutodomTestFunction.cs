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
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
