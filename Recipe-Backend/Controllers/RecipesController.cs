using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using RecipeBackend.Models;
using RecipeBackend.Repositories;
using System.Threading.Tasks;
using MongoDB.Driver;
using System;
using Microsoft.AspNetCore.Authorization;

namespace RecipeBackend.Controllers
{
    [Authorize(Policy = "Member")]
    [Route("api/recipes")]
    public class RecipesController : Controller
    {
        private readonly IRecipeRepository _recipeRepository;

        public RecipesController(IRecipeRepository recipeRepository)
        {
            _recipeRepository = recipeRepository;
        }

        [HttpGet]
        public Task<IEnumerable<Recipe>> Get()
        {
            return _recipeRepository.GetAllRecipes();
        }

        [HttpGet("search/{searchterms}")]
        public Task<IEnumerable<Recipe>> GetSpecificRecipes(String searchterms)
        {
            return _recipeRepository.GetSpecificRecipes(searchterms);
        }

        [HttpGet("{id}")]
        public Task<Recipe> Get(string id)
        {
            return _recipeRepository.GetRecipe(id);
        }

        [HttpPost]
        public Recipe Post([FromBody]Recipe recipe)
        {
            _recipeRepository.AddRecipe(recipe);
            return recipe;
        }

        [HttpPut("{id}")]
        public Task<ReplaceOneResult> Put(string id, [FromBody]Recipe recipe)
        {
            return _recipeRepository.UpdateRecipe(id, recipe);
        }

        [HttpDelete("{id}")]
        public Task<DeleteResult> Delete(string id)
        {
            return _recipeRepository.RemoveRecipe(id);
        }
    }
}