using NPrng.Generators;
using ProgressAdventure.Enums;
using ProgressAdventure.WorldManagement.Content;
using ProgressAdventure.WorldManagement.Content.Population;
using ProgressAdventure.WorldManagement.Content.Structure;
using ProgressAdventure.WorldManagement.Content.Terrain;

namespace ProgressAdventure.WorldManagement
{
    public static class WorldUtils
    {
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

        #region Config dictionaries
        /// <summary>
        /// Offsets for tile noise values
        /// </summary>
        private static readonly Dictionary<TileNoiseType, double> _tileNoiseOffsets = new()
        {
            [TileNoiseType.HEIGHT] = 0,
            [TileNoiseType.TEMPERATURE] = 0,
            [TileNoiseType.HUMIDITY] = 0,
            [TileNoiseType.HOSTILITY] = -0.1,
            [TileNoiseType.POPULATION] = -0.1,
        };

        /// <summary>
        /// Dictionary to map terrain types to their ideal properties.
        /// </summary>
        private static readonly Dictionary<Type, Dictionary<TileNoiseType, double>> _terrainContentTypePropertyMap = new()
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
        /// Dictionary to map terrain types to their ideal properties.
        /// </summary>
        private static readonly Dictionary<Type, Dictionary<TileNoiseType, double>> _structureContentTypePropertyMap = new()
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
        /// Dictionary to map terrain types to their ideal properties.
        /// </summary>
        private static readonly Dictionary<Type, Dictionary<TileNoiseType, double>> _populationContentTypePropertyMap = new()
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
        /// Dictionary to map content types to their property maps.
        /// </summary>
        private static readonly Dictionary<Type, Dictionary<Type, Dictionary<TileNoiseType, double>>> _contentTypePropertyMap = new()
        {
            [typeof(TerrainContent)] = _terrainContentTypePropertyMap,
            [typeof(StructureContent)] = _structureContentTypePropertyMap,
            [typeof(PopulationContent)] = _populationContentTypePropertyMap,
        };

        /// <summary>
        /// Dictionary to map terrain content types to their object type.
        /// </summary>
        private static readonly Dictionary<ContentTypeID, Type> _terrainContentTypeMap = new()
        {
            [ContentType.Terrain.FIELD] = typeof(FieldTerrain),
            [ContentType.Terrain.MOUNTAIN] = typeof(MountainTerrain),
            [ContentType.Terrain.OCEAN] = typeof(OceanTerrain),
            [ContentType.Terrain.SHORE] = typeof(ShoreTerrain),
        };

        /// <summary>
        /// Dictionary to map structure content types to their object type.
        /// </summary>
        private static readonly Dictionary<ContentTypeID, Type> _structureContentTypeMap = new()
        {
            [ContentType.Structure.NONE] = typeof(NoStructure),
            [ContentType.Structure.BANDIT_CAMP] = typeof(BanditCampStructure),
            [ContentType.Structure.VILLAGE] = typeof(VillageStructure),
            [ContentType.Structure.KINGDOM] = typeof(KingdomStructure),
        };

        /// <summary>
        /// Dictionary to map population content types to their object type.
        /// </summary>
        private static readonly Dictionary<ContentTypeID, Type> _populationContentTypeMap = new()
        {
            [ContentType.Population.NONE] = typeof(NoPopulation),
            [ContentType.Population.HUMAN] = typeof(HumanPopulation),
            [ContentType.Population.ELF] = typeof(ElfPopulation),
            [ContentType.Population.DWARF] = typeof(DwarfPopulation),
            [ContentType.Population.DEMON] = typeof(DemonPopulation),
        };

        /// <summary>
        /// Dictionary to map content types to their object type maps.
        /// </summary>
        internal static readonly Dictionary<Type, Dictionary<ContentTypeID, Type>> contentTypeMap = new()
        {
            [typeof(TerrainContent)] = _terrainContentTypeMap,
            [typeof(StructureContent)] = _structureContentTypeMap,
            [typeof(PopulationContent)] = _populationContentTypeMap,
        };

        /// <summary>
        /// Dictionary to map terrain content types to their text representation.
        /// </summary>
        internal static readonly Dictionary<ContentTypeID, string> terrainContentTypeIDTextMap = new()
        {
            [ContentType.Terrain.FIELD] = "field",
            [ContentType.Terrain.MOUNTAIN] = "mountain",
            [ContentType.Terrain.OCEAN] = "ocean",
            [ContentType.Terrain.SHORE] = "shore",
        };

        /// <summary>
        /// Dictionary to map structure content types to their text representation.
        /// </summary>
        internal static readonly Dictionary<ContentTypeID, string> structureContentSubtypeIDTextMap = new()
        {
            [ContentType.Structure.NONE] = "none",
            [ContentType.Structure.BANDIT_CAMP] = "bandit_camp",
            [ContentType.Structure.VILLAGE] = "village",
            [ContentType.Structure.KINGDOM] = "kingdom",
        };

        /// <summary>
        /// Dictionary to map population content types to their text representation.
        /// </summary>
        internal static readonly Dictionary<ContentTypeID, string> populationContentSubtypeIDTextMap = new()
        {
            [ContentType.Population.NONE] = "none",
            [ContentType.Population.HUMAN] = "human",
            [ContentType.Population.ELF] = "elf",
            [ContentType.Population.DWARF] = "dwarf",
            [ContentType.Population.DEMON] = "demon",
        };

        /// <summary>
        /// Dictionary to map content types to their text representation.
        /// </summary>
        internal static readonly Dictionary<ContentTypeID, string> contentTypeIDTextMap = new()
        {
            [ContentType.TerrainContentType] = "terrain",
            [ContentType.StructureContentType] = "content",
            [ContentType.PopulationContentType] = "population",
        };

        /// <summary>
        /// Dictionary to map content types to their content subtype text maps.
        /// </summary>
        internal static readonly Dictionary<ContentTypeID, Dictionary<ContentTypeID, string>> contentTypeIDSubtypeTextMap = new()
        {
            [ContentType.TerrainContentType] = terrainContentTypeIDTextMap,
            [ContentType.StructureContentType] = structureContentSubtypeIDTextMap,
            [ContentType.PopulationContentType] = populationContentSubtypeIDTextMap,
        };
        #endregion

        #region Public functions
        /// <summary>
        /// Calculates the noise values for each perlin noise generator at a specific point, and normalises it between 0 and 1.
        /// </summary>
        /// <param name="absoluteX">The absolute x coordinate of the Tile.</param>
        /// <param name="absoluteY">The absolute y coordinate of the Tile.</param>
        public static Dictionary<TileNoiseType, double> GetNoiseValues(long absoluteX, long absoluteY)
        {
            var noiseValues = new Dictionary<TileNoiseType, double>();
            foreach (var noiseGeneratorEntry in RandomStates.TileTypeNoiseGenerators)
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
                noiseValues[key] += _tileNoiseOffsets[key];
            }
        }

        /// <summary>
        /// Calculates the best tile type for the space depending on the perlin noise values.
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
            noStructureDLOverride ??= noStructureDifferenceLimit;
            noPopulationDLOverride ??= noPopulationDifferenceLimit;
            var contentProperties = _contentTypePropertyMap[typeof(T)];
            Type minDiffContentType = contentProperties.Keys.First();
            var minDiff = 1000000.0;
            foreach (var propertyEntry in contentProperties)
            {
                var properties = propertyEntry.Value;
                var sumDiff = 0.0;
                var propertyNum = 0;
                foreach (var propertyKey in properties.Keys)
                {
                    if (noiseValues.ContainsKey(propertyKey))
                    {
                        sumDiff += Math.Abs(properties[propertyKey] - noiseValues[propertyKey]);
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
            if (contentProperties == _structureContentTypePropertyMap && minDiff >= noStructureDLOverride)
            {
                minDiffContentType = typeof(NoStructure);
            }
            else if (contentProperties == _populationContentTypePropertyMap && minDiff >= noPopulationDLOverride)
            {
                minDiffContentType = typeof(NoPopulation);
            }
            var contentObj = Activator.CreateInstance(minDiffContentType, new object?[] {chunkRandom, null, null}) ?? throw new ArgumentNullException(message: "Couldn't create content object from type!", null);
            return (T)contentObj;
        }

        /// <summary>
        /// Returs the content type, if the content type ID is an ID for an content type.
        /// </summary>
        /// <param name="contentTypeID">The int representation of the content's ID.</param>
        public static ContentTypeID? ToContentType(int contentTypeID)
        {
            var newContentType = (ContentTypeID)contentTypeID;
            var contentTypes = Tools.GetNestedStaticClassFields<ContentTypeID>(typeof(ContentType));
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
        /// Returs the content type, if the string is the ttring representation of a content subtype.
        /// </summary>
        /// <param name="parrentContentType">The parrent content type ID.</param>
        /// <param name="contentSubtypeString">The string representation of the subtype content.</param>
        public static ContentTypeID? ToContentType(ContentTypeID parrentContentType, string? contentSubtypeString)
        {
            if (
                contentSubtypeString is not null &&
                contentTypeIDSubtypeTextMap.TryGetValue(parrentContentType, out Dictionary<ContentTypeID, string>? subtypeTextMap) &&
                subtypeTextMap.ContainsValue(contentSubtypeString)
            )
            {
                return subtypeTextMap.First(subT => subT.Value == contentSubtypeString).Key;
            }
            return null;
        }

        /// <summary>
        /// Tries to convert the int representation of the content ID to a content ID, and returns the success.
        /// </summary>
        /// <param name="contentTypeID">The int representation of the content's ID.</param>
        /// <param name="contentType">The resulting content, or a default content.</param>
        public static bool TryParseContentType(int contentTypeID, out ContentTypeID contentType)
        {
            var resultContent = ToContentType(contentTypeID);
            contentType = resultContent ?? ContentType.Terrain.FIELD;
            return resultContent is not null;
        }

        /// <summary>
        /// Tries to convert the string representation of the subtype content to a content ID, and returns the success.
        /// </summary>
        /// <param name="parrentContentType">The parrent content type ID.</param>
        /// <param name="contentSubtypeString">The string representation of the subtype content.</param>
        /// <param name="contentType">The resulting content, or a default content.</param>
        public static bool TryParseContentType(ContentTypeID parrentContentType, string? contentSubtypeString, out ContentTypeID contentType)
        {
            var resultContent = ToContentType(parrentContentType, contentSubtypeString);
            contentType = resultContent ?? ContentType.Terrain.FIELD;
            return resultContent is not null;
        }
        #endregion
    }
}
