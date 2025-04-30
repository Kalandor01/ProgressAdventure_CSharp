using NPrng.Generators;
using PACommon;
using PACommon.Enums;
using PACommon.Extensions;
using PACommon.JsonUtils;
using ProgressAdventure.ConfigManagement;
using ProgressAdventure.EntityManagement;
using ProgressAdventure.Enums;
using ProgressAdventure.WorldManagement.Content;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using PACTools = PACommon.Tools;

namespace ProgressAdventure.WorldManagement
{
    /// <summary>
    /// The class used to store loaded and not yet loaded entities.
    /// </summary>
    public class PopulationManager : IJsonConvertableExtra<PopulationManager, (SplittableRandom chunkRandom, (long x, long y) absolutePosition)>
    {
        #region Private fields
        /// <summary>
        /// The entities that have not been loaded yet.
        /// </summary>
        private readonly Dictionary<EnumValue<EntityType>, int> unloadedEntities;

        /// <summary>
        /// The loaded entities.
        /// </summary>
        private readonly Dictionary<EnumValue<EntityType>, List<Entity>> loadedEntities;

        /// <summary>
        /// The absolute position of the parrent <see cref="Tile"/>. Used for seting the created entites' position.
        /// </summary>
        public readonly (long x, long y) absolutePosition;

        /// <summary>
        /// The chunk random generator.
        /// </summary>
        public readonly SplittableRandom chunkRandom;

        public long UnloadedPopulationCount { get => unloadedEntities.Sum(item => item.Value); }
        public long LoadedPopulationCount { get => loadedEntities.Sum(item => item.Value.Count); }
        public long PopulationCount { get => UnloadedPopulationCount + LoadedPopulationCount; }
        public bool IsEmpty { get => unloadedEntities.Count == 0 && loadedEntities.Count == 0; }
        public List<EnumValue<EntityType>> ContainedEntities { get => [.. unloadedEntities.Keys.Concat(loadedEntities.Keys).Distinct()]; }
        #endregion

        #region Constructors
        /// <summary>
        /// <inheritdoc cref="PopulationManager" path="//summary"/>
        /// </summary>
        /// <param name="unloadedEntities"><inheritdoc cref="unloadedEntities" path="//summary"/></param>
        /// <param name="loadedEntities"><inheritdoc cref="loadedEntities" path="//summary"/></param>
        /// <param name="absolutePosition"><inheritdoc cref="absolutePosition" path="//summary"/></param>
        /// <param name="chunkRandom"><inheritdoc cref="chunkRandom" path="//summary"/></param>
        private PopulationManager(
            Dictionary<EnumValue<EntityType>, int> unloadedEntities,
            Dictionary<EnumValue<EntityType>, List<Entity>> loadedEntities,
            (long posX, long posY) absolutePosition,
            SplittableRandom chunkRandom
        )
        {
            this.unloadedEntities = unloadedEntities;
            this.loadedEntities = loadedEntities;
            this.absolutePosition = absolutePosition;
            this.chunkRandom = chunkRandom;
        }

        /// <summary>
        /// <inheritdoc cref="PopulationManager" path="//summary"/>
        /// </summary>
        /// <param name="unloadedEntities"><inheritdoc cref="unloadedEntities" path="//summary"/></param>
        /// <param name="loadedEntitiesList"><inheritdoc cref="loadedEntities" path="//summary"/></param>
        /// <param name="absolutePosition"><inheritdoc cref="absolutePosition" path="//summary"/></param>
        /// <param name="chunkRandom"><inheritdoc cref="chunkRandom" path="//summary"/></param>
        public PopulationManager(
            Dictionary<EnumValue<EntityType>, int> unloadedEntities,
            List<Entity> loadedEntitiesList,
            (long posX, long posY) absolutePosition,
            SplittableRandom chunkRandom
        )
            : this(unloadedEntities, loadedEntitiesList.ToListDictionary(key => key.type, value => value), absolutePosition, chunkRandom)
        {

        }

        /// <summary>
        /// <inheritdoc cref="PopulationManager" path="//summary"/>
        /// </summary>
        /// <param name="entityCounts"><inheritdoc cref="unloadedEntities" path="//summary"/></param>
        /// <param name="absolutePosition"><inheritdoc cref="absolutePosition" path="//summary"/></param>
        /// <param name="chunkRandom"><inheritdoc cref="chunkRandom" path="//summary"/></param>
        public PopulationManager(
            Dictionary<EnumValue<EntityType>, int> entityCounts,
            (long posX, long posY) absolutePosition,
            SplittableRandom chunkRandom
        )
            : this(entityCounts, loadedEntitiesList: [], absolutePosition, chunkRandom)
        {

        }
        #endregion

        #region Public methods
        /// <summary>
        /// Should be called if a player is on the tile, that is the parrent of this content.<br/>
        /// TODO: THIS PROBABLY NEEDS TO BE REWORKED!!!
        /// </summary>
        /// <param name="tile">The parrent tile.</param>
        public void Visit(Tile tile)
        {
            if (IsEmpty)
            {
                return;
            }

            PACSingletons.Instance.Logger.Log($"Player visited \"population\": {ToString()}", $"x: {tile.relativePosition.x}, y: {tile.relativePosition.y}, visits: {tile.Visited}");
            
            Console.WriteLine(GetPopulationAmountsString());

            if (tile.structure.subtype == ContentType.Structure.NONE)
            {
                return;
            }

            if (tile.structure.subtype == ContentType.Structure.BANDIT_CAMP)
            {
                if (chunkRandom.GenerateDouble() < 0.75)
                {
                    EntityUtils.RandomFight();
                }
            }
            else if (
                tile.structure.subtype == ContentType.Structure.VILLAGE ||
                tile.structure.subtype == ContentType.Structure.KINGDOM
            )
            {
                if (chunkRandom.GenerateDouble() < 0.01)
                {
                    EntityUtils.RandomFight();
                }
            }
        }

        /// <summary>
        /// Gets all loaded player entities in this <see cref="PopulationManager"/>.
        /// </summary>
        public List<Entity> GetPlayers()
        {
            return loadedEntities.TryGetValue(EntityType.PLAYER, out var playerList) ? [.. playerList] : [];
        }

        /// <summary>
        /// Loads entities of a specific type from the unloaded entities dict.
        /// </summary>
        /// <param name="entityType">The type of the entities to load.</param>
        /// <param name="amount">The amount of entities to load.</param>
        /// <param name="exactOnly">Whether to only load the entities if there are enough unloaded entities found.</param>
        /// <returns>The list of loaded entities.</returns>
        public List<Entity> LoadEntities(
            EnumValue<EntityType> entityType,
            int amount = 1,
            bool exactOnly = true
        )
        {
            if (
                !unloadedEntities.TryGetValue(entityType, out var currentAmount) ||
                currentAmount == 0 ||
                (exactOnly && currentAmount < amount)
            )
            {
                return [];
            }

            var actualAmount = Math.Min(amount, currentAmount);
            return LoadEntitiesUncecked(entityType, actualAmount);
        }

        /// <summary>
        /// Loads entities from the unloaded entities dict.
        /// </summary>
        /// <param name="amount">The amount of entities to load.</param>
        /// <param name="exactOnly">Whether to only load the entities if there are enough unloaded entities found.</param>
        /// <returns>The list of loaded entities.</returns>
        public List<Entity> LoadEntities(
            int amount = 1,
            bool exactOnly = true
        )
        {
            var entitiesToGenerate = new Dictionary<EnumValue<EntityType>, int>();
            var sumAmount = 0;
            while (true)
            {
                var nextType = unloadedEntities.FirstOrDefault(ue => !entitiesToGenerate.ContainsKey(ue.Key));
                if (nextType.Key is null)
                {
                    if (sumAmount == 0 || (sumAmount < amount && exactOnly))
                    {
                        return [];
                    }
                    break;
                }
                entitiesToGenerate[nextType.Key] = Math.Min(nextType.Value, amount - sumAmount);
                if (sumAmount >= amount)
                {
                    break;
                }
            }

            var loadedEntities = new List<Entity>();
            foreach (var entityToGenerate in entitiesToGenerate)
            {
                loadedEntities.AddRange(LoadEntitiesUncecked(entityToGenerate.Key, entityToGenerate.Value));
            }
            return loadedEntities;
        }

        /// <summary>
        /// Loads entities of a random type from the unloaded entities dict.
        /// </summary>
        /// <param name="amount">The amount of entities to load.</param>
        /// <param name="exactOnly">Whether to only load the entities if there are enough unloaded entities found.</param>
        /// <returns>The list of loaded entities.</returns>
        public List<Entity> LoadEntitiesRandom(
            int amount = 1,
            bool exactOnly = true
        )
        {
            if (
                exactOnly
            )
            {
                var sum = 0;
                foreach (var unloadedEntity in unloadedEntities)
                {
                    sum += unloadedEntity.Value;
                    if (sum >= amount )
                    {
                        break;
                    }
                }
                if (sum < amount)
                {
                    return [];
                }
            }

            var loadedEntities = new List<Entity>();
            for (var x = 0; x < amount; x++)
            {
                var existingKeys = unloadedEntities.Keys.ToList();
                var selectedKey = existingKeys[(int)chunkRandom.GenerateInRange(0, existingKeys.Count - 1)];
                loadedEntities.Add(LoadEntitiesUncecked(selectedKey, 1)[0]);
            }
            return loadedEntities;
        }

        /// <summary>
        /// Returns the amount of unloaded and loaded entities of a specific type.
        /// </summary>
        /// <param name="entityType">The type of the entity to return the count of.</param>
        /// <param name="unloadedCount">The count of the unloaded entities of the selected type.</param>
        public long GetEntityCount(EnumValue<EntityType> entityType, out int unloadedCount)
        {
            unloadedCount = unloadedEntities.TryGetValue(entityType, out var uCount) ? uCount : 0;
            return unloadedCount + (loadedEntities.TryGetValue(entityType, out var lCount) ? lCount.Count : 0);
        }

        /// <summary>
        /// Returns if the <see cref="Entity"/> exists in this <see cref="PopulationManager"/>.
        /// </summary>
        /// <param name="entity">The entity to check.</param>
        public bool EntityExists(Entity entity)
        {
            return loadedEntities.TryGetValue(entity.type, out var entities) && entities.Contains(entity);
        }

        /// <summary>
        /// Adds the <see cref="Entity"/> to the <see cref="PopulationManager"/> if it doesn't already exist there.
        /// </summary>
        /// <param name="entity">The <see cref="Entity"/> to add.</param>
        /// <returns>If the <see cref="Entity"/> was successfuly added. The <see cref="Entity"/> needs to have the same position as this <see cref="PopulationManager"/>.</returns>
        public bool AddEntity(Entity entity)
        {
            if (
                entity.Position is null ||
                entity.Position.Value != absolutePosition
            )
            {
                return false;
            }

            if (loadedEntities.TryGetValue(entity.type, out var entities))
            {
                if (entities.Contains(entity))
                {
                    return false;
                }
                entities.Add(entity);
            }
            else
            {
                loadedEntities[entity.type] = [entity];
            }
            return true;
        }

        /// <summary>
        /// Removes the <see cref="Entity"/> from the <see cref="PopulationManager"/> if it exist there.
        /// </summary>
        /// <param name="entity">The <see cref="Entity"/> to remove.</param>
        /// <returns>If the <see cref="Entity"/> was successfuly removed. The <see cref="Entity"/> needs to have a <see cref="null"/> position.</returns>
        public bool RemoveEntity(Entity entity)
        {
            if (entity.Position is not null)
            {
                return false;
            }

            if (!loadedEntities.TryGetValue(entity.type, out var entities))
            {
                return false;
            }

            var entityIndex = entities.IndexOf(entity);
            if (entityIndex == -1)
            {
                return false;
            }

            entities.RemoveAt(entityIndex);
            if (entities.Count == 0)
            {
                loadedEntities.Remove(entity.type);
            }
            return true;
        }

        /// <summary>
        /// Moves a specific number on unloaded <see cref="Entity"/>s from one <see cref="PopulationManager"/> to another.
        /// </summary>
        /// <param name="otherPopulationManager">The <see cref="PopulationManager"/> to move the <see cref="Entity"/> to.</param>
        /// <param name="entityType">The type of <see cref="Entity"/>s to move.</param>
        /// <param name="amount">The amount of entities to move.</param>
        /// <param name="moveExact">Whether to only succeed if the specified number of entities could be moved, or at least 1.</param>
        /// <returns>If the entities were successfuly moved.</returns>
        public bool MoveEntity(PopulationManager otherPopulationManager, EnumValue<EntityType> entityType, int amount = 1, bool moveExact = true)
        {
            if (
                this == otherPopulationManager ||
                !unloadedEntities.TryGetValue(entityType, out var entityCount) ||
                entityCount < 1 ||
                (moveExact && entityCount < amount)
            )
            {
                return false;
            }
            
            var moveAmount = Math.Min(amount, entityCount);
            if (entityCount == moveAmount)
            {
                unloadedEntities.Remove(entityType);
            }
            else
            {
                unloadedEntities[entityType] -= moveAmount;
            }

            if (otherPopulationManager.unloadedEntities.ContainsKey(entityType))
            {
                otherPopulationManager.unloadedEntities[entityType] += moveAmount;
            }
            else
            {
                otherPopulationManager.unloadedEntities[entityType] = moveAmount;
            }
            return true;
        }

        public override string? ToString()
        {
            var unloadedEntitiesStr = string.Join(", ", unloadedEntities.Select(ue => $"{ue.Key}: {ue.Value}"));
            var loadedEntitiesStr = string.Join(", ", loadedEntities.Select(le => $"{le.Key}: {le.Value.Count}"));
            return $"unloaded entities: ({unloadedEntitiesStr}), loaded entities: ({loadedEntitiesStr})";
        }
        #endregion

        #region Private methods
        private string GetPopulationAmountsString()
        {
            var amounts = new Dictionary<EnumValue<EntityType>, long>();
            var counts = unloadedEntities
                .Concat(
                    loadedEntities.Select(le => new KeyValuePair<EnumValue<EntityType>, int>(le.Key, le.Value.Count))
                );
            foreach (var count in counts)
            {
                if (amounts.TryGetValue(count.Key, out var v))
                {
                    amounts[count.Key] += count.Value;
                }
                else
                {
                    amounts[count.Key] = count.Value;
                }
            }
            var txt = new StringBuilder();
            var x = 0;
            foreach (var amount in amounts)
            {
                if (x > 0 && x < amounts.Count - 1)
                {
                    txt.Append(", ");
                }
                var multiple = amount.Value > 1;
                txt.Append($"{(multiple ? $"{amount.Value}" : "a(n)")} {EntityUtils.EntityPropertiesMap[amount.Key].displayName}{(multiple ? "s" : "")}");
                if (x == amounts.Count - 2)
                {
                    txt.Append(" and ");
                }
                x++;
            }
            var popAmount = PopulationCount;
            return $"There {(popAmount == 1 ? "is" : "are")} {txt} here.";
        }

        /// <summary>
        /// Loads a specific amount of entities from the unloaded dict.
        /// </summary>
        /// <param name="entityType">The type of the entities to load.</param>
        /// <param name="amount">The amount of entities to load.</param>
        /// <returns>The list of loaded entities.</returns>
        private List<Entity> LoadEntitiesUncecked(EnumValue<EntityType> entityType, int amount)
        {
            if (amount <= 0)
            {
                return [];
            }

            unloadedEntities[entityType] -= amount;
            if (unloadedEntities[entityType] == 0)
            {
                unloadedEntities.Remove(entityType);
            }

            var newEntities = new List<Entity>();
            for (var x = 0; x < amount; x++)
            {
                var newEntity = new Entity(entityType, generationRandom: chunkRandom);
                newEntity.AddPosition(absolutePosition);
                newEntities.Add(new Entity(entityType, generationRandom: chunkRandom));
            }
            return newEntities;
        }
        #endregion

        #region JsonConversion
        #region Protected properties
        static List<(Action<JsonDictionary, (SplittableRandom chunkRandom, (long x, long y) absolutePosition)> objectJsonCorrecter, string newFileVersion)> IJsonConvertableExtra<PopulationManager, (SplittableRandom chunkRandom, (long x, long y) absolutePosition)>.VersionCorrecters { get; } =
        [
            // 2.2.1 -> 2.2.2
            ((oldJson, chunkRandom) =>
            {
                // content type IDs are like item type IDs
                JsonDataCorrecterUtils.TransformValue<BaseContent, string>(oldJson, "subtype", (oldSubtypeValue) =>
                {
                    return (
                        WorldUtils._legacyContentSubtypeNameMap.TryGetValue(("population", oldSubtypeValue), out var newSubtype),
                        newSubtype
                    );
                });
            }, "2.2.2"),
            // 2.3 -> 2.4
            ((oldJson, chunkRandom) =>
            {
                // namespaced subtype
                if (
                    PACTools.TryParseJsonValue<string>(oldJson, "subtype", out var subtypeValue, false) &&
                    !string.IsNullOrWhiteSpace(subtypeValue)
                )
                {
                    oldJson["subtype"] = ConfigUtils.GetSpecificNamespacedString(subtypeValue);
                }
            }, "2.4"),
            // 2.4.1 -> 2.5
            ((oldJson, chunkRandom) =>
            {
                // PopulationContent to PopulationManager
                oldJson["loaded_entities"] = new JsonDictionary();
                if (
                    PACTools.TryParseJsonValue<string>(oldJson, "subtype", out var subtypeValue, false) &&
                    !string.IsNullOrWhiteSpace(subtypeValue) &&
                    PACTools.TryParseJsonValue<string>(oldJson, "amount", out var amount, false) &&
                    WorldUtils._legacyPopulationContentEntityTypeNameMap.TryGetValue(subtypeValue, out var entityType)
                )
                {
                    var ueDict = new JsonDictionary();
                    if (entityType is not null)
                    {
                        ueDict[entityType] = amount;
                    }
                    oldJson["unloaded_entities"] = ueDict;
                }
            }, "2.5"),
        ];
        #endregion

        public JsonDictionary ToJson()
        {
            return new JsonDictionary()
            {
                [Constants.JsonKeys.PopulationManager.UNLOADED_ENTITIES] = new JsonDictionary(
                    unloadedEntities.ToDictionary(
                        k => k.Key.Name,
                        v => (JsonObject?)v.Value
                    )
                ),
                [Constants.JsonKeys.PopulationManager.LOADED_ENTITIES] = new JsonDictionary(
                    loadedEntities.ToDictionary(
                        k => k.Key.Name,
                        v => (JsonObject?)new JsonArray(
                            [.. v.Value.Select(e => (JsonObject?)e.ToJson())]
                        )
                    )
                ),
            };
        }

        public static bool FromJsonWithoutCorrection(
            JsonDictionary objectJson,
            (SplittableRandom chunkRandom, (long x, long y) absolutePosition) extraData,
            string fileVersion,
            [NotNullWhen(true)] ref PopulationManager? convertedObject
        )
        {
            if (extraData.chunkRandom is null)
            {
                PACTools.LogJsonTypeParseError("invalid extra data for this type", true);
                return false;
            }

            if (
                !PACTools.TryCastJsonAnyValue<JsonDictionary>(
                    objectJson,
                    Constants.JsonKeys.PopulationManager.UNLOADED_ENTITIES,
                    out var unloadedEntitiesJson,
                    true,
                    true
                ) ||
                !PACTools.TryCastJsonAnyValue<JsonDictionary>(
                    objectJson,
                    Constants.JsonKeys.PopulationManager.LOADED_ENTITIES,
                    out var loadedEntitiesJson,
                    true,
                    true
                )
            )
            {
                return false;
            }

            var success = true;

            var unloadedEntities = new Dictionary<EnumValue<EntityType>, int>();
            foreach (var unloadedEntityJson in unloadedEntitiesJson)
            {
                if (!EntityType.TryGetValue(unloadedEntityJson.Key, out var entityType))
                {
                    PACTools.LogJsonParseError(nameof(entityType), $"invalid entity type name: \"{unloadedEntityJson.Key}\"");
                    success = false;
                    continue;
                }

                if (!PACTools.TryParseValueForJsonParsing<int>(unloadedEntityJson.Value, out var unloadedCount))
                {
                    success = false;
                    continue;
                }

                if (unloadedCount < 1)
                {
                    PACTools.LogJsonParseError(nameof(unloadedCount), $"unloaded entity count for \"{unloadedEntityJson.Key}\" is too small: {unloadedCount}");
                    success = false;
                    continue;
                }

                unloadedEntities[entityType] = unloadedCount;
            }

            var loadedEntities = new Dictionary<EnumValue<EntityType>, List<Entity>>();
            foreach (var loadedEntityJson in loadedEntitiesJson)
            {
                if (!EntityType.TryGetValue(loadedEntityJson.Key, out var entityType))
                {
                    PACTools.LogJsonParseError(nameof(entityType), $"invalid entity type name: \"{loadedEntityJson.Key}\"");
                    success = false;
                    continue;
                }

                if (
                    !PACTools.TryCastAnyValueForJsonParsing<JsonArray>(
                        loadedEntityJson.Value,
                        out var loadedEntitiesJsonArray,
                        isStraigthCast: true
                    ) ||
                    !PACTools.TryParseListValueForJsonParsing(
                        loadedEntitiesJsonArray,
                        nameof(loadedEntitiesJsonArray),
                        entityJson =>
                        {
                            if (!PACTools.TryCastAnyValueForJsonParsing<Entity, JsonDictionary>(entityJson, out var entityJsonValue, isStraigthCast: true))
                            {
                                success = false;
                                return (false, default);
                            }

                            success &= PACTools.TryFromJsonExtra<Entity, (long, long)?>(
                                entityJsonValue,
                                extraData.absolutePosition,
                                fileVersion,
                                out var entity
                            );
                            return (entity is not null, entity);
                        },
                        out var entities
                    )
                )
                {
                    success = false;
                    continue;
                }

                loadedEntities[entityType] = entities;
            }

            convertedObject = new PopulationManager(unloadedEntities, loadedEntities, extraData.absolutePosition, extraData.chunkRandom);
            return success;
        }
        #endregion
    }
}
