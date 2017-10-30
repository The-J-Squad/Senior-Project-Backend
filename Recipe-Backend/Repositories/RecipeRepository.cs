using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

using MongoDB.Bson;
using RecipeBackend.Data;
using RecipeBackend.Models;

namespace RecipeBackend.Repositories
{
    public class RecipeRepository : IRecipeRepository
    {
        private readonly RecipeContext _context = null;

        public RecipeRepository(IOptions<Settings> settings)
        {
            _context = new RecipeContext(settings);
        }

        public async Task<IEnumerable<Recipe>> GetAllRecipes()
        {
            try
            {
                return await _context.Recipes.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        public async Task<IEnumerable<Recipe>> GetSpecificRecipes(string searchterms)
        {
            try
            {
                return await _context.Recipes.Find(Builders<Recipe>.Filter.Text(searchterms)).ToListAsync();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Recipe> GetRecipe(string id)
        {
            var filter = Builders<Recipe>.Filter.Eq("Id", id);

            try
            {
                return await _context.Recipes
                                .Find(filter)
                                .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        public async Task AddRecipe(Recipe item)
        {
            try
            {
                await _context.Recipes.InsertOneAsync(item);
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        public async Task<DeleteResult> RemoveRecipe(string id)
        {
            try
            {
                return await _context.Recipes.DeleteOneAsync(
                     Builders<Recipe>.Filter.Eq("Id", id));
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        public async Task<ReplaceOneResult> UpdateRecipe(string id, Recipe item)
        {
            try
            {
                return await _context.Recipes
                            .ReplaceOneAsync(n => n.Id.Equals(id)
                                            , item
                                            , new UpdateOptions { IsUpsert = true });
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        public async Task<DeleteResult> RemoveAllRecipes()
        {
            try
            {
                return await _context.Recipes.DeleteManyAsync(new BsonDocument());
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }
    }
}
    