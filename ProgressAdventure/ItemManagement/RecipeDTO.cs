namespace ProgressAdventure.ItemManagement
{
    /// <summary>
    /// DTO used for storing a recipe for an item, and how many items that recipe creates.
    /// </summary>
    public class RecipeDTO
    {
        /// <summary>
        /// The list of items required to complete the recipe.
        /// </summary>
        public readonly List<IngredientDTO> ingredients;
        /// <summary>
        /// The amount of items that get created when completing this recipe.
        /// </summary>
        public readonly double resultAmount;

        /// <summary>
        /// <inheritdoc cref="RecipeDTO"/>
        /// </summary>
        /// <param name="ingredients"><inheritdoc cref="ingredients" path="//summary"/></param>
        /// <param name="resultAmount"><inheritdoc cref="resultAmount" path="//summary"/></param>
        public RecipeDTO(List<IngredientDTO> ingredients, double resultAmount = 1)
        {
            if (ingredients.Count == 0)
            {
                throw new ArgumentException("Ingredients list has no elements.", nameof(ingredients));
            }
            this.ingredients = ingredients;
            if (resultAmount < 0)
            {
                throw new ArgumentException("The result", nameof(resultAmount));
            }
            this.resultAmount = resultAmount;
        }

        public override string? ToString()
        {
            return string.Join(" + ", ingredients) + $" -> x{resultAmount}";
        }
    }
}
