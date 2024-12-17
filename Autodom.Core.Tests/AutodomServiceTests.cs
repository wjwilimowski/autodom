using Autodom.Core.Dtos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodom.Core.Tests
{
    public class AutodomServiceTests
    {
        private readonly AutodomService _sut;
        private readonly Mock<IMailSender> _mailSender = new();
        private readonly Mock<ITmdApi> _tmdApi = new();
        
        private readonly CosmosDbService _cosmosDbService;

        private readonly string _id = "AutodomServiceTests_" + Guid.NewGuid();

        public AutodomServiceTests()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddUserSecrets<AutodomServiceTests>();

            var config = configurationBuilder.Build();
            var dbc = config["CosmosDbConnectionString"];

            _cosmosDbService = new CosmosDbService(dbc);

            _sut = new(
                _tmdApi.Object,
                _mailSender.Object,
                _cosmosDbService,
                NullLogger<AutodomService>.Instance);
        }

        [Fact]
        public async Task Test()
        {
            var firstAccountBalance = new AccountBalanceDto
            {
                Balance = 100,
                AccountId = _id,
                LastChangedDateTime = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString()
            };
            var secondAccountBalance = new AccountBalanceDto
            {
                Balance = -150,
                AccountId = _id
            };
            var trigger = new AccountCheckTriggerDto
            {
                ApartmentName = "9c/9",
                Email = "test@example.com",
                Id = _id,
                User = 12414,
                Pass = "pass"
            };
            await _cosmosDbService.SaveCurrentAccountBalanceAsync(firstAccountBalance);

            _tmdApi.Setup(s => s.GetAccountBalanceAsync()).ReturnsAsync(firstAccountBalance);
            await _sut.GoAsync(trigger);

            _mailSender.Verify(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);

            _tmdApi.Setup(s => s.GetAccountBalanceAsync()).ReturnsAsync(firstAccountBalance   );
            await _sut.GoAsync(trigger);

            _mailSender.Verify(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            await _sut.GoAsync(trigger);

            _mailSender.Verify(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
