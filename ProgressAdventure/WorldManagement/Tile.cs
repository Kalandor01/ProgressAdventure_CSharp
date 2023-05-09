using System.Data;

namespace ProgressAdventure.WorldManagement.Content
{
    /// <summary>
    /// Object, representing a tile in a chunk.
    /// </summary>
    public class Tile
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
        /// <param name="positionX">The x position of the tile.</param>
        /// <param name="positionY">The y position of the tile.</param>
        public Tile(long positionX, long positionY)
            : this(positionX, positionY, null) { }

        /// <summary>
        /// <inheritdoc cref="Tile"/><br/>
        /// Load an existing Tile object, from json.
        /// </summary>
        /// <param name="absolutePosition">The absolute position of the Tile.</param>
        /// <param name="visited"><inheritdoc cref="Visited" path="//summary"/></param>
        /// <param name="terrain"><inheritdoc cref="terrain" path="//summary"/></param>
        /// <param name="structure"><inheritdoc cref="structure" path="//summary"/></param>
        /// <param name="population"><inheritdoc cref="structure" path="//summary"/></param>
        public Tile((long x, long y) absolutePosition, int? visited, TerrainContent terrain, StructureContent structure, PopulationContent population)
            : this(absolutePosition.x, absolutePosition.y, visited, terrain, structure, population) { }
        #endregion

        #region Private constructors
        /// <summary>
        /// <inheritdoc cref="Tile"/><br/>
        /// The coordinates only need to be absolute, if a new tile is created.
        /// </summary>
        /// <param name="absoluteX">The absolute x coordinate of the Tile.</param>
        /// <param name="absoluteY">The absolute y coordinate of the Tile.</param>
        /// <param name="visited"><inheritdoc cref="Visited" path="//summary"/></param>
        /// <param name="terrain"><inheritdoc cref="terrain" path="//summary"/></param>
        /// <param name="structure"><inheritdoc cref="structure" path="//summary"/></param>
        /// <param name="population"><inheritdoc cref="population" path="//summary"/></param>
        private Tile(long absoluteX, long absoluteY, int? visited = null, TerrainContent? terrain = null, StructureContent? structure = null, PopulationContent? population = null)
        {
            relativePosition = (Utils.Mod(absoluteX, Constants.CHUNK_SIZE), Utils.Mod(absoluteY, Constants.CHUNK_SIZE));
            Visited = visited ?? 0;
            if (terrain is null || structure is null || population is null)
            {
                //var noiseValues = WorldUtils.GetNoiseValues(absoluteX, absoluteY);
                //terrain ??= CalculateClosestContent(noiseValues, terrainProperties);
                //if structure is None:
                //    # less structures on water
                //    reset_no_sdl = _no_structure_difference_limit
                //    if terrain.subtype == Terrain_types.OCEAN:
                //        _no_structure_difference_limit -= 0.1
                //    elif terrain.subtype == Terrain_types.SHORE:
                //        _no_structure_difference_limit -= 0.05
                //    gen_structure: Structure_content = _calculate_closest(noise_values, _structure_properties)
                //    structure = gen_structure
                //    _no_structure_difference_limit = reset_no_sdl
                //if population is None:
                //    # less population on not structures
                //    reset_no_pdl = _no_population_difference_limit
                //    if structure.subtype == Structure_types.NONE:
                //        _no_population_difference_limit -= 0.1
                //    gen_population: Population_content = _calculate_closest(noise_values, _population_properties)
                //    population = gen_population
                //    _no_population_difference_limit = reset_no_pdl
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

        /// <summary>
        /// Returns a json representation of the <c>Tile</c>.
        /// </summary>
        public Dictionary<string, object?> ToJson()
        {
            var terrainJson = terrain.ToJson();
            var structureJson = structure.ToJson();
            var populationJson = population.ToJson();


            var tileJson = new Dictionary<string, object?> {
                ["xPos"] = relativePosition.x,
                ["yPos"] =  relativePosition.y,
                ["visited"] = Visited,
                ["terrain"] = terrainJson,
                ["structure"] = structureJson,
                ["population"] = populationJson,
            };
            return tileJson;
        }
        #endregion
    }
}