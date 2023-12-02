using PACommon;
using PACommon.JsonUtils;
using ProgressAdventure.Enums;
using ProgressAdventure.ItemManagement;

namespace ProgressAdventure.Entity
{
    /// <summary>
    /// A representation of an entity.<br/>
    /// Classes implementing this class MUST create a (protected) constructor, with signiture protected Type([return type from "FromJsonInternal()"] entityData) for FromJson<T>() to work.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public abstract class Entity<TEntity> : Entity, IJsonConvertable<TEntity>
        where TEntity : Entity<TEntity>, IJsonConvertable<TEntity>
    {
        /// <summary>
        /// <inheritdoc cref="Entity"/><br/>
        /// Can be used for loading the <c>Entity</c> from json.
        /// </summary>
        /// <param name="entityData">The entity data, from <c>FromJsonInternal</c>.</param>
        public Entity(
            (
                string? name,
                int? baseMaxHp,
                int? currentHp,
                int? baseAttack,
                int? baseDefence,
                int? baseAgility,
                int? originalTeam,
                int? currentTeam,
                List<Enums.Attribute>? attributes,
                List<AItem>? drops,
                (long x, long y)? position,
                Facing? facing
            ) entityData
        ) : base(entityData, false) { }

        /// <summary>
        /// <inheritdoc cref="Entity"/><br/>
        /// Can be used for creating a new <c>Entity</c>, from the result of the EntityManager function.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        /// <param name="stats">The tuple of stats, representin all other values from the other constructor, other than drops.</param>
        /// <param name="drops">The list of items, that the entity will drop on death, as loot.</param>
        public Entity(
            string name,
            EntityManagerStatsDTO stats,
            List<AItem>? drops = null
        ) : base(name, stats, drops) { }

        static List<(Action<IDictionary<string, object?>> objectJsonCorrecter, string newFileVersion)> BaseVersionCorrecters { get; } = new()
        {
            // 2.1 -> 2.1.1
            (oldJson =>
            {
                // renamed speed to agility
                if (oldJson.TryGetValue("baseSpeed", out object? baseSpeedValue))
                {
                    oldJson["baseAgility"] = baseSpeedValue;
                }
            }, "2.1.1"),
            // 2.1.1 -> 2.2
            (oldJson =>
            {
                // snake case keys
                if (oldJson.TryGetValue("baseMaxHp", out object? baseMaxHpRename))
                {
                    oldJson["base_max_hp"] = baseMaxHpRename;
                }
                if (oldJson.TryGetValue("currentHp", out object? chRename))
                {
                    oldJson["current_hp"] = chRename;
                }
                if (oldJson.TryGetValue("baseAttack", out object? baRename))
                {
                    oldJson["base_attack"] = baRename;
                }
                if (oldJson.TryGetValue("baseDefence", out object? bdRename))
                {
                    oldJson["base_defence"] = bdRename;
                }
                if (oldJson.TryGetValue("baseAgility", out object? ba2Rename))
                {
                    oldJson["base_agility"] = ba2Rename;
                }
                if (oldJson.TryGetValue("originalTeam", out object? otRename))
                {
                    oldJson["original_team"] = otRename;
                }
                if (oldJson.TryGetValue("currentTeam", out object? ctRename))
                {
                    oldJson["current_team"] = ctRename;
                }
                if (oldJson.TryGetValue("xPos", out object? xpRename))
                {
                    oldJson["x_position"] = xpRename;
                }
                if (oldJson.TryGetValue("yPos", out object? ypRnename))
                {
                    oldJson["y_position"] = ypRnename;
                }
            }, "2.2"),
        };

        static bool IJsonConvertable<TEntity>.FromJsonWithoutCorrection(IDictionary<string, object?> objectJson, string fileVersion, ref TEntity? convertedObject)
        {
            PACSingletons.Instance.JsonDataCorrecter.CorrectJsonData<TEntity>(ref objectJson, BaseVersionCorrecters, fileVersion);
            return FromJsonWithoutGeneralCorrection(objectJson, fileVersion, out convertedObject);
        }
    }
}
