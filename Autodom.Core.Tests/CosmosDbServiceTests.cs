using Autodom.Core.Dtos;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Autodom.Core.Tests
{
    public class CosmosDbServiceTests
    {
        private readonly CosmosDbService _sut;

        public CosmosDbServiceTests()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddUserSecrets<CosmosDbServiceTests>();

            var dbc = configurationBuilder.Build()["CosmosDbConnectionString"];

            _sut = new(dbc);

        }

        [Fact]
        public async Task GetAccounts()
        {
            await _sut.GetApartmentsAsync();
        }

        [Fact]
        public async Task GetLatestAccountBalanceAsync()
        {
            var ab = await _sut.GetLatestAccountBalanceAsync("1");
            Assert.NotNull(ab);
        }

        [Fact]
        public async Task SaveCurrentAccountBalanceAsync()
        {
            var b = new AccountBalanceDto { AccountId = "Test" + Guid.NewGuid(), Balance = 600m, LastChangedDateTime = DateTime.UtcNow };
            await _sut.SaveCurrentAccountBalanceAsync(b);
        }
    }
}
