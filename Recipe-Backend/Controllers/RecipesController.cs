using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using RecipeBackend.Models;
using System.Net.Http;

namespace RecipeBackend.Controllers
{
    [Route("api/[controller]")]
    public class RecipesController : Controller
    {
        private Recipe[] recipes = new Recipe[]
        {
            new Recipe()
            {
                Name= "Recipe 0",
                Id = 0,
                PrepTime = 30,
                CookTime = 60,
                Directions = new string[]{
                    "Prepare it",
                    "Cook it"
                },
                Servings= 2,
                Ingredients = new Ingredient[]
                {
                    new Ingredient()
                    {
                        Name = "Eggs",
                        Quantity = 3
                    },
                    new Ingredient()
                    {
                        Name = "Flour",
                        Quantity = 4,
                        Unit = "Cups",
                        Comment = "I used White Flour"

                    }
                },
                IsGlutenFree = false,
                IsKosher = true,
                IsVegan = false,
                IsVegitarian = true
            },
            new Recipe()
            {
                Name="Recipe 1",
                Id = 1,
                PrepTime = 15,
                CookTime = 90,
                Directions = new string[]{
                    "Step 1",
                    "Step 2",
                    "Step 3"
                },
                Servings= 50,
                Ingredients = new Ingredient[]
                {
                    new Ingredient()
                    {
                        Name = "Water",
                        Quantity = 3,
                        Unit = "Liters"
                    },
                    new Ingredient()
                    {
                        Name = "Flour",
                        Quantity = 0.25,
                        Unit = "Liters",
                        Comment = "I used whole wheat Flour"

                    },
                    new Ingredient()
                    {
                        Name = "Chicken",
                        Quantity = 3.5,
                        Unit = "kilograms"
                    }
                },
                IsGlutenFree = false,
                IsKosher = true,
                IsVegan = false,
                IsVegitarian = false
            }
        };

        [HttpGet]
        public IActionResult Get()
        {
            //TODO
            return new ObjectResult(recipes);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            //TODO
            if (id < recipes.Length)
            {
                return new ObjectResult(recipes[id]);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        public void Post([FromBody]Recipe recipe)
        {
            //TODO
        }

        [HttpPut("{id}")]
        public void Put(int id, [FromBody]Recipe recipe)
        {
            //TODO
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            //TODO
        }
    }
}
