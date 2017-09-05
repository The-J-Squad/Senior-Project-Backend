using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecipeBackend.Models
{
    public class Recipe
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public int Servings { get; set; }
        public int PrepTime { get; set; } //In Minutes
        public int CookTime { get; set; } //In Minutes
        public IEnumerable<Ingredient> Ingredients { get; set; }
        public IEnumerable<string> Directions { get; set; }
        public bool IsVegan { get; set; }
        public bool IsVegitarian { get; set; }
        public bool IsGlutenFree { get; set; }
        public bool IsKosher { get; set; }
    }
}
