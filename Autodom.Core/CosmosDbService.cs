using Autodom.Core.Dtos;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodom.Core
{
    public class AccountInfo
    {
        public int Id { get; set; }
        public List<AccountCheckTriggerDto> Apartments { get; set; }
    }


    public class CosmosDbService
    {
        private readonly string _dbConnectionString;

        public CosmosDbService(string dbConnectionString)
        {
            _dbConnectionString = dbConnectionString;
        }

        public async Task<AccountCheckTriggerDto[]> GetApartmentsAsync()
        {
            var cl = new CosmosClient(_dbConnectionString);
            var db = cl.GetDatabase("autodom-cosmosdb");
            var co = db.GetContainer("account-info");
            var accountInfo = await co.ReadItemAsync<AccountInfo>("1", new PartitionKey("1"));

            return accountInfo.Resource.Apartments.ToArray();
        }

        public async Task<AccountBalanceDto> GetLatestAccountBalanceAsync(string accountId)
        {
            var cl = new CosmosClient(_dbConnectionString);
            var query = new QueryDefinition($"select * from a where a.accountId = \"{accountId}\" order by a.lastChangedDateTime desc");
            var accountBalanceDto = await cl.GetDatabase("autodom-cosmosdb")
                .GetContainer("account-balances")
                .GetItemQueryIterator<AccountBalanceDto>(query)
                .ReadNextAsync();

            return accountBalanceDto.Resource.First();
        }

        public async Task SaveCurrentAccountBalanceAsync(AccountBalanceDto current)
        {
            var id = Guid.NewGuid().ToString();
            var cl = new CosmosClient(_dbConnectionString);
            await cl.GetDatabase("autodom-cosmosdb")
            .GetContainer("account-balances")
                .CreateItemAsync(current with { Id = id }, new PartitionKey(id));
        }
    }
}
