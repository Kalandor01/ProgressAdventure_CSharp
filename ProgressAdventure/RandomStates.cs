using NPrng.Generators;
using ProgressAdventure.Enums;

namespace ProgressAdventure
{
    /// <summary>
    /// Class for managing random number generators, used in save files.
    /// </summary>
    public static class RandomStates
    {
        #region Public properties
        /// <summary>
        /// The main random generator.
        /// </summary>
        public static SplittableRandom MainRandom { get; private set; }
        /// <summary>
        /// The world random generator.
        /// </summary>
        public static SplittableRandom WorldRandom { get; private set; }
        /// <summary>
        /// The misc random generator.
        /// </summary>
        public static SplittableRandom MiscRandom { get; private set; }
        /// <summary>
        /// The tile type noise generator seeds.
        /// </summary>
        public static Dictionary<TileNoiseType, ulong> TileTypeNoiseSeeds { get; private set; }
        /// <summary>
        /// The modifier used when creating a chunk random generator.
        /// </summary>
        public static Dictionary<TileNoiseType, PerlinNoise> TileTypeNoiseGenerators { get; private set; }
        /// <summary>
        /// The modifier used when creating a chunk random generator.
        /// </summary>
        public static double ChunkSeedModifier { get; private set; }
        #endregion

        #region "Constructors"
        /// <summary>
        /// Initialises the object's values.
        /// </summary>
        /// <param name="mainRandom"><inheritdoc cref="MainRandom" path="//summary"/></param>
        /// <param name="worldRandom"><inheritdoc cref="WorldRandom" path="//summary"/></param>
        /// <param name="miscRandom"><inheritdoc cref="MiscRandom" path="//summary"/></param>
        /// <param name="tileTypeNoiseSeeds"><inheritdoc cref="TileTypeNoiseSeeds" path="//summary"/></param>
        /// <param name="chunkSeedModifier"><inheritdoc cref="ChunkSeedModifier" path="//summary"/></param>
        public static void Initialise(
            SplittableRandom? mainRandom = null,
            SplittableRandom? worldRandom = null,
            SplittableRandom? miscRandom = null,
            Dictionary<TileNoiseType, ulong>? tileTypeNoiseSeeds = null,
            double? chunkSeedModifier = null
        )
        {
            var tempMainRandom = mainRandom ?? new SplittableRandom();
            var tempWorldRandom = worldRandom ?? Tools.MakeRandomGenerator(tempMainRandom);
            var tempMiscRandom = miscRandom ?? Tools.MakeRandomGenerator(tempMainRandom);
            UpdateSeedValues(
                tempMainRandom,
                tempWorldRandom,
                tempMiscRandom,
                tileTypeNoiseSeeds is not null ? RecalculateTileTypeNoiseSeeds(tileTypeNoiseSeeds, tempWorldRandom) : RecalculateTileTypeNoiseSeeds(tempWorldRandom),
                chunkSeedModifier ?? tempWorldRandom.GenerateDouble()
            );
        }
        
        /// <summary>
        /// Recalculates ALL seeds for perlin noise generators.
        /// </summary>
        /// <param name="parrentRandom">The random generator to use, to generate the noise seeds.</param>
        public static Dictionary<TileNoiseType, ulong> RecalculateTileTypeNoiseSeeds(SplittableRandom? parrentRandom = null)
        {
            parrentRandom ??= WorldRandom;
            return new Dictionary<TileNoiseType, ulong>
            {
                [TileNoiseType.HEIGHT] = (ulong)parrentRandom.Generate(),
                [TileNoiseType.TEMPERATURE] = (ulong)parrentRandom.Generate(),
                [TileNoiseType.HUMIDITY] = (ulong)parrentRandom.Generate(),
                [TileNoiseType.HOSTILITY] = (ulong)parrentRandom.Generate(),
                [TileNoiseType.POPULATION] = (ulong)parrentRandom.Generate(),
            };
        }

        /// <summary>
        /// Recalculates seeds for perlin noise generators that are missing from the partial tile type seed dictionary.
        /// </summary>
        /// <param name="parrentRandom">The random generator to use, to generate the missing noise seeds.</param>
        /// <param name="partialTileTypeNoiseDict">A dictionary that might not contain noise seeds for all tile types.</param>
        public static Dictionary<TileNoiseType, ulong> RecalculateTileTypeNoiseSeeds(Dictionary<TileNoiseType, ulong> partialTileTypeNoiseDict, SplittableRandom? parrentRandom = null)
        {
            parrentRandom ??= WorldRandom;
            foreach (TileNoiseType noiseType in Enum.GetValues(typeof(TileNoiseType)))
            {
                if (!partialTileTypeNoiseDict.ContainsKey(noiseType))
                {
                    partialTileTypeNoiseDict.Add(noiseType, (ulong)parrentRandom.Generate());
                }
            }
            return partialTileTypeNoiseDict;
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Updates the values for all seed, and tile noise generators.
        /// </summary>
        /// <param name="mainRandom"><inheritdoc cref="MainRandom" path="//summary"/></param>
        /// <param name="worldRandom"><inheritdoc cref="WorldRandom" path="//summary"/></param>
        /// <param name="tileTypeNoiseSeeds"><inheritdoc cref="TileTypeNoiseSeeds" path="//summary"/></param>
        private static void UpdateSeedValues(
            SplittableRandom mainRandom,
            SplittableRandom worldRandom,
            SplittableRandom miscRandom,
            Dictionary<TileNoiseType, ulong> tileTypeNoiseSeeds,
            double chunkSeedModifier
        )
        {
            MainRandom = mainRandom;
            WorldRandom = worldRandom;
            MiscRandom = miscRandom;
            TileTypeNoiseSeeds = tileTypeNoiseSeeds;
            ChunkSeedModifier = chunkSeedModifier;
            RecalculateNoiseGenerators();
        }

        /// <summary>
        /// Recalculates the perlin noise generators.
        /// </summary>
        private static void RecalculateNoiseGenerators()
        {
            TileTypeNoiseGenerators = new Dictionary<TileNoiseType, PerlinNoise>
            {
                [TileNoiseType.HEIGHT] = new PerlinNoise(TileTypeNoiseSeeds[TileNoiseType.HEIGHT]),
                [TileNoiseType.TEMPERATURE] = new PerlinNoise(TileTypeNoiseSeeds[TileNoiseType.TEMPERATURE]),
                [TileNoiseType.HUMIDITY] = new PerlinNoise(TileTypeNoiseSeeds[TileNoiseType.HUMIDITY]),
                [TileNoiseType.HOSTILITY] = new PerlinNoise(TileTypeNoiseSeeds[TileNoiseType.HOSTILITY]),
                [TileNoiseType.POPULATION] = new PerlinNoise(TileTypeNoiseSeeds[TileNoiseType.POPULATION])
            };
        }

        /// <summary>
        /// Deserialises the json representation of the tile type noise seeds, into a potentialy partial dictionary.
        /// </summary>
        /// <param name="tileTypeNoiseSeeds">The json representation of the tile type noise seeds.</param>
        private static Dictionary<TileNoiseType, ulong>? DeserialiseTileNoiseSeeds(IDictionary<string, object?>? tileTypeNoiseSeeds)
        {
            if (tileTypeNoiseSeeds is null)
            {
                Logger.Log("Tile noise seed parse error", "tile noise seed json is null", LogSeverity.WARN);
                return null;
            }

            var noiseSeedDict = new Dictionary<TileNoiseType, ulong>();
            foreach (var tileTypeNoiseSeed in tileTypeNoiseSeeds)
            {
                if (
                    tileTypeNoiseSeed.Value is not null &&
                    Enum.TryParse(tileTypeNoiseSeed.Key.ToString(), out TileNoiseType noiseTypeValue) &&
                    Enum.IsDefined(noiseTypeValue) &&
                    ulong.TryParse(tileTypeNoiseSeed.Value.ToString(), out ulong noiseSeed)
                )
                {
                    noiseSeedDict.Add(noiseTypeValue, noiseSeed);
                }
                else
                {
                    Logger.Log("Tile noise seed parse error", "tile noise seed value is incorrect", LogSeverity.WARN);
                }
            }
            return noiseSeedDict;
        }
        #endregion

        #region JsonConversion
        /// <summary>
        /// Returns a json representation of the <c>RandomState</c>.
        /// </summary>
        public static Dictionary<string, object?> ToJson()
        {
            return new Dictionary<string, object?>
            {
                ["main_random"] = Tools.SerializeRandom(MainRandom),
                ["world_random"] = Tools.SerializeRandom(WorldRandom),
                ["misc_random"] = Tools.SerializeRandom(MiscRandom),
                ["tile_type_noise_seeds"] = TileTypeNoiseSeeds,
                ["chunk_seed_modifier"] = ChunkSeedModifier,
            };
        }

        /// <summary>
        /// Converts the json representation of the object to object format.
        /// </summary>
        /// <param name="randomStatesJson">The json representation of the RandomState.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        public static bool FromJson(IDictionary<string, object?>? randomStatesJson, string fileVersion)
        {
            if (randomStatesJson is null)
            {
                Logger.Log("Random states parse error", "random states json is null", LogSeverity.WARN);
                Initialise();
                return false;
            }


            //correct data
            if (!Tools.IsUpToDate(Constants.SAVE_VERSION, fileVersion))
            {
                Logger.Log($"Random states json data is old", "correcting data");
                // 2.1.1 -> 2.2
                var newFileVersion = "2.2";
                if (!Tools.IsUpToDate(newFileVersion, fileVersion))
                {
                    // snake case rename
                    if (randomStatesJson.TryGetValue("mainRandom", out var mrRename))
                    {
                        randomStatesJson["main_random"] = mrRename;
                    }
                    if (randomStatesJson.TryGetValue("worldRandom", out var wrRename))
                    {
                        randomStatesJson["world_random"] = wrRename;
                    }
                    if (randomStatesJson.TryGetValue("miscRandom", out var mr2Rename))
                    {
                        randomStatesJson["misc_random"] = mr2Rename;
                    }
                    if (randomStatesJson.TryGetValue("tileTypeNoiseSeeds", out var ttnsRename))
                    {
                        randomStatesJson["tile_type_noise_seeds"] = ttnsRename;
                    }
                    if (randomStatesJson.TryGetValue("chunkSeedModifier", out var csmRename))
                    {
                        randomStatesJson["chunk_seed_modifier"] = csmRename;
                    }

                    Logger.Log("Corrected random states json data", $"{fileVersion} -> {newFileVersion}", LogSeverity.DEBUG);
                    fileVersion = newFileVersion;
                }
                Logger.Log($"Random states json data corrected");
            }


            var success = true;
            SplittableRandom? mainRandom = null;
            SplittableRandom? worldRandom = null;
            SplittableRandom? miscRandom = null;
            Dictionary<TileNoiseType, ulong>? tileTypeNoiseSeeds = null;
            double? chunkSeedModifier = null;

            // main random
            if (randomStatesJson.TryGetValue("main_random", out object? mainRandomValue))
            {
                success &= Tools.TryDeserializeRandom(mainRandomValue?.ToString(), out mainRandom);
            }
            else
            {
                Logger.Log("Random states parse error", "main random is null", LogSeverity.WARN);
                success = false;
            }
            // world random
            if (randomStatesJson.TryGetValue("world_random", out object? worldRandomValue))
            {
                success &= Tools.TryDeserializeRandom(worldRandomValue?.ToString(), out worldRandom);
            }
            else
            {
                Logger.Log("Random states parse error", "world random is null", LogSeverity.WARN);
                success = false;
            }
            // misc random
            if (randomStatesJson.TryGetValue("misc_random", out object? miscRandomValue))
            {
                success &= Tools.TryDeserializeRandom(miscRandomValue?.ToString(), out miscRandom);
            }
            else
            {
                Logger.Log("Random states parse error", "misc random is null", LogSeverity.WARN);
                success = false;
            }
            // tile type noise seeds
            if (randomStatesJson.TryGetValue("tile_type_noise_seeds", out object? tileTypeNoiseSeedsJson))
            {
                tileTypeNoiseSeeds = DeserialiseTileNoiseSeeds(tileTypeNoiseSeedsJson as IDictionary<string, object?>);
                success &= tileTypeNoiseSeeds is not null && tileTypeNoiseSeeds.Count == Enum.GetNames<TileNoiseType>().Length;
            }
            else
            {
                Logger.Log("Random states parse error", "misc random is null", LogSeverity.WARN);
                success = false;
            }
            // chunk seed modifier
            if (
                randomStatesJson.TryGetValue("chunk_seed_modifier", out object? chunkSeedModifierStrValue) &&
                double.TryParse(chunkSeedModifierStrValue?.ToString(), out double chunkSeedValue)
            )
            {
                chunkSeedModifier = chunkSeedValue;
            }
            else
            {
                Logger.Log("Random states parse error", "chunk seed modifier is null", LogSeverity.WARN);
                success = false;
            }

            Initialise(mainRandom, worldRandom, miscRandom, tileTypeNoiseSeeds, chunkSeedModifier);
            return success;
        }
        #endregion
    }
}
