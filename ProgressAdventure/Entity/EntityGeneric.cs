using ProgressAdventure.Enums;
using ProgressAdventure.ItemManagement;

namespace ProgressAdventure.Entity
{
    /// <summary>
    /// A representation of an entity.<br/>
    /// Classes implementing this class MUST create a (protected) constructor, with signiture protected Type([return type from "FromJsonInternal()"] entityData, IDictionary<string, object?>? miscData) for FromJson<T>() to work.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class Entity<TEntity> : Entity, IJsonConvertable<TEntity>
        where TEntity : Entity<TEntity>
    {
        /// <summary>
        /// <inheritdoc cref="Entity"/><br/>
        /// Can be used for loading the <c>Entity</c> from json.
        /// </summary>
        /// <param name="entityData">The entity data, from <c>FromJsonInternal</c>.</param>
        /// <param name="miscData">The json data, that can be used for loading extra data, specific to an entity type.<br/>
        /// Should only be null, if entity creation called this constructor.</param>
        public Entity(
            (
                string? name,
                int? baseMaxHp,
                int? currentHp,
                int? baseAttack,
                int? baseDefence,
                int? baseSpeed,
                int? originalTeam,
                int? currentTeam,
                List<Enums.Attribute>? attributes,
                List<Item>? drops,
                (long x, long y)? position,
                Facing? facing
            ) entityData,
            IDictionary<string, object?>? miscData
        ) : base(entityData, miscData) { }

        /// <summary>
        /// <inheritdoc cref="Entity"/><br/>
        /// Can be used for creating a new <c>Entity</c>, from the result of the EntityManager function.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        /// <param name="stats">The tuple of stats, representin all other values from the other constructor, other than drops.</param>
        /// <param name="drops"><inheritdoc cref="drops" path="//summary"/></param>
        public Entity(
            string name,
            EntityManagerStats stats,
            List<Item>? drops = null
        ) : base(name, stats, drops) { }

        public static TEntity? FromJson(IDictionary<string, object?>? entityJson)
        {
            return FromJson<TEntity>(entityJson);
        }
    }
}
