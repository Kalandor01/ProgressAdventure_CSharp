using NPrng.Generators;
using PACommon;
using PACommon.Enums;
using ProgressAdventure.ConfigManagement;
using ProgressAdventure.Enums;
using ProgressAdventure.WorldManagement.Content;
using ProgressAdventure.WorldManagement.Content.Population;
using ProgressAdventure.WorldManagement.Content.Structure;
using ProgressAdventure.WorldManagement.Content.Terrain;
using System.Diagnostics.CodeAnalysis;
using System.Text;
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

        #region Default config dicts
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
        private static readonly Dictionary<ContentTypeID, ContentTypeIDPropertiesDTO> _baseContentTypeMap = new()
        {
            [ContentType.TERRAIN] = new ContentTypeIDPropertiesDTO(ContentType.TERRAIN, typeof(TerrainContent)),
            [ContentType.STRUCTURE] = new ContentTypeIDPropertiesDTO(ContentType.STRUCTURE, typeof(StructureContent)),
            [ContentType.POPULATION] = new ContentTypeIDPropertiesDTO(ContentType.POPULATION, typeof(PopulationContent)),
        };
        
        /// <summary>
        /// The default value for the config used for the value of <see cref="TerrainContentTypeMap"/>.
        /// </summary>
        private static readonly Dictionary<ContentTypeID, ContentTypeIDPropertiesDTO> _defaultTerrainContentTypeMap = new()
        {
            [ContentType.Terrain.FIELD] = new ContentTypeIDPropertiesDTO(ContentType.Terrain.FIELD, typeof(FieldTerrain)),
            [ContentType.Terrain.MOUNTAIN] = new ContentTypeIDPropertiesDTO(ContentType.Terrain.MOUNTAIN, typeof(MountainTerrain)),
            [ContentType.Terrain.OCEAN] = new ContentTypeIDPropertiesDTO(ContentType.Terrain.OCEAN, typeof(OceanTerrain)),
            [ContentType.Terrain.SHORE] = new ContentTypeIDPropertiesDTO(ContentType.Terrain.SHORE, typeof(ShoreTerrain)),
        };

        /// <summary>
        /// The default value for the config used for the value of <see cref="StructureContentTypeMap"/>.
        /// </summary>
        private static readonly Dictionary<ContentTypeID, ContentTypeIDPropertiesDTO> _defaultStructureContentTypeMap = new()
        {
            [ContentType.Structure.NONE] = new ContentTypeIDPropertiesDTO(ContentType.Structure.NONE, typeof(NoStructure)),
            [ContentType.Structure.BANDIT_CAMP] = new ContentTypeIDPropertiesDTO(ContentType.Structure.BANDIT_CAMP, typeof(BanditCampStructure)),
            [ContentType.Structure.VILLAGE] = new ContentTypeIDPropertiesDTO(ContentType.Structure.VILLAGE, typeof(VillageStructure)),
            [ContentType.Structure.KINGDOM] = new ContentTypeIDPropertiesDTO(ContentType.Structure.KINGDOM, typeof(KingdomStructure)),
        };

        /// <summary>
        /// The default value for the config used for the value of <see cref="PopulationContentTypeMap"/>.
        /// </summary>
        private static readonly Dictionary<ContentTypeID, ContentTypeIDPropertiesDTO> _defaultPopulationContentTypeMap = new()
        {
            [ContentType.Population.NONE] = new ContentTypeIDPropertiesDTO(ContentType.Population.NONE, typeof(NoPopulation)),
            [ContentType.Population.HUMAN] = new ContentTypeIDPropertiesDTO(ContentType.Population.HUMAN, typeof(HumanPopulation)),
            [ContentType.Population.ELF] = new ContentTypeIDPropertiesDTO(ContentType.Population.ELF, typeof(ElfPopulation)),
            [ContentType.Population.DWARF] = new ContentTypeIDPropertiesDTO(ContentType.Population.DWARF, typeof(DwarfPopulation)),
            [ContentType.Population.DEMON] = new ContentTypeIDPropertiesDTO(ContentType.Population.DEMON, typeof(DemonPopulation)),
        };
        #endregion

        #region Config dictionaries
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
        internal static Dictionary<ContentTypeID, ContentTypeIDPropertiesDTO> BaseContentTypeMap { get; set; }

        /// <summary>
        /// Dictionary to map content types to their content subtype property maps.
        /// </summary>
        internal static readonly Dictionary<ContentTypeID, Dictionary<ContentTypeID, ContentTypeIDPropertiesDTO>> contentTypeSubtypesMap = new()
        {
            [ContentType.TERRAIN] = null,
            [ContentType.STRUCTURE] = null,
            [ContentType.POPULATION] = null,
        };

        /// <summary>
        /// Dictionary to map terrain content types to their type properties.
        /// </summary>
        internal static Dictionary<ContentTypeID, ContentTypeIDPropertiesDTO> TerrainContentTypeMap { get; set; }

        /// <summary>
        /// Dictionary to map structure content types to their type properties.
        /// </summary>
        internal static Dictionary<ContentTypeID, ContentTypeIDPropertiesDTO> StructureContentTypeMap { get; set; }

        /// <summary>
        /// Dictionary to map population content types to their type properties.
        /// </summary>
        internal static Dictionary<ContentTypeID, ContentTypeIDPropertiesDTO> PopulationContentTypeMap { get; set; }
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
            contentTypeSubtypesMap[ContentType.TERRAIN] = TerrainContentTypeMap;
            contentTypeSubtypesMap[ContentType.STRUCTURE] = StructureContentTypeMap;
            contentTypeSubtypesMap[ContentType.POPULATION] = PopulationContentTypeMap;

            contentTypePropertyMap[typeof(TerrainContent)] = TerrainContentTypePropertyMap;
            contentTypePropertyMap[typeof(StructureContent)] = StructureContentTypePropertyMap;
            contentTypePropertyMap[typeof(PopulationContent)] = PopulationContentTypePropertyMap;
        }

        /// <summary>
        /// Resets all variables that come from configs.
        /// </summary>
        public static void LoadDefaultConfigs()
        {
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
                Path.Join(Constants.PA_CONFIGS_NAMESPACE, Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "tile_noise_offsets"),
                null,
                _defaultTileNoiseOffsets
            );

            PACSingletons.Instance.ConfigManager.SetConfig(
                Path.Join(Constants.PA_CONFIGS_NAMESPACE, Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "terrain_content_type_property_map"),
                null,
                _defaultTerrainContentTypePropertyMap,
                key => key.FullName ?? ""
            );

            PACSingletons.Instance.ConfigManager.SetConfig(
                Path.Join(Constants.PA_CONFIGS_NAMESPACE, Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "structure_content_type_property_map"),
                null,
                _defaultStructureContentTypePropertyMap,
                key => key.FullName ?? ""
            );

            PACSingletons.Instance.ConfigManager.SetConfig(
                Path.Join(Constants.PA_CONFIGS_NAMESPACE, Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "population_content_type_property_map"),
                null,
                _defaultPopulationContentTypePropertyMap,
                key => key.FullName ?? ""
            );

            PACSingletons.Instance.ConfigManager.SetConfig(
                Path.Join(Constants.PA_CONFIGS_NAMESPACE, Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "base_content_type_map"),
                null,
                _baseContentTypeMap,
                key => key.ToString()!
            );

            PACSingletons.Instance.ConfigManager.SetConfig(
                Path.Join(Constants.PA_CONFIGS_NAMESPACE, Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "terrain_content_type_map"),
                null,
                _defaultTerrainContentTypeMap,
                key => key.ToString()!
            );

            PACSingletons.Instance.ConfigManager.SetConfig(
                Path.Join(Constants.PA_CONFIGS_NAMESPACE, Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "structure_content_type_map"),
                null,
                _defaultStructureContentTypeMap,
                key => key.ToString()!
            );

            PACSingletons.Instance.ConfigManager.SetConfig(
                Path.Join(Constants.PA_CONFIGS_NAMESPACE, Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "population_content_type_map"),
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
        public static void ReloadConfigs(List<string> namespaceFolders, bool isVanillaInvalid, int? showProgressIndentation = null)
        {
            Tools.ReloadConfigsFolderDisplayProgress(Constants.CONFIGS_WORLD_SUBFOLDER_NAME, showProgressIndentation);
            showProgressIndentation = showProgressIndentation + 1 ?? null;

            TileNoiseOffsets = ConfigUtils.ReloadConfigsAggregateDict(
                Path.Join(Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "tile_noise_offsets"),
                namespaceFolders,
                _defaultTileNoiseOffsets,
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
                key => key.ToString()!,
                key => ParseContentTypeFromRealName(key) ?? throw new JsonException($"Unknown content type real name: \"{key}\""),
                isVanillaInvalid,
                showProgressIndentation
            );

            TerrainContentTypeMap = ConfigUtils.ReloadConfigsAggregateDict(
                Path.Join(Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "terrain_content_type_map"),
                namespaceFolders,
                _defaultTerrainContentTypeMap,
                key => key.ToString()!,
                key => ParseContentTypeFromRealName(key) ?? throw new JsonException($"Unknown content type real name: \"{key}\""),
                isVanillaInvalid,
                showProgressIndentation
            );

            StructureContentTypeMap = ConfigUtils.ReloadConfigsAggregateDict(
                Path.Join(Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "structure_content_type_map"),
                namespaceFolders,
                _defaultStructureContentTypeMap,
                key => key.ToString()!,
                key => ParseContentTypeFromRealName(key) ?? throw new JsonException($"Unknown content type real name: \"{key}\""),
                isVanillaInvalid,
                showProgressIndentation
            );

            PopulationContentTypeMap = ConfigUtils.ReloadConfigsAggregateDict(
                Path.Join(Constants.CONFIGS_WORLD_SUBFOLDER_NAME, "population_content_type_map"),
                namespaceFolders,
                _defaultPopulationContentTypeMap,
                key => key.ToString()!,
                key => ParseContentTypeFromRealName(key) ?? throw new JsonException($"Unknown content type real name: \"{key}\""),
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
        /// Returns all content type IDs.
        /// </summary>
        public static List<ContentTypeID> GetAllContentTypes()
        {
            return Utils.GetNestedStaticClassFields<ContentTypeID>(typeof(ContentType));
        }

        /// <summary>
        /// Returs the content type, if the content type ID is an ID for an content type.
        /// </summary>
        /// <param name="contentTypeID">The uint representation of the content's ID.</param>
        public static ContentTypeID? ToContentType(uint contentTypeID)
        {
            var newContentType = (ContentTypeID)(int)contentTypeID;
            var contentTypes = GetAllContentTypes();
            foreach (var contentType in contentTypes)
            {
                if (newContentType == contentType)
                {
                    return contentType;
                }
            }
            return null;
        }

        /// <summary>
        /// Returs the content properties, if the string is the ttring representation of a content subtype.
        /// </summary>
        /// <param name="parrentContentType">The parrent content type ID.</param>
        /// <param name="contentSubtypeString">The string representation of the subtype content.</param>
        public static ContentTypeIDPropertiesDTO? ToContentTypeProperties(ContentTypeID parrentContentType, string? contentSubtypeString)
        {
            if (
                contentSubtypeString is not null &&
                contentTypeSubtypesMap.TryGetValue(parrentContentType, out Dictionary<ContentTypeID, ContentTypeIDPropertiesDTO>? subtypePropertesMap) &&
                subtypePropertesMap.FirstOrDefault(subtypeMap => subtypeMap.Value.typeName == contentSubtypeString)
                    is KeyValuePair<ContentTypeID, ContentTypeIDPropertiesDTO> contentProperties &&
                contentProperties.Value is not null
            )
            {
                return contentProperties.Value;
            }
            return null;
        }

        /// <summary>
        /// Tries to convert the uint representation of the content ID to a content ID, and returns the success.
        /// </summary>
        /// <param name="contentTypeID">The int representation of the content's ID.</param>
        /// <param name="contentType">The resulting content, or a default content.</param>
        public static bool TryParseContentType(uint contentTypeID, out ContentTypeID contentType)
        {
            var resultContent = ToContentType(contentTypeID);
            contentType = resultContent ?? ContentType.Terrain.FIELD;
            return resultContent is not null;
        }

        /// <summary>
        /// Tries to convert the string representation of the subtype content to content properties, and returns the success.
        /// </summary>
        /// <param name="parrentContentType">The parrent content type ID.</param>
        /// <param name="contentSubtypeString">The string representation of the subtype content.</param>
        /// <param name="contentProperties">The resulting content properties.</param>
        public static bool TryParseContentType(
            ContentTypeID parrentContentType,
            string? contentSubtypeString,
            [NotNullWhen(true)] out ContentTypeIDPropertiesDTO? contentProperties
        )
        {
            contentProperties = ToContentTypeProperties(parrentContentType, contentSubtypeString);
            return contentProperties is not null;
        }

        /// <summary>
        /// Returs the content type, if the content name is a name for a content type.
        /// </summary>
        /// <param name="contentTypeRealName">The real name of the content.</param>
        public static ContentTypeID? ParseContentTypeFromRealName(string? contentTypeRealName)
        {
            if (string.IsNullOrWhiteSpace(contentTypeRealName))
            {
                return null;
            }
            var resultContent = GetAllContentTypes().FirstOrDefault(content => content.ToString() == contentTypeRealName);
            return resultContent == default ? null : resultContent;
        }

        /// <summary>
        /// Converts the content type ID, to it's default type name.
        /// </summary>
        /// <param name="contentTypeID">The content type ID.</param>
        public static string ContentIDToTypeName(ContentTypeID contentTypeID)
        {
            var modifiedPath = new StringBuilder();
            var name = contentTypeID.ToString();
            if (name is null || !TryParseContentType(contentTypeID.mID, out _))
            {
                PACSingletons.Instance.Logger.Log("Unknown content type", $"ID: {contentTypeID.mID}", LogSeverity.ERROR);
                return "[UNKNOWN CONTENT TYPE]";
            }

            var actualNamePath = name.Split(nameof(ContentType) + ".").Last();
            var pathParts = actualNamePath.Split('.');
            for (var x = 0; x < pathParts.Length - 1; x++)
            {
                var pathPart = pathParts[x];
                var modifiedPathPart = new StringBuilder();
                for (var y = 0; y < pathPart.Length; y++)
                {
                    if (y != 0 && char.IsUpper(pathPart[y]))
                    {
                        modifiedPathPart.Append('_');
                    }
                    modifiedPathPart.Append(pathPart[y]);
                }
                modifiedPath.Append(modifiedPathPart + "/");
            }

            modifiedPath.Append(pathParts.Last());
            var modifiedPathStr = modifiedPath.ToString().ToLower();
            return string.IsNullOrWhiteSpace(modifiedPathStr) ? "[UNKNOWN CONTENT TYPE]" : modifiedPathStr;
        }

        /// <summary>
        /// Converts the string representation of the content's type to a content ID.
        /// </summary>
        /// <param name="contentTypeName">The string representation of the content's type.</param>
        public static ContentTypeID? ParseContentType(string? contentTypeName)
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
        public static bool TryParseContentType(string? contentTypeName, out ContentTypeID contentType)
        {
            var resultContent = ParseContentType(contentTypeName);
            contentType = resultContent ?? ContentType.Terrain.FIELD;
            return resultContent is not null;
        }
        #endregion
    }
}
