using NPrng.Generators;
using PACommon;
using PACommon.Enums;
using PACommon.Extensions;
using ProgressAdventure.ConfigManagement;
using ProgressAdventure.Enums;
using ProgressAdventure.WorldManagement.Content;
using ProgressAdventure.WorldManagement.Content.Population;
using ProgressAdventure.WorldManagement.Content.Structure;
using ProgressAdventure.WorldManagement.Content.Terrain;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

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
        #endregion

        #region Constatnts
        /// <summary>
        /// If difference is larger than this the structure will not generate.
        /// </summary>
        public static readonly double noStructureDifferenceLimit = 0.3;
        /// <summary>
        /// If difference is larger than this the population will not generate.
        /// </summary>
        public static readonly double noPopulationDifferenceLimit = 0.2;
        #endregion

        #region Default config values

        /// <summary>
        /// The default value for the config used for the values of <see cref="ContentType"/>.
        /// </summary>
        private static readonly List<EnumTreeValue<ContentType>> _defaultContentTypes =
        [
            // terrains
            ContentType.Terrain.FIELD,
            ContentType.Terrain.MOUNTAIN,
            ContentType.Terrain.OCEAN,
            ContentType.Terrain.SHORE,
            // structures
            ContentType.Structure.NONE,
            ContentType.Structure.VILLAGE,
            ContentType.Structure.KINGDOM,
            ContentType.Structure.BANDIT_CAMP,
            // population
            ContentType.Population.NONE,
            ContentType.Population.HUMAN,
            ContentType.Population.DWARF,
            ContentType.Population.ELF,
            ContentType.Population.DEMON,
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
        /// The default value for the config used for the value of <see cref="TerrainContentTypePropertyMap"/>.
        /// </summary>
        private static readonly Dictionary<Type, Dictionary<TileNoiseType, double>> _defaultTerrainContentTypePropertyMap = new()
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
        /// The default value for the config used for the value of <see cref="StructureContentTypePropertyMap"/>.
        /// </summary>
        private static readonly Dictionary<Type, Dictionary<TileNoiseType, double>> _defaultStructureContentTypePropertyMap = new()
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
        /// The default value for the config used for the value of <see cref="PopulationContentTypePropertyMap"/>.
        /// </summary>
        private static readonly Dictionary<Type, Dictionary<TileNoiseType, double>> _defaultPopulationContentTypePropertyMap = new()
        {
            [typeof(NoPopulation)] = new Dictionary<TileNoiseType, double>()
            {
                [TileNoiseType.POPULATION] = 0.1,
            },
            [typeof(HumanPopulation)] = new Dictionary<TileNoiseType, double>()
            {
                [TileNoiseType.HEIGHT] = 0.6,
                [TileNoiseType.TEMPERATURE] = 0.6,
                [TileNoiseType.HUMIDITY] = 0.4,
                [TileNoiseType.HOSTILITY] = 0.3,
            },
            [typeof(ElfPopulation)] = new Dictionary<TileNoiseType, double>()
            {
                [TileNoiseType.HEIGHT] = 1.0,
                [TileNoiseType.TEMPERATURE] = 0.5,
                [TileNoiseType.HUMIDITY] = 0.75,
                [TileNoiseType.HOSTILITY] = 0.3,
            },
            [typeof(DwarfPopulation)] = new Dictionary<TileNoiseType, double>()
            {
                [TileNoiseType.HEIGHT] = 0.1,
                [TileNoiseType.TEMPERATURE] = 0.6,
                [TileNoiseType.HUMIDITY] = 0.3,
                [TileNoiseType.HOSTILITY] = 0.6,
            },
            [typeof(DemonPopulation)] = new Dictionary<TileNoiseType, double>()
            {
                [TileNoiseType.HEIGHT] = 0.1,
                [TileNoiseType.TEMPERATURE] = 0.9,
                [TileNoiseType.HUMIDITY] = 0.1,
                [TileNoiseType.HOSTILITY] = 0.9,
            },
        };
        
        /// <summary>
        /// The default value for the config used for the value of <see cref="BaseContentTypeMap"/>.
        /// </summary>
        private static readonly Dictionary<EnumTreeValue<ContentType>, ContentTypePropertiesDTO> _baseContentTypeMap = new()
        {
            [ContentType._TERRAIN] = new ContentTypePropertiesDTO(ContentType._TERRAIN, typeof(TerrainContent)),
            [ContentType._STRUCTURE] = new ContentTypePropertiesDTO(ContentType._STRUCTURE, typeof(StructureContent)),
            [ContentType._POPULATION] = new ContentTypePropertiesDTO(ContentType._POPULATION, typeof(PopulationContent)),
        };
        
        /// <summary>
        /// The default value for the config used for the value of <see cref="TerrainContentTypeMap"/>.
        /// </summary>
        private static readonly Dictionary<EnumTreeValue<ContentType>, ContentTypePropertiesDTO> _defaultTerrainContentTypeMap = new()
        {
            [ContentType.Terrain.FIELD] = new ContentTypePropertiesDTO(ContentType.Terrain.FIELD, typeof(FieldTerrain)),
            [ContentType.Terrain.MOUNTAIN] = new ContentTypePropertiesDTO(ContentType.Terrain.MOUNTAIN, typeof(MountainTerrain)),
            [ContentType.Terrain.OCEAN] = new ContentTypePropertiesDTO(ContentType.Terrain.OCEAN, typeof(OceanTerrain)),
            [ContentType.Terrain.SHORE] = new ContentTypePropertiesDTO(ContentType.Terrain.SHORE, typeof(ShoreTerrain)),
        };

        /// <summary>
        /// The default value for the config used for the value of <see cref="StructureContentTypeMap"/>.
        /// </summary>
        private static readonly Dictionary<EnumTreeValue<ContentType>, ContentTypePropertiesDTO> _defaultStructureContentTypeMap = new()
        {
            [ContentType.Structure.NONE] = new ContentTypePropertiesDTO(ContentType.Structure.NONE, typeof(NoStructure)),
            [ContentType.Structure.BANDIT_CAMP] = new ContentTypePropertiesDTO(ContentType.Structure.BANDIT_CAMP, typeof(BanditCampStructure)),
            [ContentType.Structure.VILLAGE] = new ContentTypePropertiesDTO(ContentType.Structure.VILLAGE, typeof(VillageStructure)),
            [ContentType.Structure.KINGDOM] = new ContentTypePropertiesDTO(ContentType.Structure.KINGDOM, typeof(KingdomStructure)),
        };

        /// <summary>
        /// The default value for the config used for the value of <see cref="PopulationContentTypeMap"/>.
        /// </summary>
        private static readonly Dictionary<EnumTreeValue<ContentType>, ContentTypePropertiesDTO> _defaultPopulationContentTypeMap = new()
        {
            [ContentType.Population.NONE] = new ContentTypePropertiesDTO(ContentType.Population.NONE, typeof(NoPopulation)),
            [ContentType.Population.HUMAN] = new ContentTypePropertiesDTO(ContentType.Population.HUMAN, typeof(HumanPopulation)),
            [ContentType.Population.ELF] = new ContentTypePropertiesDTO(ContentType.Population.ELF, typeof(ElfPopulation)),
            [ContentType.Population.DWARF] = new ContentTypePropertiesDTO(ContentType.Population.DWARF, typeof(DwarfPopulation)),
            [ContentType.Population.DEMON] = new ContentTypePropertiesDTO(ContentType.Population.DEMON, typeof(DemonPopulation)),
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
        internal static Dictionary<Type, Dictionary<TileNoiseType, double>> TerrainContentTypePropertyMap { get; set; }

        /// <summary>
        /// Dictionary to map terrain types to their ideal properties.
        /// </summary>
        internal static Dictionary<Type, Dictionary<TileNoiseType, double>> StructureContentTypePropertyMap { get; set; }

        /// <summary>
        /// Dictionary to map terrain types to their ideal properties.
        /// </summary>
        internal static Dictionary<Type, Dictionary<TileNoiseType, double>> PopulationContentTypePropertyMap { get; set; }

        /// <summary>
        /// Dictionary to map content types to their property maps.
        /// </summary>
        internal static readonly Dictionary<Type, Dictionary<Type, Dictionary<TileNoiseType, double>>> contentTypePropertyMap = new()
        {
            [typeof(TerrainContent)] = null,
            [typeof(StructureContent)] = null,
            [typeof(PopulationContent)] = null,
        };

        /// <summary>
        /// Dictionary to map base content types to their content properties.
        /// </summary>
        internal static Dictionary<EnumTreeValue<ContentType>, ContentTypePropertiesDTO> BaseContentTypeMap { get; set; }

        /// <summary>
        /// Dictionary to map content types to their content subtype property maps.
        /// </summary>
        internal static readonly Dictionary<EnumTreeValue<ContentType>, Dictionary<EnumTreeValue<ContentType>, ContentTypePropertiesDTO>> contentTypeSubtypesMap = new()
        {
            [ContentType._TERRAIN] = null,
            [ContentType._STRUCTURE] = null,
            [ContentType._POPULATION] = null,
        };

        /// <summary>
        /// Dictionary to map terrain content types to their type properties.
        /// </summary>
        internal static Dictionary<EnumTreeValue<ContentType>, ContentTypePropertiesDTO> TerrainContentTypeMap { get; set; }

        /// <summary>
        /// Dictionary to map structure content types to their type properties.
        /// </summary>
        internal static Dictionary<EnumTreeValue<ContentType>, ContentTypePropertiesDTO> StructureContentTypeMap { get; set; }

        /// <summary>
        /// Dictionary to map population content types to their type properties.
        /// </summary>
        internal static Dictionary<EnumTreeValue<ContentType>, ContentTypePropertiesDTO> PopulationContentTypeMap { get; set; }
        #endregion

        #region Constructors
        static WorldUtils()
        {
            LoadDefaultConfigs();
        }
        #endregion

        #region Public functions
        #region Configs
        private static void UpdateNonConfigDicts()
        {
            contentTypeSubtypesMap[ContentType._TERRAIN] = TerrainContentTypeMap;
            contentTypeSubtypesMap[ContentType._STRUCTURE] = StructureContentTypeMap;
            contentTypeSubtypesMap[ContentType._POPULATION] = PopulationContentTypeMap;

            contentTypePropertyMap[typeof(TerrainContent)] = TerrainContentTypePropertyMap;
            contentTypePropertyMap[typeof(StructureContent)] = StructureContentTypePropertyMap;
            contentTypePropertyMap[typeof(PopulationContent)] = PopulationContentTypePropertyMap;
        }

        /// <summary>
        /// Resets all variables that come from configs.
        /// </summary>
        public static void LoadDefaultConfigs()
        {
            Tools.LoadDefultAdvancedEnumTree(_defaultContentTypes);
            TileNoiseOffsets = _defaultTileNoiseOffsets;
            TerrainContentTypePropertyMap = _defaultTerrainContentTypePropertyMap;
            StructureContentTypePropertyMap = _defaultStructureContentTypePropertyMap;
            PopulationContentTypePropertyMap = _defaultPopulationContentTypePropertyMap;
            BaseContentTypeMap = _baseContentTypeMap;
            TerrainContentTypeMap = _defaultTerrainContentTypeMap;
            StructureContentTypeMap = _defaultStructureContentTypeMap;
            PopulationContentTypeMap = _defaultPopulationContentTypeMap;
            UpdateNonConfigDicts();
        }

        /// <summary>
        /// Resets all config files to their default states.
        /// </summary>
        public static void WriteDefaultConfigs()
        {
            PACSingletons.Instance.ConfigManager.SetConfig(
                Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "content_types"),
                null,
                _defaultContentTypes
            );

            PACSingletons.Instance.ConfigManager.SetConfig(
                Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "tile_noise_offsets"),
                null,
                _defaultTileNoiseOffsets
            );

            PACSingletons.Instance.ConfigManager.SetConfigDict(
                Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "terrain_content_type_property_map"),
                null,
                _defaultTerrainContentTypePropertyMap,
                key => key.FullName ?? ""
            );

            PACSingletons.Instance.ConfigManager.SetConfigDict(
                Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "structure_content_type_property_map"),
                null,
                _defaultStructureContentTypePropertyMap,
                key => key.FullName ?? ""
            );

            PACSingletons.Instance.ConfigManager.SetConfigDict(
                Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "population_content_type_property_map"),
                null,
                _defaultPopulationContentTypePropertyMap,
                key => key.FullName ?? ""
            );

            PACSingletons.Instance.ConfigManager.SetConfigDict(
                Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "base_content_type_map"),
                null,
                _baseContentTypeMap,
                key => key.ToString()!
            );

            PACSingletons.Instance.ConfigManager.SetConfigDict(
                Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "terrain_content_type_map"),
                null,
                _defaultTerrainContentTypeMap,
                key => key.ToString()!
            );

            PACSingletons.Instance.ConfigManager.SetConfigDict(
                Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "structure_content_type_map"),
                null,
                _defaultStructureContentTypeMap,
                key => key.ToString()!
            );

            PACSingletons.Instance.ConfigManager.SetConfigDict(
                Path.Join(Constants.VANILLA_CONFIGS_NAMESPACE, Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "population_content_type_map"),
                null,
                _defaultPopulationContentTypeMap,
                key => key.ToString()!
            );
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

            ConfigUtils.ReloadConfigsAggregateAdvancedEnumTree(
                Path.Join(Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "content_types"),
                namespaceFolders,
                _defaultContentTypes,
                isVanillaInvalid,
                showProgressIndentation,
                true
            );

            TileNoiseOffsets = ConfigUtils.ReloadConfigsAggregateDict(
                Path.Join(Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "tile_noise_offsets"),
                namespaceFolders,
                _defaultTileNoiseOffsets,
                key => key.ToString(),
                Enum.Parse<TileNoiseType>,
                isVanillaInvalid,
                showProgressIndentation
            );

            TerrainContentTypePropertyMap = ConfigUtils.ReloadConfigsAggregateDict(
                Path.Join(Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "terrain_content_type_property_map"),
                namespaceFolders,
                _defaultTerrainContentTypePropertyMap,
                key => key.FullName ?? "",
                key => Utils.GetTypeFromName(key) ?? throw new JsonException($"Unknown type name: \"{key}\""),
                isVanillaInvalid,
                showProgressIndentation
            );

            StructureContentTypePropertyMap = ConfigUtils.ReloadConfigsAggregateDict(
                Path.Join(Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "structure_content_type_property_map"),
                namespaceFolders,
                _defaultStructureContentTypePropertyMap,
                key => key.FullName ?? "",
                key => Utils.GetTypeFromName(key) ?? throw new JsonException($"Unknown type name: \"{key}\""),
                isVanillaInvalid,
                showProgressIndentation
            );

            PopulationContentTypePropertyMap = ConfigUtils.ReloadConfigsAggregateDict(
                Path.Join(Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "population_content_type_property_map"),
                namespaceFolders,
                _defaultPopulationContentTypePropertyMap,
                key => key.FullName ?? "",
                key => Utils.GetTypeFromName(key) ?? throw new JsonException($"Unknown type name: \"{key}\""),
                isVanillaInvalid,
                showProgressIndentation
            );

            BaseContentTypeMap = ConfigUtils.ReloadConfigsAggregateDict(
                Path.Join(Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "base_content_type_map"),
                namespaceFolders,
                _baseContentTypeMap,
                key => key.FullName,
                key => ParseContentTypeFromRealName(ConfigUtils.GetNamepsacedString(key))
                    ?? throw new JsonException($"Unknown content type real name: \"{key}\""),
                isVanillaInvalid,
                showProgressIndentation
            );

            TerrainContentTypeMap = ConfigUtils.ReloadConfigsAggregateDict(
                Path.Join(Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "terrain_content_type_map"),
                namespaceFolders,
                _defaultTerrainContentTypeMap,
                key => key.FullName,
                key => ParseContentTypeFromRealName(ConfigUtils.GetNamepsacedString(key))
                    ?? throw new JsonException($"Unknown content type real name: \"{key}\""),
                isVanillaInvalid,
                showProgressIndentation
            );

            StructureContentTypeMap = ConfigUtils.ReloadConfigsAggregateDict(
                Path.Join(Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "structure_content_type_map"),
                namespaceFolders,
                _defaultStructureContentTypeMap,
                key => key.FullName,
                key => ParseContentTypeFromRealName(ConfigUtils.GetNamepsacedString(key))
                    ?? throw new JsonException($"Unknown content type real name: \"{key}\""),
                isVanillaInvalid,
                showProgressIndentation
            );

            PopulationContentTypeMap = ConfigUtils.ReloadConfigsAggregateDict(
                Path.Join(Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "population_content_type_map"),
                namespaceFolders,
                _defaultPopulationContentTypeMap,
                key => key.FullName,
                key => ParseContentTypeFromRealName(ConfigUtils.GetNamepsacedString(key))
                    ?? throw new JsonException($"Unknown content type real name: \"{key}\""),
                isVanillaInvalid,
                showProgressIndentation
            );

            UpdateNonConfigDicts();
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
                noiseValues[key] += TileNoiseOffsets[key];
            }
        }

        /// <summary>
        /// Calculates the best tile type for the space depending on the perlin noise values.
        /// </summary>
        /// <typeparam name="T">The content type to reurn.</typeparam>
        /// <param name="noiseValues">The list of noise values for each perlin noise generator.</param>
        /// <param name="noStructureDLOverride">Overrides the default limit for choosing no structure, if the noise value difference is over this limit.</param>
        /// <param name="noPopulationDLOverride">Overrides the default limit for choosing no population, if the noise value difference is over this limit.</param>
        public static Type CalculateClosestContentType<T>(IDictionary<TileNoiseType, double> noiseValues, double? noStructureDLOverride = null, double? noPopulationDLOverride = null)
            where T : BaseContent
        {
            noStructureDLOverride ??= noStructureDifferenceLimit;
            noPopulationDLOverride ??= noPopulationDifferenceLimit;
            var contentProperties = contentTypePropertyMap[typeof(T)];
            var minDiffContentType = contentProperties.Keys.First();
            var minDiff = 1000000.0;
            foreach (var propertyEntry in contentProperties)
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
            if (contentProperties == StructureContentTypePropertyMap && minDiff >= noStructureDLOverride)
            {
                minDiffContentType = typeof(NoStructure);
            }
            else if (contentProperties == PopulationContentTypePropertyMap && minDiff >= noPopulationDLOverride)
            {
                minDiffContentType = typeof(NoPopulation);
            }
            return minDiffContentType;
        }

        /// <summary>
        /// Calculates the best content for the space depending on the perlin noise values.
        /// </summary>
        /// <typeparam name="T">The content type to reurn.</typeparam>
        /// <param name="chunkRandom">The parrent chunk's random generator.</param>
        /// <param name="noiseValues">The list of noise values for each perlin noise generator.</param>
        /// <param name="noStructureDLOverride">Overrides the default limit for choosing no structure, if the noise value difference is over this limit.</param>
        /// <param name="noPopulationDLOverride">Overrides the default limit for choosing no population, if the noise value difference is over this limit.</param>
        /// <exception cref="ArgumentNullException">Thrown if the content type cannot be created.</exception>
        public static T CalculateClosestContent<T>(SplittableRandom chunkRandom, IDictionary<TileNoiseType, double> noiseValues, double? noStructureDLOverride = null, double? noPopulationDLOverride = null)
            where T : BaseContent
        {
            var minDiffContentType = CalculateClosestContentType<T>(noiseValues, noStructureDLOverride, noPopulationDLOverride);
            var contentObj = Activator.CreateInstance(minDiffContentType, [chunkRandom, null, null]) ?? throw new ArgumentNullException(message: "Couldn't create content object from type!", null);
            return (T)contentObj;
        }

        /// <summary>
        /// Returs the content properties, if the string is the ttring representation of a content subtype.
        /// </summary>
        /// <param name="parrentContentType">The parrent content type ID.</param>
        /// <param name="contentSubtypeString">The string representation of the subtype content.</param>
        public static ContentTypePropertiesDTO? ToContentTypeProperties(EnumTreeValue<ContentType> parrentContentType, string? contentSubtypeString)
        {
            if (
                contentSubtypeString is not null &&
                contentTypeSubtypesMap.TryGetValue(parrentContentType, out Dictionary<EnumTreeValue<ContentType>, ContentTypePropertiesDTO>? subtypePropertesMap) &&
                subtypePropertesMap.FirstOrDefault(subtypeMap => subtypeMap.Value.typeName == contentSubtypeString)
                    is KeyValuePair<EnumTreeValue<ContentType>, ContentTypePropertiesDTO> contentProperties &&
                contentProperties.Value is not null
            )
            {
                return contentProperties.Value;
            }
            return null;
        }

        /// <summary>
        /// Tries to convert the string representation of the subtype content to content properties, and returns the success.
        /// </summary>
        /// <param name="parrentContentType">The parrent content type ID.</param>
        /// <param name="contentSubtypeString">The string representation of the subtype content.</param>
        /// <param name="contentProperties">The resulting content properties.</param>
        public static bool TryParseContentType(
            EnumTreeValue<ContentType> parrentContentType,
            string? contentSubtypeString,
            [NotNullWhen(true)] out ContentTypePropertiesDTO? contentProperties
        )
        {
            contentProperties = ToContentTypeProperties(parrentContentType, contentSubtypeString);
            return contentProperties is not null;
        }

        /// <summary>
        /// Returs the content type, if the content name is a name for a content type.
        /// </summary>
        /// <param name="contentTypeFullName">The full name of the content.</param>
        public static EnumTreeValue<ContentType>? ParseContentTypeFromRealName(string? contentTypeFullName)
        {
            if (string.IsNullOrWhiteSpace(contentTypeFullName))
            {
                return null;
            }
            return ContentType.GetAllValues().FirstOrDefault(content => content.FullName == contentTypeFullName);
        }

        /// <summary>
        /// Converts the content type, to it's default display name.
        /// </summary>
        /// <param name="contentType">The content type.</param>
        public static string ContentTypeToDisplayName(EnumTreeValue<ContentType> contentType)
        {
            var displayName = ConfigUtils.RemoveNamespace(contentType.FullName)
                .Split(ContentType.LayerNameSeparator)
                .Last()
                .Replace("_", " ")
                .Capitalize();
            return string.IsNullOrWhiteSpace(displayName) ? "[INVALID CONTENT NAME]" : displayName;
        }

        /// <summary>
        /// Converts the string representation of the content's type to a content ID.
        /// </summary>
        /// <param name="contentTypeName">The string representation of the content's type.</param>
        public static EnumTreeValue<ContentType>? ParseContentType(string? contentTypeName)
        {
            if (string.IsNullOrWhiteSpace(contentTypeName))
            {
                return null;
            }
            var properties = BaseContentTypeMap
                .Select(t => t)
                .ToList();
            properties.AddRange(contentTypeSubtypesMap.SelectMany(s => s.Value.Select(t => t)));
            var resultContent = properties.FirstOrDefault(contentAttribute => contentAttribute.Value.typeName == contentTypeName).Key;
            return resultContent == default ? null : resultContent;
        }

        /// <summary>
        /// Tries to convert the string representation of the content's type to a content ID, and returns the success.
        /// </summary>
        /// <param name="contentTypeName">The string representation of the content's type.</param>
        /// <param name="contentType">The resulting content, or a default content.</param>
        public static bool TryParseContentType(string? contentTypeName, out EnumTreeValue<ContentType> contentType)
        {
            var resultContent = ParseContentType(contentTypeName);
            contentType = resultContent ?? ContentType.Terrain.FIELD;
            return resultContent is not null;
        }
        #endregion
    }
}
