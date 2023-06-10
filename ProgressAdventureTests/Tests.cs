using ProgressAdventure.Entity;
using ProgressAdventure.ItemManagement;
using Xunit;
using Moq;
using PATools = ProgressAdventure.Tools;
using System.Reflection;
using ProgressAdventure.Enums;
using ProgressAdventure;

namespace ProgressAdventureTests
{
    public static class Tests
    {
        /// <summary>
        /// Checks if all item IDs can be turned into items.
        /// </summary>
        [Fact]
        public static (LogSeverity resultType, string? exeption)? CheckAllItemTypesExist()
        {
            var allItems = new List<Item>();

            foreach (var itemID in ItemUtils.GetAllItemTypes())
            {
                allItems.Add(new Item(itemID));
            }

            return null;
        }

        /// <summary>
        /// Checks if all entities have a type name, and can be loaded from json.
        /// </summary>
        [Fact]
        public static (LogSeverity resultType, string? exeption)? AllEntitiesLoadable()
        {
            RandomStates.Initialise();

            var entityType = typeof(Entity);
            var paAssembly = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name == nameof(ProgressAdventure)).First();
            var entityTypes = paAssembly.GetTypes().Where(entityType.IsAssignableFrom);
            var filteredEntityTypes = entityTypes.Where(type => type != typeof(Entity) && type != typeof(Entity<>));

            var entities = new List<Entity>();

            foreach (var type in filteredEntityTypes)
            {
                // get entity type name
                var entityTypeMapName = "entityTypeMap";
                IDictionary<string, Type> entityTypeMap;

                try
                {
                    entityTypeMap = (IDictionary<string, Type>)typeof(EntityUtils).GetField(entityTypeMapName, BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

                    if (entityTypeMap is null)
                    {
                        throw new ArgumentNullException(nameof(entityType));
                    }
                }
                catch (Exception ex)
                {
                    return (LogSeverity.FAIL, $"Exeption because of (outdated?) test structure in {nameof(EntityUtils)}: " + ex);
                }

                string? typeName = null;
                foreach (var eType in entityTypeMap)
                {
                    if (eType.Value == type)
                    {
                        typeName = eType.Key;
                        break;
                    }
                }

                if (typeName is null)
                {
                    return (LogSeverity.FAIL, "Entity type has no type name in entity type map");
                }


                var defEntityJson = new Dictionary<string, object?>()
                {
                    ["type"] = typeName,
                };

                var entity = Entity.AnyEntityFromJson(defEntityJson);

                if (entity is null)
                {
                    return (LogSeverity.FAIL, $"Failed to create entity type from default json: {typeName}");
                }

                entities.Add(entity);
            }

            return null;
        }
    }
}
