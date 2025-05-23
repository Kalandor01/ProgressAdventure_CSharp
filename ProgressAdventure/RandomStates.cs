﻿using NPrng.Generators;
using PACommon;
using PACommon.Enums;
using PACommon.JsonUtils;
using ProgressAdventure.Enums;
using System.Diagnostics.CodeAnalysis;
using PACTools = PACommon.Tools;
using ProgressAdventure.Extensions;

namespace ProgressAdventure
{
    /// <summary>
    /// Class for managing random number generators, used in save files.
    /// </summary>
    public class RandomStates : IJsonConvertable<RandomStates>
    {
        #region Public properties
        /// <summary>
        /// The main random generator.
        /// </summary>
        public SplittableRandom MainRandom { get; private set; }
        /// <summary>
        /// The world random generator.
        /// </summary>
        public SplittableRandom WorldRandom { get; private set; }
        /// <summary>
        /// The misc random generator.
        /// </summary>
        public SplittableRandom MiscRandom { get; private set; }
        /// <summary>
        /// The tile type noise generator seeds.
        /// </summary>
        public Dictionary<TileNoiseType, ulong> TileTypeNoiseSeeds { get; private set; }
        /// <summary>
        /// The modifier used when creating a chunk random generator.
        /// </summary>
        public Dictionary<TileNoiseType, PerlinNoise> TileTypeNoiseGenerators { get; private set; }
        /// <summary>
        /// The modifier used when creating a chunk random generator.
        /// </summary>
        public double ChunkSeedModifier { get; private set; }
        /// <summary>
        /// The seed string that can be used to get the same seed again.
        /// </summary>
        public string SeedString { get; private set; }
        #endregion

        #region Private fields
        /// <summary>
        /// Object used for locking the thread while the singleton gets created.
        /// </summary>
        private static readonly object _threadLock = new();
        /// <summary>
        /// The singleton istance.
        /// </summary>
        private static RandomStates? _instance = null;
        #endregion

        #region Public properties
        /// <summary>
        /// <inheritdoc cref="_instance" path="//summary"/>
        /// </summary>
        public static RandomStates Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_threadLock)
                    {
                        _instance ??= Initialize();
                    }
                }
                return _instance;
            }
        }
        #endregion

        #region Private constructors
        /// <summary>
        /// <inheritdoc cref="RandomStates" path="//summary"/>
        /// </summary>
        /// <param name="mainRandom"><inheritdoc cref="MainRandom" path="//summary"/></param>
        /// <param name="worldRandom"><inheritdoc cref="WorldRandom" path="//summary"/></param>
        /// <param name="miscRandom"><inheritdoc cref="MiscRandom" path="//summary"/></param>
        /// <param name="tileTypeNoiseSeeds"><inheritdoc cref="TileTypeNoiseSeeds" path="//summary"/></param>
        /// <param name="chunkSeedModifier"><inheritdoc cref="ChunkSeedModifier" path="//summary"/></param>
        /// <param name="seedString"><inheritdoc cref="SeedString" path="//summary"/></param>
        private RandomStates(
            SplittableRandom? mainRandom = null,
            SplittableRandom? worldRandom = null,
            SplittableRandom? miscRandom = null,
            Dictionary<TileNoiseType, ulong>? tileTypeNoiseSeeds = null,
            double? chunkSeedModifier = null,
            string? seedString = null
        )
        {
            MainRandom = mainRandom ?? new SplittableRandom();
            SeedString = seedString ?? MainRandom.ToSeedString();
            WorldRandom = worldRandom ?? PACTools.MakeRandomGenerator(MainRandom);
            MiscRandom = miscRandom ?? PACTools.MakeRandomGenerator(MainRandom);
            TileTypeNoiseSeeds = tileTypeNoiseSeeds is not null ? RecalculateTileTypeNoiseSeeds(tileTypeNoiseSeeds, WorldRandom) : RecalculateTileTypeNoiseSeeds(WorldRandom);
            ChunkSeedModifier = chunkSeedModifier ?? WorldRandom.GenerateDouble();

            RecalculateNoiseGenerators();
        }
        #endregion

        #region "Initializer"
        /// <summary>
        /// Initializes the object's values.
        /// </summary>
        /// <param name="mainRandom"><inheritdoc cref="MainRandom" path="//summary"/></param>
        /// <param name="worldRandom"><inheritdoc cref="WorldRandom" path="//summary"/></param>
        /// <param name="miscRandom"><inheritdoc cref="MiscRandom" path="//summary"/></param>
        /// <param name="tileTypeNoiseSeeds"><inheritdoc cref="TileTypeNoiseSeeds" path="//summary"/></param>
        /// <param name="chunkSeedModifier"><inheritdoc cref="ChunkSeedModifier" path="//summary"/></param>
        /// <param name="seedString"><inheritdoc cref="SeedString" path="//summary"/></param>
        public static RandomStates Initialize(
            SplittableRandom? mainRandom = null,
            SplittableRandom? worldRandom = null,
            SplittableRandom? miscRandom = null,
            Dictionary<TileNoiseType, ulong>? tileTypeNoiseSeeds = null,
            double? chunkSeedModifier = null,
            string? seedString = null
        )
        {
            _instance = new RandomStates(mainRandom, worldRandom, miscRandom, tileTypeNoiseSeeds, chunkSeedModifier, seedString);
            PACSingletons.Instance.Logger.Log($"{nameof(RandomStates)} initialized");
            return _instance;
        }

        /// <summary>
        /// Recalculates ALL seeds for perlin noise generators.
        /// </summary>
        /// <param name="parrentRandom">The random generator to use, to generate the noise seeds.</param>
        public static Dictionary<TileNoiseType, ulong> RecalculateTileTypeNoiseSeeds(SplittableRandom parrentRandom)
        {
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
        /// <param name="partialTileTypeNoiseDict">A dictionary that might not contain noise seeds for all tile types.</param>
        /// <param name="parrentRandom">The random generator to use, to generate the missing noise seeds.</param>
        public static Dictionary<TileNoiseType, ulong> RecalculateTileTypeNoiseSeeds(Dictionary<TileNoiseType, ulong> partialTileTypeNoiseDict, SplittableRandom parrentRandom)
        {
            foreach (TileNoiseType noiseType in Enum.GetValues(typeof(TileNoiseType)))
            {
                if (!partialTileTypeNoiseDict.ContainsKey(noiseType))
                {
                    partialTileTypeNoiseDict.Add(noiseType, (ulong)parrentRandom.Generate());
                }
            }
            return partialTileTypeNoiseDict;
        }

        /// <summary>
        /// Recalculates the perlin noise generators.
        /// </summary>
        public void RecalculateNoiseGenerators()
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
        #endregion

        #region JsonConversion
        static List<(Action<JsonDictionary> objectJsonCorrecter, string newFileVersion)> IJsonConvertable<RandomStates>.VersionCorrecters { get; } =
        [
            // 2.1.1 -> 2.2
            (oldJson => {
                // snake case rename
                JsonDataCorrecterUtils.RemapKeysIfExist(oldJson, new Dictionary<string, string>
                {
                    ["mainRandom"] = "main_random",
                    ["worldRandom"] = "world_random",
                    ["miscRandom"] = "misc_random",
                    ["tileTypeNoiseSeeds"] = "tile_type_noise_seeds",
                    ["chunkSeedModifier"] = "chunk_seed_modifier",
                });
            }, "2.2"),
            // 2.4 -> 2.4.1
            (oldJson =>
            {
                // empty seed string
                oldJson["seed"] = "";
            }, "2.4.1"),
        ];

        public JsonDictionary ToJson()
        {
            return new JsonDictionary
            {
                [Constants.JsonKeys.RandomStates.MAIN_RANDOM] = PACTools.SerializeRandom(MainRandom),
                [Constants.JsonKeys.RandomStates.WORLD_RANDOM] = PACTools.SerializeRandom(WorldRandom),
                [Constants.JsonKeys.RandomStates.MISC_RANDOM] = PACTools.SerializeRandom(MiscRandom),
                [Constants.JsonKeys.RandomStates.TILE_TYPE_NOISE_SEEDS] = TileTypeNoiseSeeds.ToDictionary(
                    k => k.Key.ToString(),
                    v => (JsonObject?)v.Value
                ),
                [Constants.JsonKeys.RandomStates.CHUNK_SEED_MODIFIER] = ChunkSeedModifier,
                [Constants.JsonKeys.RandomStates.SEED_STRING] = SeedString,
            };
        }

        /// <summary>
        /// Deserialises the json representation of the tile type noise seeds, into a potentialy partial dictionary.
        /// </summary>
        /// <param name="tileTypeNoiseSeeds">The json representation of the tile type noise seeds.</param>
        private static Dictionary<TileNoiseType, ulong>? DeserialiseTileNoiseSeeds(JsonDictionary? tileTypeNoiseSeeds)
        {
            if (tileTypeNoiseSeeds is null)
            {
                PACSingletons.Instance.Logger.Log("Tile noise seed parse error", "tile noise seed json is null", LogSeverity.WARN);
                return null;
            }

            var noiseSeedDict = new Dictionary<TileNoiseType, ulong>();
            foreach (var tileTypeNoiseSeed in tileTypeNoiseSeeds)
            {
                if (
                    tileTypeNoiseSeed.Value is not null &&
                    Enum.TryParse(tileTypeNoiseSeed.Key.ToString(), out TileNoiseType noiseTypeValue) &&
                    Enum.IsDefined(noiseTypeValue) &&
                    ulong.TryParse(tileTypeNoiseSeed.Value.Value.ToString(), out var noiseSeed)
                )
                {
                    noiseSeedDict.Add(noiseTypeValue, noiseSeed);
                }
                else
                {
                    PACSingletons.Instance.Logger.Log("Tile noise seed parse error", "tile noise seed value is incorrect", LogSeverity.WARN);
                }
            }
            return noiseSeedDict;
        }

        static bool IJsonConvertable<RandomStates>.FromJsonWithoutCorrection(JsonDictionary randomStatesJson, string fileVersion, [NotNullWhen(true)] ref RandomStates? randomStates)
        {
            Dictionary<TileNoiseType, ulong>? tileTypeNoiseSeeds = null;

            var success = true;
            success &= PACTools.TryParseJsonValue<SplittableRandom?>(randomStatesJson, Constants.JsonKeys.RandomStates.MAIN_RANDOM, out var mainRandom);
            success &= PACTools.TryParseJsonValue<SplittableRandom?>(randomStatesJson, Constants.JsonKeys.RandomStates.WORLD_RANDOM, out var worldRandom);
            success &= PACTools.TryParseJsonValue<SplittableRandom?>(randomStatesJson, Constants.JsonKeys.RandomStates.MISC_RANDOM, out var miscRandom);

            if (
                PACTools.TryCastJsonAnyValue<JsonDictionary>(
                    randomStatesJson,
                    Constants.JsonKeys.RandomStates.TILE_TYPE_NOISE_SEEDS,
                    out var tileTypeNoiseSeedsJson,
                    isStraigthCast: true
                )
            )
            {
                tileTypeNoiseSeeds = DeserialiseTileNoiseSeeds(tileTypeNoiseSeedsJson);
                success &= tileTypeNoiseSeeds is not null && tileTypeNoiseSeeds.Count == Enum.GetNames<TileNoiseType>().Length;
            }
            else
            {
                success = false;
            }

            success &= PACTools.TryParseJsonValue<double?>(randomStatesJson, Constants.JsonKeys.RandomStates.CHUNK_SEED_MODIFIER, out var chunkSeedModifier);
            success &= PACTools.TryParseJsonValue<string?>(randomStatesJson, Constants.JsonKeys.RandomStates.SEED_STRING, out var seedString);

            randomStates = Initialize(mainRandom, worldRandom, miscRandom, tileTypeNoiseSeeds, chunkSeedModifier, seedString);
            return success;
        }
        #endregion
    }
}
