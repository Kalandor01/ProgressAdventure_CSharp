using NPrng.Generators;
using ProgressAdventure.Enums;

namespace ProgressAdventure
{
    /// <summary>
    /// Class for managing random number generators, used in save files.
    /// </summary>
    public static class RandomStates
    {
        #region Private fields
        /// <summary>
        /// The main random generator.
        /// </summary>
        private static SplittableRandom _mainRandom;
        /// <summary>
        /// The world random generator.
        /// </summary>
        private static SplittableRandom _worldRandom;
        /// <summary>
        /// The misc random generator.
        /// </summary>
        private static SplittableRandom _miscRandom;
        /// <summary>
        /// The tile type noise generator seeds.
        /// </summary>
        private static Dictionary<TileNoiseType, ulong> _tileTypeNoiseSeeds;
        /// <summary>
        /// The tile type noise generators.
        /// </summary>
        private static Dictionary<TileNoiseType, PerlinNoise> _tileTypeNoiseGenerators;
        #endregion

        #region Public properties
        /// <summary>
        /// <inheritdoc cref="_mainRandom" path="//summary"/>
        /// </summary>
        public static SplittableRandom MainRandom { get => _mainRandom; }
        /// <summary>
        /// <inheritdoc cref="_worldRandom" path="//summary"/>
        /// </summary>
        public static SplittableRandom WorldRandom { get => _worldRandom; }
        /// <summary>
        /// <inheritdoc cref="_miscRandom" path="//summary"/>
        /// </summary>
        public static SplittableRandom MiscRandom { get => _miscRandom; }
        /// <summary>
        /// <inheritdoc cref="_tileTypeNoiseSeeds" path="//summary"/>
        /// </summary>
        public static Dictionary<TileNoiseType, ulong> TileTypeNoiseSeeds { get => _tileTypeNoiseSeeds; }
        /// <summary>
        /// <inheritdoc cref="_tileTypeNoiseGenerators" path="//summary"/>
        /// </summary>
        public static Dictionary<TileNoiseType, PerlinNoise> TileTypeNoiseGenerators { get => _tileTypeNoiseGenerators; }
        #endregion

        #region "Constructors"
        /// <summary>
        /// Initialises the object's values.
        /// </summary>
        /// <param name="mainRandom"><inheritdoc cref="_mainRandom" path="//summary"/></param>
        /// <param name="worldRandom"><inheritdoc cref="_worldRandom" path="//summary"/></param>
        /// <param name="miscRandom"><inheritdoc cref="_miscRandom" path="//summary"/></param>
        /// <param name="tileTypeNoiseSeeds"><inheritdoc cref="_tileTypeNoiseSeeds" path="//summary"/></param>
        public static void Initialise(
            SplittableRandom? mainRandom = null,
            SplittableRandom? worldRandom = null,
            SplittableRandom? miscRandom = null,
            Dictionary<TileNoiseType, ulong>? tileTypeNoiseSeeds = null
        )
        {
            var tempMainRandom = mainRandom ?? new SplittableRandom();
            var tempWorldRandom = worldRandom ?? Tools.MakeRandomGenerator(tempMainRandom);
            var tempMiscRandom = miscRandom ?? Tools.MakeRandomGenerator(tempMainRandom);
            UpdateSeedValues(
                tempMainRandom,
                tempWorldRandom,
                tempMiscRandom,
                tileTypeNoiseSeeds is not null ? RecalculateTileTypeNoiseSeeds(tileTypeNoiseSeeds, tempWorldRandom) : RecalculateTileTypeNoiseSeeds(tempWorldRandom)
            );
        }

        /// <summary>
        /// Returns a json representation of the <c>RandomState</c>.
        /// </summary>
        public static Dictionary<string, object?> ToJson()
        {
            return new Dictionary<string, object?>
            {
                ["mainRandom"] = Tools.SerializeRandom(MainRandom),
                ["worldRandom"] = Tools.SerializeRandom(WorldRandom),
                ["miscRandom"] = Tools.SerializeRandom(MiscRandom),
                ["tileTypeNoiseSeeds"] = TileTypeNoiseSeeds
            };
        }

        public static void FromJson(IDictionary<string, object?>? randomStatesJson)
        {
            SplittableRandom? mainRandom = null;
            SplittableRandom? worldRandom = null;
            SplittableRandom? miscRandom = null;
            Dictionary<TileNoiseType, ulong>? tileTypeNoiseSeeds = null;
            if (randomStatesJson is not null)
            {
                mainRandom = Tools.DeserializeRandom(randomStatesJson["mainRandom"]?.ToString());
                worldRandom = Tools.DeserializeRandom(randomStatesJson["worldRandom"]?.ToString());
                miscRandom = Tools.DeserializeRandom(randomStatesJson["miscRandom"]?.ToString());
                var tileTypeNoiseSeedsJson = randomStatesJson["tileTypeNoiseSeeds"];
                tileTypeNoiseSeeds = DeserialiseTileNoiseSeeds((IDictionary<string, object?>?)tileTypeNoiseSeedsJson);
            }
            else
            {
                Logger.Log("Random states parse error", "random states json is null", LogSeverity.WARN);
            }
            Initialise(mainRandom, worldRandom, miscRandom, tileTypeNoiseSeeds);
        }
        
        /// <summary>
        /// Recalculates ALL seeds for perlin noise generators.
        /// </summary>
        /// <param name="parrentRandom">The random generator to use, to generate the noise seeds.</param>
        public static Dictionary<TileNoiseType, ulong> RecalculateTileTypeNoiseSeeds(SplittableRandom? parrentRandom = null)
        {
            parrentRandom ??= _worldRandom;
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
            parrentRandom ??= _worldRandom;
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
        /// <param name="mainRandom"><inheritdoc cref="_mainRandom" path="//summary"/></param>
        /// <param name="worldRandom"><inheritdoc cref="_worldRandom" path="//summary"/></param>
        /// <param name="tileTypeNoiseSeeds"><inheritdoc cref="_tileTypeNoiseSeeds" path="//summary"/></param>
        private static void UpdateSeedValues(
            SplittableRandom mainRandom,
            SplittableRandom worldRandom,
            SplittableRandom miscRandom,
            Dictionary<TileNoiseType, ulong> tileTypeNoiseSeeds
        )
        {
            _mainRandom = mainRandom;
            _worldRandom = worldRandom;
            _miscRandom = miscRandom;
            _tileTypeNoiseSeeds = tileTypeNoiseSeeds;
            RecalculateNoiseGenerators();
        }

        /// <summary>
        /// Recalculates the perlin noise generators.
        /// </summary>
        private static void RecalculateNoiseGenerators()
        {
            _tileTypeNoiseGenerators = new Dictionary<TileNoiseType, PerlinNoise>
            {
                [TileNoiseType.HEIGHT] = new PerlinNoise(_tileTypeNoiseSeeds[TileNoiseType.HEIGHT]),
                [TileNoiseType.TEMPERATURE] = new PerlinNoise(_tileTypeNoiseSeeds[TileNoiseType.TEMPERATURE]),
                [TileNoiseType.HUMIDITY] = new PerlinNoise(_tileTypeNoiseSeeds[TileNoiseType.HUMIDITY]),
                [TileNoiseType.HOSTILITY] = new PerlinNoise(_tileTypeNoiseSeeds[TileNoiseType.HOSTILITY]),
                [TileNoiseType.POPULATION] = new PerlinNoise(_tileTypeNoiseSeeds[TileNoiseType.POPULATION])
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
                    Enum.TryParse(typeof(TileNoiseType), tileTypeNoiseSeed.Key.ToString(), out object? noiseTypeValue) &&
                    noiseTypeValue is not null &&
                    Enum.IsDefined(typeof(TileNoiseType), noiseTypeValue) &&
                    uint.TryParse(tileTypeNoiseSeed.Value.ToString(), out uint noiseSeed)
                )
                {
                    noiseSeedDict.Add((TileNoiseType)noiseTypeValue, noiseSeed);
                }
            }
            return noiseSeedDict;
        }
        #endregion
    }
}
