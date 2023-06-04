using NPrng.Generators;
using ProgressAdventure.WorldManagement.Content;

namespace ProgressAdventure.WorldManagement
{
    /// <summary>
    /// Object, representing a tile in a chunk.
    /// </summary>
    public class Tile : IJsonReadable
    {
        #region Public fields
        /// <summary>
        /// The relative position of the tile.
        /// </summary>
        public readonly (long x, long y) relativePosition;
        /// <summary>
        /// The terrain layer of the tile.
        /// </summary>
        public readonly TerrainContent terrain;
        /// <summary>
        /// The structure layer of the tile.
        /// </summary>
        public readonly StructureContent structure;
        /// <summary>
        /// The population on this tile.
        /// </summary>
        public readonly PopulationContent population;
        #endregion

        #region Public properties
        /// <summary>
        /// How many times the tile has been visited.
        /// </summary>
        public int Visited { get; private set; }
        #endregion

        #region Public constructors
        /// <summary>
        /// <inheritdoc cref="Tile"/><br/>
        /// Creates a new Tile object.
        /// </summary>
        /// <param name="absoluteX">The absolute x position of the tile.</param>
        /// <param name="absoluteY">The absolute y position of the tile.</param>
        /// <param name="chunkRandom">The parrent chunk's random generator.</param>
        public Tile(long absoluteX, long absoluteY, SplittableRandom? chunkRandom)
            : this(absoluteX, absoluteY, chunkRandom, null) { }

        /// <summary>
        /// <inheritdoc cref="Tile"/><br/>
        /// Load an existing Tile object, from json.
        /// </summary>
        /// <param name="chunkRandom">The parrent chunk's random generator.</param>
        /// <param name="position">The position of the Tile.</param>
        /// <param name="visited"><inheritdoc cref="Visited" path="//summary"/></param>
        /// <param name="terrain"><inheritdoc cref="terrain" path="//summary"/></param>
        /// <param name="structure"><inheritdoc cref="structure" path="//summary"/></param>
        /// <param name="population"><inheritdoc cref="structure" path="//summary"/></param>
        public Tile(SplittableRandom chunkRandom, (long x, long y) position, int? visited, TerrainContent terrain, StructureContent structure, PopulationContent population)
            : this(position.x, position.y, chunkRandom, visited, terrain, structure, population) { }
        #endregion

        #region Private constructors
        /// <summary>
        /// <inheritdoc cref="Tile"/><br/>
        /// The coordinates only need to be absolute, if a new tile is created.
        /// </summary>
        /// <param name="absoluteX">The absolute x coordinate of the Tile.</param>
        /// <param name="absoluteY">The absolute y coordinate of the Tile.</param>
        /// <param name="chunkRandom">The parrent chunk's random generator.</param>
        /// <param name="visited"><inheritdoc cref="Visited" path="//summary"/></param>
        /// <param name="terrain"><inheritdoc cref="terrain" path="//summary"/></param>
        /// <param name="structure"><inheritdoc cref="structure" path="//summary"/></param>
        /// <param name="population"><inheritdoc cref="population" path="//summary"/></param>
        private Tile(long absoluteX, long absoluteY, SplittableRandom? chunkRandom = null, int? visited = null, TerrainContent? terrain = null, StructureContent? structure = null, PopulationContent? population = null)
        {
            relativePosition = (Utils.Mod(absoluteX, Constants.CHUNK_SIZE), Utils.Mod(absoluteY, Constants.CHUNK_SIZE));
            Visited = visited ?? 0;
            if (terrain is null || structure is null || population is null)
            {
                chunkRandom ??= Chunk.GetChunkRandom((absoluteX, absoluteY));
                var noiseValues = WorldUtils.GetNoiseValues(absoluteX, absoluteY);
                terrain ??= WorldUtils.CalculateClosestContent<TerrainContent>(chunkRandom, noiseValues);
                if (structure is null)
                {
                    // less structures on water
                    var noStructureDL = WorldUtils.noStructureDifferenceLimit;
                    if (terrain.subtype == ContentType.Terrain.OCEAN)
                    {
                        noStructureDL -= 0.1;
                    }
                    else if (terrain.subtype == ContentType.Terrain.SHORE)
                    {
                        noStructureDL -= 0.05;
                    }
                    structure = WorldUtils.CalculateClosestContent<StructureContent>(chunkRandom, noiseValues, noStructureDL);
                }
                if (population is null)
                {
                    // less population on not structures
                    var noPopulationDL = WorldUtils.noPopulationDifferenceLimit;
                    if (structure.subtype == ContentType.Structure.NONE)
                    {
                        noPopulationDL -= 0.1;
                    }
                    population = WorldUtils.CalculateClosestContent<PopulationContent>(chunkRandom, noiseValues, noPopulationDL);
                }
            }
            this.terrain = terrain;
            this.structure = structure;
            this.population = population;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Should be called, if a player is on this tile.
        /// </summary>
        public void Visit()
        {
            Visited++;
            terrain.Visit(this);
            structure.Visit(this);
            population.Visit(this);
        }
        #endregion

        #region JsonConvert
        public Dictionary<string, object?> ToJson()
        {
            var terrainJson = terrain.ToJson();
            var structureJson = structure.ToJson();
            var populationJson = population.ToJson();


            var tileJson = new Dictionary<string, object?>
            {
                ["xPos"] = relativePosition.x,
                ["yPos"] = relativePosition.y,
                ["visited"] = Visited,
                ["terrain"] = terrainJson,
                ["structure"] = structureJson,
                ["population"] = populationJson,
            };
            return tileJson;
        }

        /// <summary>
        /// Converts the json representation of the tile into a tile object.
        /// </summary>
        /// <param name="chunkRandom">The parrent chunk's random generator.</param>
        /// <param name="tileJson">The json representation of the tile.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        public static Tile? FromJson(SplittableRandom chunkRandom, IDictionary<string, object?>? tileJson, string fileVersion)
        {
            if (tileJson is null)
            {
                Logger.Log("Tile parse error", "tile json is null", Enums.LogSeverity.ERROR);
                return null;
            }

            // x and y
            if (!(
                tileJson.TryGetValue("xPos", out object? xPosValue) &&
                long.TryParse(xPosValue?.ToString(), out long xPos) &&
                tileJson.TryGetValue("yPos", out object? yPosValue) &&
                long.TryParse(yPosValue?.ToString(), out long yPos)
            ))
            {
                Logger.Log("Tile parse error", "tile coordinates cannot be parsed", Enums.LogSeverity.ERROR);
                return null;
            }
            // visited
            int? visited = null;
            if (
                tileJson.TryGetValue("visited", out object? visitedValueStr) &&
                int.TryParse(visitedValueStr?.ToString(), out int visitedValue)
            )
            {
                visited = visitedValue;
            }
            else
            {
                Logger.Log("Tile decode error", "couldn't decode visited from json. Recreacting...", Enums.LogSeverity.WARN);
            }
            // terrain
            TerrainContent? terrain = null;
            if (
                tileJson.TryGetValue("terrain", out object? terrainJson)
            )
            {
                terrain = TerrainContent.FromJson(chunkRandom, (IDictionary<string, object?>?)terrainJson, fileVersion);
            }
            else
            {
                Logger.Log("Tile decode error", "couldn't decode terrain from json. Recreacting...", Enums.LogSeverity.WARN);
            }
            // structure
            StructureContent? structure = null;
            if (
                tileJson.TryGetValue("structure", out object? structureJson)
            )
            {
                structure = StructureContent.FromJson(chunkRandom, (IDictionary<string, object?>?)structureJson, fileVersion);
            }
            else
            {
                Logger.Log("Tile decode error", "couldn't decode structure from json. Recreacting...", Enums.LogSeverity.WARN);
            }
            // population
            PopulationContent? population = null;
            if (
                tileJson.TryGetValue("population", out object? populationJson)
            )
            {
                population = PopulationContent.FromJson(chunkRandom, (IDictionary<string, object?>?)populationJson, fileVersion);
            }
            else
            {
                Logger.Log("Tile decode error", "couldn't decode population from json. Recreacting...", Enums.LogSeverity.WARN);
            }
            return new Tile(xPos, yPos, chunkRandom, visited, terrain, structure, population);
        }
        #endregion
    }
}