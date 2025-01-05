using PACommon;
using PACommon.JsonUtils;
using ProgressAdventure.ItemManagement;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace ProgressAdventure.Entity
{
    /// <summary>
    /// A representation of an entity.<br/>
    /// Classes implementing this class MUST create a (protected) constructor, with signiture protected Type([return type from "FromJsonInternal()"] entityData) for FromJson() to work.<br/>
    /// You can also add a property for extra child class specific correction with the signature:<br/>
    /// protected static List{(Action{IDictionary{string, object?}} objectJsonCorrecter, string newFileVersion)} MiscVersionCorrecters { get; }
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
        public Entity(GenericEntityConstructorParametersDTO entityData)
            : base(entityData, true) { }

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

        static List<(Action<JsonDictionary> objectJsonCorrecter, string newFileVersion)> BaseVersionCorrecters { get; } =
        [
            // 2.1 -> 2.1.1
            (oldJson =>
            {
                // renamed speed to agility
                if (oldJson.TryGetValue("baseSpeed", out var baseSpeedValue))
                {
                    oldJson["baseAgility"] = baseSpeedValue;
                }
            }, "2.1.1"),
            // 2.1.1 -> 2.2
            (oldJson =>
            {
                // snake case keys
                if (oldJson.TryGetValue("baseMaxHp", out var baseMaxHpRename))
                {
                    oldJson["base_max_hp"] = baseMaxHpRename;
                }
                if (oldJson.TryGetValue("currentHp", out var chRename))
                {
                    oldJson["current_hp"] = chRename;
                }
                if (oldJson.TryGetValue("baseAttack", out var baRename))
                {
                    oldJson["base_attack"] = baRename;
                }
                if (oldJson.TryGetValue("baseDefence", out var bdRename))
                {
                    oldJson["base_defence"] = bdRename;
                }
                if (oldJson.TryGetValue("baseAgility", out var ba2Rename))
                {
                    oldJson["base_agility"] = ba2Rename;
                }
                if (oldJson.TryGetValue("originalTeam", out var otRename))
                {
                    oldJson["original_team"] = otRename;
                }
                if (oldJson.TryGetValue("currentTeam", out var ctRename))
                {
                    oldJson["current_team"] = ctRename;
                }
                if (oldJson.TryGetValue("xPos", out var xpRename))
                {
                    oldJson["x_position"] = xpRename;
                }
                if (oldJson.TryGetValue("yPos", out var ypRnename))
                {
                    oldJson["y_position"] = ypRnename;
                }
            }, "2.2"),
        ];

        static bool IJsonConvertable<TEntity>.FromJsonWithoutCorrection(JsonDictionary objectJson, string fileVersion, [NotNullWhen(true)] ref TEntity? convertedObject)
        {
            PACSingletons.Instance.JsonDataCorrecter.CorrectJsonData<TEntity>(objectJson, BaseVersionCorrecters, fileVersion);

            var miscVersionCorrectersProperty = typeof(TEntity).GetProperty(
                "MiscVersionCorrecters",
                BindingFlags.NonPublic | BindingFlags.Static,
                null,
                typeof(List<(Action<JsonDictionary> objectJsonCorrecter, string newFileVersion)>),
                [],
                null
            );

            var miscVersionCorrecters = miscVersionCorrectersProperty?.GetValue(null) as List<(Action<JsonDictionary> objectJsonCorrecter, string newFileVersion)>;
            if (miscVersionCorrecters is not null)
            {
                PACSingletons.Instance.JsonDataCorrecter.CorrectJsonData<Entity<TEntity>>(objectJson, miscVersionCorrecters, fileVersion);
            }

            return FromJsonWithoutGeneralCorrection(objectJson, fileVersion, out convertedObject);
        }
    }
}
