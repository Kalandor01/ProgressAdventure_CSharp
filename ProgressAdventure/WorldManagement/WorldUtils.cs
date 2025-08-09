using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using NPrng.Generators;
using PACommon;
using PACommon.Enums;
using ProgressAdventure.ConfigManagement;
using ProgressAdventure.Enums;
using ProgressAdventure.WorldManagement.Content;
using ProgressAdventure.WorldManagement.Content.Structure;
using ProgressAdventure.WorldManagement.Content.Terrain;

namespace ProgressAdventure.WorldManagement
{
    public static class WorldUtils
    {
        #region Json correction dicts
        /// <summary>
        /// The dictionary pairing up pre and post 2.2.2 content subtype names.
        /// </summary>
        internal static readonly Dictionary<(string type, string subtype), string> _legacyContentSubtypeNameMap = new()
        {
            //terrain
            [("terrain", "field")] = "terrain/field",
            [("terrain", "mountain")] = "terrain/mountain",
            [("terrain", "ocean")] = "terrain/ocean",
            [("terrain", "shore")] = "terrain/shore",
            //structure
            [("structure", "none")] = "structure/none",
            [("structure", "bandit_camp")] = "structure/bandit_camp",
            [("structure", "village")] = "structure/village",
            [("structure", "kingdom")] = "structure/kingdom",
            //population
            [("population", "none")] = "population/none",
            [("population", "human")] = "population/human",
            [("population", "elf")] = "population/elf",
            [("population", "dwarf")] = "population/dwarf",
            [("population", "demon")] = "population/demon",
        };

        /// <summary>
        /// The dictionary pairing up pre and post 2.5 unloaded entity tape names.
        /// </summary>
        internal static readonly Dictionary<string, string?> _legacyPopulationContentEntityTypeNameMap = new()
        {
            ["pa:population/none"] = null,
            ["pa:population/human"] = "pa:human",
            ["pa:population/elf"] = "pa:elf",
            ["pa:population/dwarf"] = "pa:dwarf",
            ["pa:population/demon"] = "pa:demon",
        };
        #endregion

        #region Constatnts
        /// <summary>
        /// If difference is larger than this, the structure will not generate.
        /// </summary>
        public const double noStructureDifferenceLimit = 0.3;
        /// <summary>
        /// If difference is larger than this, the population will not generate.
        /// </summary>
        public const double noPopulationDifferenceLimit = 0.2;
        public const double populationGenerationAmountMultiplier = 100;
        #endregion

        #region Default config values
        /// <summary>
        /// The default value for the config used for the values of <see cref="TerrainTypes"/>.
        /// </summary>
        private static readonly List<EnumValue<TerrainType>> _defaultTerrainTypes =
        [
            TerrainType.FIELD,
            TerrainType.MOUNTAIN,
            TerrainType.OCEAN,
            TerrainType.SHORE,
        ];

        /// <summary>
        /// The default value for the config used for the values of <see cref="StructureTypes"/>.
        /// </summary>
        private static readonly List<EnumValue<StructureType>> _defaultStructureTypes =
        [
            StructureType.NONE,
            StructureType.VILLAGE,
            StructureType.KINGDOM,
            StructureType.BANDIT_CAMP,
        ];

        /// <summary>
        /// The default value for the config used for the value of <see cref="TileNoiseOffsets"/>.
        /// </summary>
        private static readonly Dictionary<TileNoiseType, double> _defaultTileNoiseOffsets = new()
        {
            [TileNoiseType.HEIGHT] = 0,
            [TileNoiseType.TEMPERATURE] = 0,
            [TileNoiseType.HUMIDITY] = 0,
            [TileNoiseType.HOSTILITY] = -0.1,
            [TileNoiseType.POPULATION] = -0.1,
        };

        /// <summary>
        /// The default value for the config used for the value of <see cref="TerrainTypePropertyMap"/>.
        /// </summary>
        private static readonly Dictionary<Type, Dictionary<TileNoiseType, double>> _defaultTerrainTypePropertyMap = new()
        {
            [typeof(MountainTerrain)] = new Dictionary<TileNoiseType, double>()
            {
                [TileNoiseType.HEIGHT] = 1.0,
            },
            [typeof(FieldTerrain)] = new Dictionary<TileNoiseType, double>()
            {
                [TileNoiseType.HEIGHT] = 0.5,
            },
            [typeof(ShoreTerrain)] = new Dictionary<TileNoiseType, double>()
            {
                [TileNoiseType.HEIGHT] = 0.325,
            },
            [typeof(OceanTerrain)] = new Dictionary<TileNoiseType, double>()
            {
                [TileNoiseType.HEIGHT] = 0.29,
            },
        };

        /// <summary>
        /// The default value for the config used for the value of <see cref="StructureTypePropertyMap"/>.
        /// </summary>
        private static readonly Dictionary<Type, Dictionary<TileNoiseType, double>> _defaultStructureTypePropertyMap = new()
        {
            [typeof(NoStructure)] = new Dictionary<TileNoiseType, double>()
            {
                [TileNoiseType.POPULATION] = 0.0,
            },
            [typeof(BanditCampStructure)] = new Dictionary<TileNoiseType, double>()
            {
                [TileNoiseType.HOSTILITY] = 1.0,
                [TileNoiseType.POPULATION] = 0.3,
            },
            [typeof(VillageStructure)] = new Dictionary<TileNoiseType, double>()
            {
                [TileNoiseType.HOSTILITY] = 0.0,
                [TileNoiseType.POPULATION] = 0.6,
            },
            [typeof(KingdomStructure)] = new Dictionary<TileNoiseType, double>()
            {
                [TileNoiseType.HOSTILITY] = 0.0,
                [TileNoiseType.POPULATION] = 0.8,
            },
        };

        /// <summary>
        /// The default value for the config used for the value of <see cref="PopulationTypePropertyMap"/>.
        /// </summary>
        private static readonly Dictionary<EnumValue<EntityType>, Dictionary<TileNoiseType, double>> _defaultPopulationTypePropertyMap = new()
        {
            [EntityType.HUMAN] = new Dictionary<TileNoiseType, double>()
            {
                [TileNoiseType.HEIGHT] = 0.6,
                [TileNoiseType.TEMPERATURE] = 0.6,
                [TileNoiseType.HUMIDITY] = 0.4,
                [TileNoiseType.HOSTILITY] = 0.3,
            },
            [EntityType.ELF] = new Dictionary<TileNoiseType, double>()
            {
                [TileNoiseType.HEIGHT] = 1.0,
                [TileNoiseType.TEMPERATURE] = 0.5,
                [TileNoiseType.HUMIDITY] = 0.75,
                [TileNoiseType.HOSTILITY] = 0.3,
            },
            [EntityType.DWARF] = new Dictionary<TileNoiseType, double>()
            {
                [TileNoiseType.HEIGHT] = 0.1,
                [TileNoiseType.TEMPERATURE] = 0.6,
                [TileNoiseType.HUMIDITY] = 0.3,
                [TileNoiseType.HOSTILITY] = 0.6,
            },
            [EntityType.DEMON] = new Dictionary<TileNoiseType, double>()
            {
                [TileNoiseType.HEIGHT] = 0.1,
                [TileNoiseType.TEMPERATURE] = 0.9,
                [TileNoiseType.HUMIDITY] = 0.1,
                [TileNoiseType.HOSTILITY] = 0.9,
            },
        };
        
        /// <summary>
        /// The default value for the config used for the value of <see cref="TerrainTypeMap"/>.
        /// </summary>
        private static readonly Dictionary<EnumValue<TerrainType>, TerrainTypePropertiesDTO> _defaultTerrainTypeMap = new()
        {
            [TerrainType.FIELD] = new TerrainTypePropertiesDTO(TerrainType.FIELD, typeof(FieldTerrain)),
            [TerrainType.MOUNTAIN] = new TerrainTypePropertiesDTO(TerrainType.MOUNTAIN, typeof(MountainTerrain)),
            [TerrainType.OCEAN] = new TerrainTypePropertiesDTO(TerrainType.OCEAN, typeof(OceanTerrain), -0.1),
            [TerrainType.SHORE] = new TerrainTypePropertiesDTO(TerrainType.SHORE, typeof(ShoreTerrain), -0.05),
        };

        /// <summary>
        /// The default value for the config used for the value of <see cref="StructureTypeMap"/>.
        /// </summary>
        private static readonly Dictionary<EnumValue<StructureType>, StructureTypePropertiesDTO> _defaultStructureTypeMap = new()
        {
            [StructureType.NONE] = new StructureTypePropertiesDTO(StructureType.NONE, typeof(NoStructure), -0.1),
            [StructureType.BANDIT_CAMP] = new StructureTypePropertiesDTO(StructureType.BANDIT_CAMP, typeof(BanditCampStructure), fightChance: 0.75),
            [StructureType.VILLAGE] = new StructureTypePropertiesDTO(StructureType.VILLAGE, typeof(VillageStructure), fightChance: 0.01),
            [StructureType.KINGDOM] = new StructureTypePropertiesDTO(StructureType.KINGDOM, typeof(KingdomStructure), fightChance: 0.01),
        };
        #endregion

        #region Config values
        /// <summary>
        /// Offsets for tile noise values.
        /// </summary>
        internal static Dictionary<TileNoiseType, double> TileNoiseOffsets { get; set; }

        /// <summary>
        /// Dictionary to map terrain types to their ideal properties.
        /// </summary>
        internal static Dictionary<Type, Dictionary<TileNoiseType, double>> TerrainTypePropertyMap { get; set; }

        /// <summary>
        /// Dictionary to map structure types to their ideal properties.
        /// </summary>
        internal static Dictionary<Type, Dictionary<TileNoiseType, double>> StructureTypePropertyMap { get; set; }

        /// <summary>
        /// Dictionary to map entity types to their ideal properties.
        /// </summary>
        internal static Dictionary<EnumValue<EntityType>, Dictionary<TileNoiseType, double>> PopulationTypePropertyMap { get; set; }

        /// <summary>
        /// Dictionary to map terrain content types to their object types.
        /// </summary>
        internal static Dictionary<EnumValue<TerrainType>, TerrainTypePropertiesDTO> TerrainTypeMap { get; set; }

        /// <summary>
        /// Dictionary to map structure content types to their object types.
        /// </summary>
        internal static Dictionary<EnumValue<StructureType>, StructureTypePropertiesDTO> StructureTypeMap { get; set; }
        #endregion

        #region Constructors
        static WorldUtils()
        {
            LoadDefaultConfigs();
        }
        #endregion

        #region Public functions
        #region Configs
        #region Write default config or get reload common data
        private static (string configName, string? comment, bool paddingData) WriteDefaultConfigOrGetReloadDataTerrainTypes(bool isWriteConfig)
        {
            const string? comment = null;
            var basePath = Path.Join(Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "terrain_types");
            if (!isWriteConfig)
            {
                return (basePath, comment, false);
            }

            PACSingletons.Instance.ConfigManager.SetConfig(
                Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, basePath),
                null,
                _defaultTerrainTypes,
                comment
            );
            return default;
        }

        private static (string configName, string? comment, bool paddingData) WriteDefaultConfigOrGetReloadDataStructureTypes(bool isWriteConfig)
        {
            const string? comment = null;
            var basePath = Path.Join(Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "structure_types");
            if (!isWriteConfig)
            {
                return (basePath, comment, false);
            }

            PACSingletons.Instance.ConfigManager.SetConfig(
                Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, basePath),
                null,
                _defaultStructureTypes,
                comment
            );
            return default;
        }

        private static (string configName, string? comment, bool paddingData) WriteDefaultConfigOrGetReloadDataTileNoiseOffsets(bool isWriteConfig)
        {
            const string? comment = null;
            var basePath = Path.Join(Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "tile_noise_offsets");
            if (!isWriteConfig)
            {
                return (basePath, comment, false);
            }

            PACSingletons.Instance.ConfigManager.SetConfig(
                Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, basePath),
                null,
                _defaultTileNoiseOffsets,
                comment
            );
            return default;
        }

        private static (
            string configName,
            string? comment,
            Func<Type, string> serializeKeys
        ) WriteDefaultConfigOrGetReloadDataTerrainTypePropertyMap(bool isWriteConfig)
        {
            const string? comment = null;
            var basePath = Path.Join(Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "terrain_type_property_map");
            static string KeySerializer(Type key) => key.FullName
                ?? throw new ArgumentException($"Cannot get the name of the type: {key}");
            if (!isWriteConfig)
            {
                return (basePath, comment, KeySerializer);
            }

            PACSingletons.Instance.ConfigManager.SetConfigDict(
                    Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, basePath),
                    null,
                    _defaultTerrainTypePropertyMap,
                    KeySerializer,
                    comment
                );
            return default;
        }

        private static (
            string configName,
            string? comment,
            Func<Type, string> serializeKeys
        ) WriteDefaultConfigOrGetReloadDataStructureTypePropertyMap(bool isWriteConfig)
        {
            const string? comment = null;
            var basePath = Path.Join(Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "structure_type_property_map");
            static string KeySerializer(Type key) => key.FullName
                ?? throw new ArgumentException($"Cannot get the name of the type: {key}");
            if (!isWriteConfig)
            {
                return (basePath, comment, KeySerializer);
            }

            PACSingletons.Instance.ConfigManager.SetConfigDict(
                    Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, basePath),
                    null,
                    _defaultStructureTypePropertyMap,
                    KeySerializer,
                    comment
                );
            return default;
        }

        private static (
            string configName,
            string? comment,
            Func<EnumValue<EntityType>, string> serializeKeys
        ) WriteDefaultConfigOrGetReloadDataPopulationTypePropertyMap(bool isWriteConfig)
        {
            const string? comment = null;
            var basePath = Path.Join(Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "population_type_property_map");
            static string KeySerializer(EnumValue<EntityType> key) => key.Name
                ?? throw new ArgumentException($"Cannot get the name of the type: {key}");
            if (!isWriteConfig)
            {
                return (basePath, comment, KeySerializer);
            }

            PACSingletons.Instance.ConfigManager.SetConfigDict(
                    Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, basePath),
                    null,
                    _defaultPopulationTypePropertyMap,
                    KeySerializer,
                    comment
                );
            return default;
        }

        private static (
            string configName,
            string? comment,
            Func<EnumValue<TerrainType>, string> serializeKeys
        ) WriteDefaultConfigOrGetReloadDataTerrainTypeMap(bool isWriteConfig)
        {
            const string? comment = null;
            var basePath = Path.Join(Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "terrain_type_map");
            static string KeySerializer(EnumValue<TerrainType> key) => key.Name;
            if (!isWriteConfig)
            {
                return (basePath, comment, KeySerializer);
            }

            PACSingletons.Instance.ConfigManager.SetConfigDict(
                    Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, basePath),
                    null,
                    _defaultTerrainTypeMap,
                    KeySerializer,
                    comment
                );
            return default;
        }

        private static (
            string configName,
            string? comment,
            Func<EnumValue<StructureType>, string> serializeKeys
        ) WriteDefaultConfigOrGetReloadDataStructureTypeMap(bool isWriteConfig)
        {
            const string? comment = null;
            var basePath = Path.Join(Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "structure_type_map");
            static string KeySerializer(EnumValue<StructureType> key) => key.Name;
            if (!isWriteConfig)
            {
                return (basePath, comment, KeySerializer);
            }

            PACSingletons.Instance.ConfigManager.SetConfigDict(
                    Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, basePath),
                    null,
                    _defaultStructureTypeMap,
                    KeySerializer,
                    comment
                );
            return default;
        }
        #endregion

        /// <summary>
        /// Resets all variables that come from configs.
        /// </summary>
        public static void LoadDefaultConfigs()
        {
            Tools.LoadDefultAdvancedEnum(_defaultTerrainTypes);
            Tools.LoadDefultAdvancedEnum(_defaultStructureTypes);
            TileNoiseOffsets = _defaultTileNoiseOffsets;
            TerrainTypePropertyMap = _defaultTerrainTypePropertyMap;
            StructureTypePropertyMap = _defaultStructureTypePropertyMap;
            PopulationTypePropertyMap = _defaultPopulationTypePropertyMap;
            TerrainTypeMap = _defaultTerrainTypeMap;
            StructureTypeMap = _defaultStructureTypeMap;
        }

        /// <summary>
        /// Resets all config files to their default states.
        /// </summary>
        public static void WriteDefaultConfigs()
        {
            WriteDefaultConfigOrGetReloadDataTerrainTypes(true);
            WriteDefaultConfigOrGetReloadDataStructureTypes(true);
            WriteDefaultConfigOrGetReloadDataTileNoiseOffsets(true);
            WriteDefaultConfigOrGetReloadDataTerrainTypePropertyMap(true);
            WriteDefaultConfigOrGetReloadDataStructureTypePropertyMap(true);
            WriteDefaultConfigOrGetReloadDataPopulationTypePropertyMap(true);
            WriteDefaultConfigOrGetReloadDataTerrainTypeMap(true);
            WriteDefaultConfigOrGetReloadDataStructureTypeMap(true);
        }

        /// <summary>
        /// Reloads all values that come from configs.
        /// </summary>
        /// <param name="namespaceFolders">The name of the currently active config folders.</param>
        /// <param name="isVanillaInvalid">If the vanilla config is valid.</param>
        /// <param name="showProgressIndentation">If not null, shows the progress of loading the configs on the console.</param>
        public static void ReloadConfigs(
            List<(string folderName, string namespaceName)> namespaceFolders,
            bool isVanillaInvalid,
            int? showProgressIndentation = null
        )
        {
            Tools.ReloadConfigsFolderDisplayProgress(Constants.CONFIGS_WORLD_SUBFOLDER_NAME, showProgressIndentation);
            showProgressIndentation = showProgressIndentation + 1 ?? null;
            
            var defaultConfigOrGetReloadDataTerrainTypesData = WriteDefaultConfigOrGetReloadDataTerrainTypes(false);
            ConfigUtils.ReloadConfigsAggregateAdvancedEnum(
                defaultConfigOrGetReloadDataTerrainTypesData.configName,
                namespaceFolders,
                _defaultTerrainTypes,
                isVanillaInvalid,
                showProgressIndentation,
                true,
                comment: defaultConfigOrGetReloadDataTerrainTypesData.comment
            );

            var defaultConfigOrGetReloadDataStructureTypesData = WriteDefaultConfigOrGetReloadDataStructureTypes(false);
            ConfigUtils.ReloadConfigsAggregateAdvancedEnum(
                defaultConfigOrGetReloadDataStructureTypesData.configName,
                namespaceFolders,
                _defaultStructureTypes,
                isVanillaInvalid,
                showProgressIndentation,
                true,
                comment: defaultConfigOrGetReloadDataStructureTypesData.comment
            );

            var defaultConfigOrGetReloadDataTileNoiseOffsetsData = WriteDefaultConfigOrGetReloadDataTileNoiseOffsets(false);
            TileNoiseOffsets = ConfigUtils.ReloadConfigsAggregateDict(
                defaultConfigOrGetReloadDataTileNoiseOffsetsData.configName,
                namespaceFolders,
                _defaultTileNoiseOffsets,
                key => key.ToString(),
                Enum.Parse<TileNoiseType>,
                isVanillaInvalid,
                showProgressIndentation,
                comment: defaultConfigOrGetReloadDataTileNoiseOffsetsData.comment
            );

            var terrainContentTypePropertyMapData = WriteDefaultConfigOrGetReloadDataTerrainTypePropertyMap(false);
            TerrainTypePropertyMap = ConfigUtils.ReloadConfigsAggregateDict(
                terrainContentTypePropertyMapData.configName,
                namespaceFolders,
                _defaultTerrainTypePropertyMap,
                terrainContentTypePropertyMapData.serializeKeys,
                key => Utils.GetTypeFromName(key) ?? throw new JsonException($"Unknown type name: \"{key}\""),
                isVanillaInvalid,
                showProgressIndentation,
                comment: terrainContentTypePropertyMapData.comment
            );

            var structureContentTypePropertyMapData = WriteDefaultConfigOrGetReloadDataStructureTypePropertyMap(false);
            StructureTypePropertyMap = ConfigUtils.ReloadConfigsAggregateDict(
                structureContentTypePropertyMapData.configName,
                namespaceFolders,
                _defaultStructureTypePropertyMap,
                structureContentTypePropertyMapData.serializeKeys,
                key => Utils.GetTypeFromName(key) ?? throw new JsonException($"Unknown type name: \"{key}\""),
                isVanillaInvalid,
                showProgressIndentation,
                comment: structureContentTypePropertyMapData.comment
            );

            var populationContentTypePropertyMapData = WriteDefaultConfigOrGetReloadDataPopulationTypePropertyMap(false);
            PopulationTypePropertyMap = ConfigUtils.ReloadConfigsAggregateDict(
                populationContentTypePropertyMapData.configName,
                namespaceFolders,
                _defaultPopulationTypePropertyMap,
                populationContentTypePropertyMapData.serializeKeys,
                key => EntityType.GetValue(ConfigUtils.GetNameapacedString(key)),
                isVanillaInvalid,
                showProgressIndentation,
                comment: populationContentTypePropertyMapData.comment
            );

            var terrainContentTypeMapData = WriteDefaultConfigOrGetReloadDataTerrainTypeMap(false);
            TerrainTypeMap = ConfigUtils.ReloadConfigsAggregateDict(
                terrainContentTypeMapData.configName,
                namespaceFolders,
                _defaultTerrainTypeMap,
                terrainContentTypeMapData.serializeKeys,
                key => TerrainType.GetValue(ConfigUtils.GetNameapacedString(key)),
                isVanillaInvalid,
                showProgressIndentation,
                comment: terrainContentTypeMapData.comment
            );

            var structureContentTypeMapData = WriteDefaultConfigOrGetReloadDataStructureTypeMap(false);
            StructureTypeMap = ConfigUtils.ReloadConfigsAggregateDict(
                structureContentTypeMapData.configName,
                namespaceFolders,
                _defaultStructureTypeMap,
                structureContentTypeMapData.serializeKeys,
                key => StructureType.GetValue(ConfigUtils.GetNameapacedString(key)),
                isVanillaInvalid,
                showProgressIndentation,
                comment: structureContentTypeMapData.comment
            );
        }
        #endregion

        /// <summary>
        /// Calculates the noise values for each perlin noise generator at a specific point, and normalises it between 0 and 1.
        /// </summary>
        /// <param name="absoluteX">The absolute x coordinate of the Tile.</param>
        /// <param name="absoluteY">The absolute y coordinate of the Tile.</param>
        public static Dictionary<TileNoiseType, double> GetNoiseValues(long absoluteX, long absoluteY)
        {
            var noiseValues = new Dictionary<TileNoiseType, double>();
            foreach (var noiseGeneratorEntry in RandomStates.Instance.TileTypeNoiseGenerators)
            {
                var noiseKey = noiseGeneratorEntry.Key;
                var noiseGenerator = noiseGeneratorEntry.Value;
                var noiseValue = noiseGenerator.Generate(absoluteX, absoluteY, 16.0 / Constants.TILE_NOISE_DIVISION) * 1;
                noiseValue += noiseGenerator.Generate(absoluteX, absoluteY, 8.0 / Constants.TILE_NOISE_DIVISION) * 2;
                noiseValue += noiseGenerator.Generate(absoluteX, absoluteY, 4.0 / Constants.TILE_NOISE_DIVISION) * 4;
                noiseValue += noiseGenerator.Generate(absoluteX, absoluteY, 2.0 / Constants.TILE_NOISE_DIVISION) * 8;
                noiseValue += noiseGenerator.Generate(absoluteX, absoluteY, 1.0 / Constants.TILE_NOISE_DIVISION) * 16;
                noiseValue /= 31;
                noiseValues[noiseKey] = noiseValue;
            }
            return noiseValues;
        }

        /// <summary>
        /// Shifts the noise values, by their offsets.
        /// </summary>
        /// <param name="noiseValues">The noise values to be moidified.</param>
        public static void ShiftNoiseValues(IDictionary<TileNoiseType, double> noiseValues)
        {
            foreach (var key in noiseValues.Keys)
            {
                if (TileNoiseOffsets.TryGetValue(key, out var offset))
                {
                    noiseValues[key] += offset;
                }
            }
        }

        /// <summary>
        /// Calculates the best tile type for the space depending on the perlin noise values.
        /// </summary>
        /// <param name="noiseValues">The list of noise values for each perlin noise generator.</param>
        /// <param name="noStructureDLOverride">Overrides the default limit for choosing no structure, if the noise value difference is over this limit.</param>
        private static Type CalculateClosestContentType(
            Dictionary<Type, Dictionary<TileNoiseType, double>> contentPropertiesMap,
            IDictionary<TileNoiseType, double> noiseValues,
            double? noStructureDLOverride = null
        )
        {
            noStructureDLOverride ??= noStructureDifferenceLimit;
            var minDiffContentType = contentPropertiesMap.Keys.First();
            var minDiff = double.MaxValue;
            foreach (var propertyEntry in contentPropertiesMap)
            {
                var properties = propertyEntry.Value;
                var sumDiff = 0.0;
                var propertyNum = 0;
                foreach (var propertyKey in properties.Keys)
                {
                    if (noiseValues.TryGetValue(propertyKey, out double noiseValue))
                    {
                        sumDiff += Math.Abs(properties[propertyKey] - noiseValue);
                        propertyNum++;
                    }
                }
                var propDif = sumDiff / propertyNum;
                if (propDif < minDiff)
                {
                    minDiff = propDif;
                    minDiffContentType = propertyEntry.Key;
                }
            }

            // no content if difference is too big
            if (contentPropertiesMap == StructureTypePropertyMap && minDiff >= noStructureDLOverride)
            {
                minDiffContentType = typeof(NoStructure);
            }
            return minDiffContentType;
        }

        /// <inheritdoc cref="CalculateClosestContentType(Dictionary{Type, Dictionary{TileNoiseType, double}}, IDictionary{TileNoiseType, double}, double?)"/>
        public static Type CalculateClosestTerrainType(
            IDictionary<TileNoiseType, double> noiseValues
        )
        {
            return CalculateClosestContentType(TerrainTypePropertyMap, noiseValues);
        }

        /// <inheritdoc cref="CalculateClosestContentType(Dictionary{Type, Dictionary{TileNoiseType, double}}, IDictionary{TileNoiseType, double}, double?)"/>
        public static Type CalculateClosestStructureType(
            IDictionary<TileNoiseType, double> noiseValues,
            double? noStructureDLOverride = null
        )
        {
            return CalculateClosestContentType(StructureTypePropertyMap, noiseValues, noStructureDLOverride);
        }

        /// <summary>
        /// Calculates the best terrain for the tile depending on the perlin noise values.
        /// </summary>
        /// <param name="chunkRandom">The parrent chunk's random generator.</param>
        /// <param name="noiseValues">The list of noise values for each perlin noise generator.</param>
        /// <exception cref="InvalidOperationException">Thrown if the terrain type cannot be created.</exception>
        public static TerrainContent CalculateBestFitTerrain(SplittableRandom chunkRandom, IDictionary<TileNoiseType, double> noiseValues)
        {
            var minDiffContentType = CalculateClosestTerrainType(noiseValues);
            return (TerrainContent)(Activator.CreateInstance(minDiffContentType, [chunkRandom, null, null])
                ?? throw new InvalidOperationException("Couldn't create terrain object from type!"));
        }

        /// <summary>
        /// Calculates the best structure for the tile depending on the perlin noise values.
        /// </summary>
        /// <param name="chunkRandom">The parrent chunk's random generator.</param>
        /// <param name="noiseValues">The list of noise values for each perlin noise generator.</param>
        /// <param name="noStructureDLOverride">Overrides the default limit for choosing no structure, if the noise value difference is over this limit.</param>
        /// <exception cref="InvalidOperationException">Thrown if the structure type cannot be created.</exception>
        public static StructureContent CalculateBestFitStructure(
            SplittableRandom chunkRandom,
            IDictionary<TileNoiseType, double> noiseValues,
            double? noStructureDLOverride = null
        )
        {
            var minDiffContentType = CalculateClosestStructureType(noiseValues, noStructureDLOverride);
            return (StructureContent)(Activator.CreateInstance(minDiffContentType, [chunkRandom, null, null])
                ?? throw new InvalidOperationException("Couldn't create structure object from type!"));
        }

        /// <summary>
        /// Calculates the fit differences for all entity types for the tile depending on the perlin noise values.
        /// </summary>
        /// <param name="noiseValues">The list of noise values for each perlin noise generator.</param>
        /// <param name="noPopulationDLOverride">Overrides the default limit for not having a population of that type, if the noise value difference is over this limit.</param>
        public static Dictionary<EnumValue<EntityType>, double> CalculatePopulationFitDifferences(
            IDictionary<TileNoiseType, double> noiseValues,
            double? noPopulationDLOverride = null
        )
        {
            noPopulationDLOverride ??= noPopulationDifferenceLimit;
            var entityCountDistributions = new Dictionary<EnumValue<EntityType>, double>();
            var allSumDiff = 0.0;
            foreach (var propertyEntry in PopulationTypePropertyMap)
            {
                var properties = propertyEntry.Value;
                var sumDiff = 0.0;
                var propertyNum = 0;
                foreach (var property in properties)
                {
                    if (noiseValues.TryGetValue(property.Key, out double noiseValue))
                    {
                        sumDiff += Math.Abs(property.Value - noiseValue);
                        propertyNum++;
                    }
                }
                var propDif = sumDiff / propertyNum;
                if (propDif < noPopulationDLOverride)
                {
                    entityCountDistributions[propertyEntry.Key] = propDif;
                    allSumDiff += propDif;
                }
            }

            return entityCountDistributions;
        }

        /// <summary>
        /// Calculates the distribution of entity types for the tile depending on the perlin noise values.
        /// </summary>
        /// <param name="noiseValues">The list of noise values for each perlin noise generator.</param>
        /// <param name="noPopulationDLOverride">Overrides the default limit for not having a population of that type, if the noise value difference is over this limit.</param>
        public static Dictionary<EnumValue<EntityType>, int> CalculatePopulation(
            IDictionary<TileNoiseType, double> noiseValues,
            double? noPopulationDLOverride = null
        )
        {
            var entityCountDistributions = CalculatePopulationFitDifferences(noiseValues, noPopulationDLOverride);
            var allSumDiff = entityCountDistributions.Values.Sum();

            return entityCountDistributions
                .Select(d => new KeyValuePair<EnumValue<EntityType>, int>(
                    d.Key,
                    (int)((allSumDiff - d.Value) / allSumDiff * populationGenerationAmountMultiplier)
                ))
                .Where(v => v.Value > 0)
                .ToDictionary(
                    k => k.Key,
                    v => v.Value
                );
        }

        /// <summary>
        /// NEEDS TO BE REWORKED SOON!!!<br/>
        /// Returs the content properties (for terrain and structures), if the string is the string representation of a content type.
        /// </summary>
        /// <typeparam name="TEnum">The enum type of the content.</typeparam>
        /// <param name="contentTypeString">The string representation of the content type.</param>
        public static ContentTypePropertiesDTO? ContentTypeStrToProperties<TEnum>(string? contentTypeString)
            where TEnum : AdvancedEnum<TEnum>
        {
            if (contentTypeString is null)
            {
                return null;
            }

            if (
                typeof(TEnum) == typeof(TerrainType) &&
                TerrainType.TryGetValue(contentTypeString, out var terrainType) &&
                TerrainTypeMap.TryGetValue(terrainType, out var terrainProps)
            )
            {
                return terrainProps;
            }
            else if (
                typeof(TEnum) == typeof(StructureType) &&
                StructureType.TryGetValue(contentTypeString, out var structureType) &&
                StructureTypeMap.TryGetValue(structureType, out var structureProps)
            )
            {
                return structureProps;
            }
            return null;
        }

        /// <summary>
        /// NEEDS TO BE REWORKED SOON!!!<br/>
        /// Tries to convert the string representation of the content type (terrain/structure) to content properties, and returns the success.
        /// </summary>
        /// <typeparam name="TEnum">The enum type of the content.</typeparam>
        /// <param name="contentTypeString">The string representation of the content type.</param>
        /// <param name="contentProperties">The resulting content properties.</param>
        public static bool TryParseContentTypeStrToProperties<TEnum>(
            string? contentTypeString,
            [NotNullWhen(true)] out ContentTypePropertiesDTO? contentProperties
        )
            where TEnum : AdvancedEnum<TEnum>
        {
            contentProperties = ContentTypeStrToProperties<TEnum>(contentTypeString);
            return contentProperties is not null;
        }

        /// <summary>
        /// NEEDS TO BE REWORKED SOON!!!<br/>
        /// Tries to convert the string representation of the content type (terrain/structure) to content properties, and returns the success.
        /// </summary>
        /// <typeparam name="TType">The enum type of the content.</typeparam>
        /// <param name="contentTypeMap">The matching content type map.</param>
        /// <param name="classType">The type of the content class.</param>
        public static EnumValue<TType> GetContentTypeFromClassType<TType, TProps>(
            Dictionary<EnumValue<TType>, TProps> contentTypeMap,
            Type classType
        )
            where TType : AdvancedEnum<TType>
            where TProps : ContentTypePropertiesDTO
        {
            return contentTypeMap.FirstOrDefault(ct => ct.Value.matchingType == classType).Key
                ?? throw new KeyNotFoundException($"No content type found for {classType.FullName} type in {typeof(TType)} property map.");
        }
        #endregion
    }
}
