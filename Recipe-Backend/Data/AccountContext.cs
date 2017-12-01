using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RecipeBackend.Models;

namespace RecipeBackend.Data
{
    public class AccountContext
    {
        private readonly IMongoDatabase _database = null;

        public AccountContext(IOptions<Settings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            if (client != null)
                _database = client.GetDatabase(settings.Value.Database);
        }

        public IMongoCollection<AccountInformation> Accounts
        {
            get
            {
                return _database.GetCollection<AccountInformation>("Account");
            }
        }
    }
}
