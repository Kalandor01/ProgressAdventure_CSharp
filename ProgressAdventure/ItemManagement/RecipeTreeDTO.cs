using PACommon;
using System.Collections.Immutable;
using System.Text;

namespace ProgressAdventure.ItemManagement
{
    /// <summary>
    /// DTO for storing a tree to which recipe to use in the making of an item.
    /// </summary>
    public class RecipeTreeDTO
    {
        /// <summary>
        /// The index of the recipe to make the item.
        /// </summary>
        public readonly uint recipeIndex;

        /// <summary>
        /// The recipe trees, for the parts of the item.
        /// </summary>
        public readonly ImmutableList<RecipeTreeDTO>? partRecipeTrees;

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="RecipeTreeDTO"/>
        /// </summary>
        /// <param name="recipeIndex"><inheritdoc cref="recipeIndex" path="//summary"/></param>
        /// <param name="partRecipeTrees"><inheritdoc cref="partRecipeTrees" path="//summary"/></param>
        public RecipeTreeDTO(uint recipeIndex, IEnumerable<RecipeTreeDTO>? partRecipeTrees = null)
        {
            this.recipeIndex = recipeIndex;
            this.partRecipeTrees = partRecipeTrees?.Any() == true ? partRecipeTrees.ToImmutableList() : null;
        }
        #endregion

        #region Public functions
        /// <summary>
        /// Converts the string representation of a recipe tree to a tree.
        /// </summary>
        /// <param name="recipeTreeString">The string representation of a recipe tree.</param>
        public static RecipeTreeDTO? Parse(string recipeTreeString)
        {
            var symbos = new List<object>();
            string extractedSymbol;
            do
            {
                if (Utils.GetFirstCharOrInt(ref recipeTreeString, out extractedSymbol))
                {
                    symbos.Add(uint.Parse(extractedSymbol));
                    continue;
                }

                if (extractedSymbol == "(")
                {
                    symbos.Add(true);
                }
                else if (extractedSymbol == ")")
                {
                    symbos.Add(false);
                }
            }
            while (extractedSymbol != "");
            return GetRecipeTreeFromSymbolArray([.. symbos]).recipeTree;
        }
        #endregion

        #region Public methods
        public override string? ToString()
        {
            var str = new StringBuilder(recipeIndex.ToString());
            if (partRecipeTrees is not null)
            {
                str.Append('(');
                for (var x = 0; x < partRecipeTrees.Count; x++)
                {
                    var recipeTreeStr = partRecipeTrees[x].ToString();
                    str.Append(recipeTreeStr);
                    if (x < partRecipeTrees.Count - 1 && recipeTreeStr?.Last() != ')')
                    {
                        str.Append(' ');
                    }
                }
                str.Append(')');
            }
            return str.ToString();
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Finds the end of the symbol section.
        /// </summary>
        /// <param name="symbols">The array of symbols. (true = section begin, false = section end)</param>
        /// <param name="startIndex">The index of the beginning of the section.</param>
        /// <returns>The index of the end of the symbol section, or -1 if the symbol has no end.</returns>
        private static int FindSymbolSectionEnd(object[] symbols, int startIndex = 0)
        {
            var depth = 1;
            var index = startIndex;
            while (index + 1 < symbols.Length && depth > 0)
            {
                index++;
                if (symbols[index] is bool symbol)
                {
                    depth += Convert.ToInt32(symbol) * 2 - 1;
                }
            }
            return index + 1 < symbols.Length ? index : -1;
        }

        /// <summary>
        /// Converts a recipe tree string to a tree.
        /// </summary>
        /// <param name="recipeSymbolsArray">The array representation of a recipe tree, where:<br/>
        /// uint: recipe index<br/>
        /// bool: sub-recipe section beginning/end</param>
        /// <returns>The recipe tree if succesful, and the end of the recipe tree in the inputed array, or -1 if that's the end of the array.</returns>
        private static (RecipeTreeDTO? recipeTree, int treeEndIndexInArray) GetRecipeTreeFromSymbolArray(object[] recipeSymbolsArray)
        {
            if (!(
                recipeSymbolsArray.Length != 0 &&
                recipeSymbolsArray[0] is uint rootRecipe
            ))
            {
                return (null, -1);
            }

            if (recipeSymbolsArray.Length <= 1)
            {
                return (new RecipeTreeDTO(rootRecipe, null), 0);
            }

            var firstNumber = recipeSymbolsArray[1..].FirstOrDefault(el => el is uint);
            var numberIndex = Array.IndexOf(recipeSymbolsArray, firstNumber, 1);
            var sectionBeginIndex = Array.IndexOf(recipeSymbolsArray, true, 1);
            if ((sectionBeginIndex == -1 ? recipeSymbolsArray.Length : sectionBeginIndex) > numberIndex)
            {
                return (new RecipeTreeDTO(rootRecipe, null), 0);
            }

            var partRecipeTrees = new List<RecipeTreeDTO>();

            var recipeTreeEnd = FindSymbolSectionEnd(recipeSymbolsArray, 1);
            if (recipeTreeEnd == -1)
            {
                recipeTreeEnd = recipeSymbolsArray.Length;
            }
            var symbolIndex = 1;
            while (symbolIndex < recipeTreeEnd)
            {
                var symbol = recipeSymbolsArray[symbolIndex];
                if (symbol is not uint)
                {
                    symbolIndex++;
                    continue;
                }

                var (recipeTree, treeEndIndexInArray) = GetRecipeTreeFromSymbolArray(recipeSymbolsArray[symbolIndex..]);
                if (recipeTree is not null)
                {
                    partRecipeTrees.Add(recipeTree);
                }
                symbolIndex += treeEndIndexInArray == -1 ? recipeSymbolsArray.Length : (treeEndIndexInArray + 1);
            }

            return (
                new RecipeTreeDTO(rootRecipe, partRecipeTrees),
                recipeTreeEnd == recipeSymbolsArray.Length ? -1 : recipeTreeEnd
            );
        }
        #endregion
    }
}
