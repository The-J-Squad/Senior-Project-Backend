namespace RecipeBackend.Models
{
    public class Settings
    {
        public string ConnectionString { get; set; } = "mongodb://127.0.0.1:27017";
        public string Database { get; set; } = "RecipesDb";
    }
}
