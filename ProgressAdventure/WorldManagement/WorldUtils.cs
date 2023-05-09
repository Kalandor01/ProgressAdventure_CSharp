using ProgressAdventure.Enums;
using ProgressAdventure.WorldManagement.Content;
using System;
using System.Xml.Linq;

namespace ProgressAdventure.WorldManagement
{
    public static class WorldUtils
    {
        #region Constatnts
        public static readonly double noStructureDifferenceLimit = 0.3;
        public static readonly double noPopulationDifferenceLimit = 0.2;
        #endregion

        #region Config dictionaries

        #endregion

        #region Public functions
        /// <summary>
        /// Calculates the noise values for each perlin noise generator at a specific point, and normalises it between 0 and 1.
        /// </summary>
        /// <param name="absoluteX">The absolute x coordinate of the Tile.</param>
        /// <param name="absoluteY">The absolute y coordinate of the Tile.</param>
        //public static Dictionary<string, double> GetNoiseValues(long absoluteX, long absoluteY)
        //{
        //    var noiseValues = new Dictionary<TileNoiseType, double>();
        //    foreach (var noiseGenerator in SaveData.TileTypeNoiseGenerators)
        //    {
        //        noiseValues[noiseGenerator.Key] = 
        //        noise_values[name] = (noises[0]([(absolute_x + TILE_NOISE_RESOLUTION / 2) / TILE_NOISE_RESOLUTION,
        //                                    (absoulte_y + TILE_NOISE_RESOLUTION / 2) / TILE_NOISE_RESOLUTION])
        //                                    )
        //        noise_values[name] += (noises[1]([(absolute_x + TILE_NOISE_RESOLUTION / 2) / TILE_NOISE_RESOLUTION,
        //                                    (absoulte_y + TILE_NOISE_RESOLUTION / 2) / TILE_NOISE_RESOLUTION])
        //                                    * 0.5)
        //        noise_values[name] += (noises[2]([(absolute_x + TILE_NOISE_RESOLUTION / 2) / TILE_NOISE_RESOLUTION,
        //                                    (absoulte_y + TILE_NOISE_RESOLUTION / 2) / TILE_NOISE_RESOLUTION])
        //        * 0.25)
        //        noise_values[name] = (noise_values[name] + 0.875 + _tile_type_noise_offsets[name]) / 1.75
        //    }
        //    for name, noises in noise_generators.items():


        //    return noiseValues;
        //}


        //def _calculate_closest(noise_values:dict[str, float], content_properties:dict[type[Base_content], dict[str, float]]) :
        //    """Calculates the best tile type for the space depending on the perlin noise values."""
        //    min_diff_content = list(content_properties.keys())[0]
        //        min_diff = 1000000
        //    for content_type, properties in content_properties.items():
        //        sum_diff = 0
        //        property_num = 0
        //        for name in properties:
        //            try:
        //                sum_diff += abs(properties[name] - noise_values[name])
        //                property_num += 1
        //            except KeyError:
        //                pass
        //        prop_dif = sum_diff / property_num
        //        if prop_dif<min_diff:
        //            min_diff = prop_dif
        //            min_diff_content = content_type
        //    // no content if difference is too big
        //    if content_properties == _structure_properties and min_diff >= _no_structure_difference_limit:
        //        min_diff_content = No_structure
        //    elif content_properties == _population_properties and min_diff >= _no_population_difference_limit:
        //        min_diff_content = No_population
        //    return min_diff_content()


        //def _get_content(content_type:str, type_map:dict[str, type[Base_content]]) -> type[Base_content]:
        //    """Get the content class from the content type and type map."""
        //    # get content type index
        //    try: return type_map[content_type]
        //    except KeyError:
        //        logger("Unknown content type", f'type: "{content_type}"', Log_type.ERROR)
        //        return list(type_map.values())[0]


        //        def _load_content(content_json:dict[str, Any]|None, type_map:dict[str, type[Base_content]]):
        //    """Load a content object from the content json."""
        //    // get content type
        //    if content_json is not None:
        //        try: content_type = content_json["subtype"]
        //        except KeyError: content_type = list(type_map.keys())[0]
        //    else:
        //        content_type = list(type_map.keys())[0]
        //        // get content
        //        content_class = _get_content(content_type, type_map)
        //    return content_class(content_json)


        //def _gen_content_name(content:Base_content):
        //    content.name = content.subtype.value + " " + str(int(random.random()* 100000))
        #endregion
    }
}
