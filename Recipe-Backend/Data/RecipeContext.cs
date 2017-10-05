using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RecipeBackend.Models;

namespace RecipeBackend.Data
{
    public class RecipeContext
    {
        private readonly IMongoDatabase _database = null;

        public RecipeContext(IOptions<Settings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            if (client != null)
                _database = client.GetDatabase(settings.Value.Database);
        }

        public IMongoCollection<Recipe> Recipes
        {
            get
            {
                return _database.GetCollection<Recipe>("Recipe");
            }
        }
    }
}
