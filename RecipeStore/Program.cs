
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;


/// Create a recipe store based on the following requirements
/// 1. implement basic CRUD operations with the following restrictions
///     - recipeId's will be prefixed with "recipeId" plus a number
///     - this number will start from 1 and increment upwards
///     - For GetRecipe -> return steps and ingredients separated by a comma as 1 string
/// 2. Add the ability to query
///     - return all recipes that have a particular ingredient in them
///     - if there are multiple -> return by desc order or number of ingredients
namespace RecipeStore
{
    public class RecipeStore
    {
        private const string RecipeIdPrefix = "recipeId";
        private const string SbDelimiter = ",";
        /// <summary>
        /// contains all recipes
        /// key - recipeId
        /// Item1 - recipeName
        /// Item2 - ingredients
        /// Item3 - steps
        /// </summary>
        private Dictionary<string, (string, List<string>, List<string>)> recipes;

        /// <summary>
        /// maps recipeName -> recipeId.
        /// </summary>
        private Dictionary<string, string> recipeNameToRecipeId;

        /// <summary>
        /// the recipe Counter that will be used to construct the recipeId. Starts at 1 and increments
        /// </summary>
        private int recipeCounter;

        private Dictionary<string, List<(string, int)>> ingredientToRecipeIdAndIngredientCount;

        RecipeStore()
        {
            this.recipes = new();
            this.recipeNameToRecipeId = new();
            this.ingredientToRecipeIdAndIngredientCount = new();
        }

        // Part 1
        public String? AddRecipe(string name, List<string> ingredients, List<string> steps)
        {
            if(this.recipeNameToRecipeId.ContainsKey(name))
            {
                // duplicate recipe
                return null;
            }

            string key = RecipeIdPrefix + recipeCounter;

            // update datatypes
            this.recipes[key] = (
                name,
                ingredients,
                steps
            );
            this.recipeNameToRecipeId[name] = key;
            this.recipeCounter++;

            return key;
        }

        public List<string> GetRecipe(string recipeId)
        {
            List<string> result = new();

            if(this.recipes.ContainsKey(recipeId))
            {
                return new List<string>();
            }

            //0. Get recipe
            var recipe = this.recipes[recipeId];
            
            //1. Name
            result.Add(recipe.Item1);

            //2. Ingredients
            StringBuilder sbIngredients = new();
            foreach(string ingredients in recipe.Item2)
            {
                sbIngredients.Append(ingredients);
                sbIngredients.Append(SbDelimiter);
            }
            sbIngredients.Length -= 1;
            result.Add(sbIngredients.ToString());

            //3. Steps
            StringBuilder sbSteps = new();
            foreach(string step in recipe.Item3)
            {
                sbSteps.Append(step);
                sbSteps.Append(SbDelimiter);
            }
            sbSteps.Length -= 1;
            result.Add(sbSteps.ToString());

            return result;
        }

        public bool UpdateRecipe(string recipeId, string name, List<string> ingredients, List<string> steps)
        {
            // 0a. check if recipeId exists -> if it does not return false
            if(!this.recipes.ContainsKey(recipeId))
            {
                return false;
            }
            // 0b - recipeId exists so now we need to verify if name update is valid or not
            if(this.recipeNameToRecipeId.ContainsKey(name))
            {
                if(this.recipeNameToRecipeId[name] != this.recipes[recipeId].Item1)
                {
                    // need to prevent duplicate name so return false
                    return false;
                }
            }

            // 1. Now we can update
            string originalRecipeName = this.recipes[recipeId].Item1;

            this.recipes[recipeId] = (
                name,
                ingredients,
                steps
            );

            this.recipeNameToRecipeId.Remove(originalRecipeName);
            this.recipeNameToRecipeId[name] = recipeId;

            return false;
        }
        
        public bool DeleteRecipe(string recipeId)
        {
            if(!this.recipes.ContainsKey(recipeId))
            {
                return false;
            }
            
            string recipeName = this.recipes[recipeId].Item1;

            // delete
            this.recipes.Remove(recipeId);
            this.recipeNameToRecipeId.Remove(recipeName);

            return true;
        }

        // Part 2
        public List<string> SearchByIngredient(string ingredient)
        {
            List<string> result = new();

            // 0. Check if we have any recipes that have this ingredient
            if(!this.ingredientToRecipeIdAndIngredientCount.ContainsKey(ingredient))
            {
                return new List<string>();
            }

            // 1. Get all recipes and return ordering in desc order by number of ingredients
            var recipes = this.ingredientToRecipeIdAndIngredientCount[ingredient];

            result = (List<string>)recipes.OrderByDescending(x => x.Item2).Select(x => x.Item1);

            return result;
        }
    }
}