using NPrng.Generators;
using PACommon;
using PACommon.JsonUtils;
using ProgressAdventure.Enums;

namespace ProgressAdventure
{
    /// <summary>
    /// Class for managing random number generators, used in save files.
    /// </summary>
    public interface IRandomStates<TSelf> : IJsonConvertable<TSelf>, IDisposable
        where TSelf : IJsonConvertable<TSelf>, IRandomStates<TSelf>
    {
        #region Public properties
        /// <summary>
        /// The main random generator.
        /// </summary>
        public SplittableRandom MainRandom { get; }
        /// <summary>
        /// The world random generator.
        /// </summary>
        public SplittableRandom WorldRandom { get; }
        /// <summary>
        /// The misc random generator.
        /// </summary>
        public SplittableRandom MiscRandom { get; }
        /// <summary>
        /// The tile type noise generator seeds.
        /// </summary>
        public Dictionary<TileNoiseType, ulong> TileTypeNoiseSeeds { get; }
        /// <summary>
        /// The modifier used when creating a chunk random generator.
        /// </summary>
        public Dictionary<TileNoiseType, PerlinNoise> TileTypeNoiseGenerators { get; }
        /// <summary>
        /// The modifier used when creating a chunk random generator.
        /// </summary>
        public double ChunkSeedModifier { get; }
        /// <summary>
        /// The seed string that can be used to get the same seed again.
        /// </summary>
        public string SeedString { get; }
        #endregion

        #region Public methods
        /// <summary>
        /// Recalculates ALL seeds for perlin noise generators.
        /// </summary>
        /// <param name="parrentRandom">The random generator to use, to generate the noise seeds.</param>
        public Dictionary<TileNoiseType, ulong> RecalculateTileTypeNoiseSeeds(SplittableRandom parrentRandom);

        /// <summary>
        /// Recalculates seeds for perlin noise generators that are missing from the partial tile type seed dictionary.
        /// </summary>
        /// <param name="partialTileTypeNoiseDict">A dictionary that might not contain noise seeds for all tile types.</param>
        /// <param name="parrentRandom">The random generator to use, to generate the missing noise seeds.</param>
        public Dictionary<TileNoiseType, ulong> RecalculateTileTypeNoiseSeeds(
            Dictionary<TileNoiseType, ulong> partialTileTypeNoiseDict,
            SplittableRandom parrentRandom
        );

        /// <summary>
        /// Recalculates the perlin noise generators.
        /// </summary>
        public void RecalculateNoiseGenerators();
        #endregion
    }
}
