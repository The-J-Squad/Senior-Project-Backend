using MongoDB.Driver;
using RecipeBackend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecipeBackend.Repositories
{
    public interface IRecipeRepository
    {
        Task<IEnumerable<Recipe>> GetAllRecipes();
        Task<IEnumerable<Recipe>> GetSpecificRecipes(string searchterms);
        Task<Recipe> GetRecipe(string id);
        Task AddRecipe(Recipe item);
        Task<DeleteResult> RemoveRecipe(string id);

        // demo interface - full document update
        Task<ReplaceOneResult> UpdateRecipe(string id, Recipe recipe);

        // should be used with high cautious, only in relation with demo setup
        Task<DeleteResult> RemoveAllRecipes();
    }
}
