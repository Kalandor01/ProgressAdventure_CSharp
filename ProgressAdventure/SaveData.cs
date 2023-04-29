using NPrng.Generators;
using ProgressAdventure.Entity;
using ProgressAdventure.Enums;

namespace ProgressAdventure
{
    public static class SaveData
    {
        #region Public fields
        /// <summary>
        /// The name of the save file.
        /// </summary>
        public static string saveName;
        /// <summary>
        /// The save name to display.
        /// </summary>
        public static string displaySaveName;
        /// <summary>
        /// The last time, the save file was saved.
        /// </summary>
        public static DateTime lastAccess;
        /// <summary>
        /// The player object.
        /// </summary>
        public static Player player;
        #endregion

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
        /// Initialises the object values.
        /// </summary>
        /// <param name="saveName"><inheritdoc cref="saveName" path="//summary"/></param>
        /// <param name="displaySaveName"><inheritdoc cref="displaySaveName" path="//summary"/></param>
        /// <param name="lastAccess"><inheritdoc cref="lastAccess" path="//summary"/></param>
        /// <param name="player"><inheritdoc cref="player" path="//summary"/></param>
        /// <param name="mainRandom"><inheritdoc cref="_mainRandom" path="//summary"/></param>
        /// <param name="worldRandom"><inheritdoc cref="_worldRandom" path="//summary"/></param>
        /// <param name="tileTypeNoiseSeeds"><inheritdoc cref="_tileTypeNoiseSeeds" path="//summary"/></param>
        public static void Initialise(
            string saveName,
            string? displaySaveName = null,
            DateTime? lastAccess = null,
            Player? player = null,
            SplittableRandom? mainRandom = null,
            SplittableRandom? worldRandom = null,
            Dictionary<TileNoiseType, ulong>? tileTypeNoiseSeeds = null
        )
        {
            SaveData.saveName = saveName;
            SaveData.displaySaveName = displaySaveName ?? saveName;
            SaveData.lastAccess = lastAccess ?? DateTime.Now;

            var tempMainRandom = mainRandom ?? new SplittableRandom();
            var tempWorldRandom = worldRandom ?? Tools.MakeRandomGenerator(tempMainRandom);
            UpdateSeedValues(
                tempMainRandom,
                tempWorldRandom,
                tileTypeNoiseSeeds is not null ? RecalculateTileTypeNoiseSeeds(tileTypeNoiseSeeds, tempWorldRandom) : RecalculateTileTypeNoiseSeeds(tempWorldRandom)
            );

            SaveData.player = player ?? new Player();
        }
        #endregion

        #region Public functions
        /// <summary>
        /// Converts the data for the display part of the data file to a json format.
        /// </summary>
        public static Dictionary<string, object?> DisplayDataToJson()
        {
            return new Dictionary<string, object?> {
                ["saveVersion"] = Constants.SAVE_VERSION,
                ["displayName"] = displaySaveName,
                ["lastAccess"] = DateTime.Now,
                ["playerName"] = player.name
            };
        }

        /// <summary>
        /// Converts the seeds data to json format.
        /// </summary>
        public static Dictionary<string, object?> SeedsToJson()
        {
            return new Dictionary<string, object?> {
                ["mainRandom"] = Tools.SerializeRandom(MainRandom),
                ["worldRandom"] = Tools.SerializeRandom(WorldRandom),
                ["tileTypeNoiseSeeds"] = TileTypeNoiseSeeds
            };
        }

        /// <summary>
        /// Converts the data for the main part of the data file to a json format.
        /// </summary>
        public static Dictionary<string, object?> MainDataToJson()
        {
            return new Dictionary<string, object?> {
                ["saveVersion"] = Constants.SAVE_VERSION,
                ["displayName"] = displaySaveName,
                ["lastAccess"] = DateTime.Now,
                ["player"] = player.ToJson(),
                ["seeds"] = SeedsToJson()
            };
        }
        #endregion

        #region Private functions
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

        /// <summary>
        /// Updates the values for all seed, and tile noise generators.
        /// </summary>
        /// <param name="mainRandom"><inheritdoc cref="_mainRandom" path="//summary"/></param>
        /// <param name="worldRandom"><inheritdoc cref="_worldRandom" path="//summary"/></param>
        /// <param name="tileTypeNoiseSeeds"><inheritdoc cref="_tileTypeNoiseSeeds" path="//summary"/></param>
        private static void UpdateSeedValues(
            SplittableRandom mainRandom,
            SplittableRandom worldRandom,
            Dictionary<TileNoiseType, ulong> tileTypeNoiseSeeds
        )
        {
            _mainRandom = mainRandom;
            _worldRandom = worldRandom;
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
        #endregion
    }
}
