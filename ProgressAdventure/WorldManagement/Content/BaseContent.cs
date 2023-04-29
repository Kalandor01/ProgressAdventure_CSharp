namespace ProgressAdventure.WorldManagement.Content
{
    public abstract class BaseContent
    {
        #region Public fields
        public readonly ContentTypeID type;
        public readonly ContentTypeID subtype;
        public readonly string? name;
        #endregion

        #region Constructors
        public BaseContent(IDictionary<string, object?>? data = null)
        {
            if (
                type.Super != ContentType.BaseContentType ||
                subtype.Super == ContentType.BaseContentType
            )
            {
                throw new ArgumentException("Content types are missmached.");
            }

            name = null;
            if (data is not null && data.ContainsKey("name"))
            {
                name = (string?)data["name"];
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Should be called if a player is on the tile, that is the parrent of this content.
        /// </summary>
        /// <param name="tile">The parrent tile.</param>
        protected virtual void Visit(Tile tile)
        {
            Logger.Log($"Player visited \"{type}\": \"{subtype}\"{(name is not null ? $" ({name})" : "")}", $"x: {tile.x}, y: {tile.y}, visits: {tile.visited}");
        }

        /// <summary>
        /// Returns a json representation of the <c>Content</c>.
        /// </summary>
        public virtual Dictionary<string, object?> ToJson()
        {
            return new Dictionary<string, object?> {
                ["type"] = type.GetHashCode(),
                ["subtype"] = subtype.GetHashCode(),
                ["name"] = name
            };
        }
        #endregion
    }
}
