using ProgressAdventure;
using ProgressAdventure.Entity;
using ProgressAdventure.Enums;
using ProgressAdventure.ItemManagement;
using System.Reflection;
using Xunit;
using PAConstants = ProgressAdventure.Constants;

namespace ProgressAdventureTests
{
    public static class Tests
    {
        /// <summary>
        /// Checks if all item IDs can be turned into items.
        /// </summary>
        [Fact]
        public static (LogSeverity resultType, string? exeption)? AllItemTypesExistAndLoadable()
        {
            var itemAmount = 3;

            var allItems = new List<Item>();

            // all item IDs can turn into items
            foreach (var itemID in ItemUtils.GetAllItemTypes())
            {
                Item item;
                try
                {
                    item = new Item(itemID, itemAmount);
                }
                catch (Exception ex)
                {
                    return (LogSeverity.FAIL, $"Couldn't create item from type \"{itemID}\": " + ex);
                }

                allItems.Add(item);
            }

            // items loadable from json and are the same as before load
            foreach (var item in allItems)
            {
                Item loadedItem;

                try
                {
                    var itemJson = item.ToJson();
                    loadedItem = Item.FromJson(itemJson, PAConstants.SAVE_VERSION);

                    if (loadedItem is null)
                    {
                        throw new ArgumentNullException(item.Type.ToString());
                    }
                }
                catch (Exception ex)
                {
                    return (LogSeverity.FAIL, $"Loading item from json failed for \"{item.Type}\": " + ex);
                }

                if (
                    loadedItem.Type == item.Type &&
                    loadedItem.Amount == item.Amount &&
                    loadedItem.DisplayName == item.DisplayName &&
                    loadedItem.Consumable == item.Consumable
                )
                {
                    continue;
                }

                return (LogSeverity.FAIL, $"Original item, and item loaded from json are not the same for \"{item.Type}\"");
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

            // check if entity exists and loadable from jsom
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
                        throw new ArgumentNullException(entityType.ToString());
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

                Entity entity;

                try
                {
                    entity = Entity.AnyEntityFromJson(defEntityJson);

                    if (entity is null)
                    {
                        throw new ArgumentNullException(typeName);
                    }
                }
                catch (Exception ex)
                {
                    return (LogSeverity.FAIL, $"Entity creation from default json failed for \"{entityType}\": " + ex);
                }

                entities.Add(entity);
            }

            // entities loadable from json and are the same as before load
            foreach (var entity in entities)
            {
                Entity loadedEntity;

                try
                {
                    var entityJson = entity.ToJson();
                    loadedEntity = Entity.AnyEntityFromJson(entityJson);

                    if (loadedEntity is null)
                    {
                        throw new ArgumentNullException(entity.GetType().ToString());
                    }
                }
                catch (Exception ex)
                {
                    return (LogSeverity.FAIL, $"Loading entity from json failed for \"{entity.GetType()}\": " + ex);
                }

                if (
                    loadedEntity.GetType() == entity.GetType() &&
                    loadedEntity.FullName == entity.FullName &&
                    loadedEntity.MaxHp == entity.MaxHp &&
                    loadedEntity.CurrentHp == entity.CurrentHp &&
                    loadedEntity.Attack == entity.Attack &&
                    loadedEntity.Defence == entity.Defence &&
                    loadedEntity.Agility == entity.Agility
                )
                {
                    continue;
                }

                return (LogSeverity.FAIL, $"Original entity, and entity loaded from json are not the same for \"{entity.GetType()}\"");
            }

            return null;
        }
    }
}
