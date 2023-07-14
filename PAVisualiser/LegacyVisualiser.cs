namespace PAVisualiser
{
    public class LegacyVisualiser
    {
        //class Content_colors(Enum) :
        //    ERROR =         (255, 0, 255, 255)
        //    EMPTY =         (0, 0, 0, 0)
        //    RED =           (255, 0, 0, 255)
        //    GREEN =         (0, 255, 0, 255)
        //    BLUE =          (0, 0, 255, 255)
        //    BROWN =         (61, 42, 27, 255)
        //    SKIN =          (212, 154, 99, 255)
        //    LIGHT_BLUE =    (60, 60, 255, 255)
        //    LIGHT_GRAY =    (75, 75, 75, 255)
        //    LIGHT_BROWN =   (82, 56, 36, 255)
        //    LIGHTER_BLUE =  (99, 99, 255, 255)
        //    DARK_GREEN =    (28, 87, 25, 255)


        //class Terrain_colors(Enum) :
        //    EMPTY = Content_colors.EMPTY.value
        //    FIELD = Content_colors.DARK_GREEN.value
        //    MOUNTAIN = Content_colors.LIGHT_GRAY.value
        //    OCEAN = Content_colors.LIGHT_BLUE.value
        //    SHORE = Content_colors.LIGHTER_BLUE.value


        //class Structure_colors(Enum) :
        //    EMPTY = Content_colors.EMPTY.value
        //    BANDIT_CAMP = Content_colors.RED.value
        //    VILLAGE = Content_colors.LIGHT_BROWN.value
        //    KINGDOM = Content_colors.BROWN.value


        //class Population_colors(Enum) :
        //    EMPTY = Content_colors.EMPTY.value
        //    HUMAN = Content_colors.SKIN.value
        //    ELF = Content_colors.DARK_GREEN.value
        //    DWARF = Content_colors.BROWN.value
        //    DEMON = Content_colors.RED.value


        //terrain_type_colors = {
        //    Terrain_types.NONE: Terrain_colors.EMPTY,
        //            Terrain_types.FIELD: Terrain_colors.FIELD,
        //            Terrain_types.MOUNTAIN: Terrain_colors.MOUNTAIN,
        //            Terrain_types.OCEAN: Terrain_colors.OCEAN,
        //            Terrain_types.SHORE: Terrain_colors.SHORE
        //}

        //structure_type_colors = {
        //            Structure_types.NONE: Structure_colors.EMPTY,
        //            Structure_types.BANDIT_CAMP: Structure_colors.BANDIT_CAMP,
        //            Structure_types.VILLAGE: Structure_colors.VILLAGE,
        //            Structure_types.KINGDOM: Structure_colors.KINGDOM
        //}

        //population_type_colors = {
        //            Population_types.NONE: Population_colors.EMPTY,
        //            Population_types.HUMAN: Population_colors.HUMAN,
        //            Population_types.ELF: Population_colors.ELF,
        //            Population_types.DWARF: Population_colors.DWARF,
        //            Population_types.DEMON: Population_colors.DEMON
        //}


        //def get_tile_color(tile:Tile, tile_type_counts:dict[str, int], type_colors_map = "terrain", opacity_multi = None) -> tuple[dict[str, int], tuple[int, int, int, int]]:
        //    if type_colors_map == "structure":
        //        tcm = structure_type_colors
        //        subtype = tile.structure.subtype
        //    elif type_colors_map == "population":
        //        tcm = population_type_colors
        //        subtype = tile.population.subtype
        //    else:
        //        tcm = terrain_type_colors
        //        subtype = tile.terrain.subtype


        //    try: tile_type_counts[subtype.value] += 1
        //    except KeyError: tile_type_counts[subtype.value] = 1
        //    try: t_color: tuple[int, int, int, int] = tcm[subtype].value
        //    except KeyError: t_color = Content_colors.ERROR.value
        //    if opacity_multi is None:
        //        return (tile_type_counts, t_color)
        //    else:
        //        return (tile_type_counts, (t_color[0], t_color[1], t_color[2], int(t_color[3] * opacity_multi)))


        //def draw_world_tiles(type_colors= "terrain", image_path= "world.png"):
        //    """
        //    Genarates an image, representing the different types of tiles, and their placements in the world.\n
        //    Also returns the tile count for all tile types.\n
        //    `type_colors` sets witch map to export (terrain, structure, population).
        //    """
        //    tile_size = (1, 1)


        //    tile_type_counts = { "TOTAL": 0}

        //    corners = World._get_corners()
        //    size = ((corners[2] - corners[0] + 1) * tile_size[0], (corners[3] - corners[1] + 1) * tile_size[1])

        //    im = Image.new("RGBA", size, Content_colors.EMPTY.value)
        //    draw = ImageDraw.Draw(im, "RGBA")
        //    for chunk in World.chunks.values():
        //        for tile in chunk.tiles.values():
        //            x = (chunk.base_x - corners[0]) + tile.x
        //            y = (chunk.base_y - corners[1]) + tile.y
        //            start_x = int(x * tile_size[0])
        //            start_y = int(y * tile_size[1])
        //            end_x = int(x * tile_size[0] + tile_size[0] - 1)
        //            end_y = int(y * tile_size[1] + tile_size[1] - 1)
        //            # find type
        //            tile_type_counts["TOTAL"] += 1
        //            data = get_tile_color(tile, tile_type_counts, type_colors)
        //            tile_type_counts = data[0]
        //            draw.rectangle((start_x, start_y, end_x, end_y), data[1])
        //    im.save(image_path)
        //    # reorder tile_type_counts
        //    total = tile_type_counts.pop("TOTAL")
        //    tile_type_counts["TOTAL"] = total
        //    return tile_type_counts



        //def draw_combined_img(image_path= "combined.png"):
        //    """
        //    Genarates an image, representing the different types of tiles with the layers overlayed, and their placements in the world.\n
        //    Also returns the tile count for all tile types.
        //    """
        //    tile_size = (1, 1)


        //    tile_type_counts = { "TOTAL": 0}

        //    corners = World._get_corners()
        //    size = ((corners[2] - corners[0] + 1) * tile_size[0], (corners[3] - corners[1] + 1) * tile_size[1])


        //    def make_transparrent_img(ttc:dict[str, int], type_colors = "terrain", opacity = 1 / 3):
        //        im = Image.new("RGBA", size, Content_colors.EMPTY.value)
        //        draw = ImageDraw.Draw(im, "RGBA")
        //        for chunk in World.chunks.values():
        //            for tile in chunk.tiles.values():
        //                x = (chunk.base_x - corners[0]) + tile.x
        //                y = (chunk.base_y - corners[1]) + tile.y
        //                start_x = int(x * tile_size[0])
        //                start_y = int(y * tile_size[1])
        //                end_x = int(x * tile_size[0] + tile_size[0] - 1)
        //                end_y = int(y * tile_size[1] + tile_size[1] - 1)
        //                # find type
        //                ttc["TOTAL"] += 1
        //                data = get_tile_color(tile, ttc, type_colors, opacity)
        //                ttc = data[0]
        //                draw.rectangle((start_x, start_y, end_x, end_y), data[1])
        //        return im



        //    terrain_img = make_transparrent_img(tile_type_counts, "terrain", 1)
        //    structure_img = make_transparrent_img(tile_type_counts, "structure", 1 / 2)
        //    population_img = make_transparrent_img(tile_type_counts, "population")
        //    terrain_img.paste(structure_img, (0, 0), structure_img)
        //    terrain_img.paste(population_img, (0, 0), population_img)

        //    terrain_img.save(image_path)
        //    # reorder tile_type_counts
        //    total = tile_type_counts.pop("TOTAL")
        //    tile_type_counts["TOTAL"] = total
        //    return tile_type_counts


        //def save_visualizer(save_name:str):
        //    """
        //    Visualises the data in a save file
        //    """
        //    EXPORT_FOLDER = "visualised_saves"
        //    EXPORT_DATA_FILE = "data.txt"
        //    EXPORT_TERRAIN_FILE = "terrain.png"
        //    EXPORT_STRUCTURE_FILE = "structure.png"
        //    EXPORT_POPULATOIN_FILE = "population.png"
        //    EXPORT_COMBINED_FILE = "combined.png"


        //    ts.threading.current_thread().name = VISUALIZER_THREAD_NAME



        //    def make_img(type_colors:str, export_file: str):
        //        """`type_colors`: terrain, structure or population"""
        //        print("Generating image...", end = "", flush = True)
        //        tile_type_counts = draw_world_tiles(type_colors, join(visualized_save_path, export_file))
        //        print("DONE!")
        //        text = f"\nTile types:\n"
        //        for tt, count in tile_type_counts.items():
        //            text += f"\t{tt}: {count}\n"
        //        print(text)


        //    def make_combined_img(export_file: str):
        //        print("Generating image...", end = "", flush = True)
        //        tile_type_counts = draw_combined_img(join(visualized_save_path, export_file))
        //        print("DONE!")
        //        text = f"\nTile types:\n"
        //        for tt, count in tile_type_counts.items():
        //            text += f"\t{tt}: {count}\n"
        //        print(text)



        //    try:
        //        save_folder_path = join(SAVES_FOLDER_PATH, save_name)
        //        now = datetime.now()
        //        visualized_save_name = f"{save_name}_{make_date(now)}_{make_time(now, ';')}"
        //        display_visualized_save_path = join(EXPORT_FOLDER, visualized_save_name)
        //        visualized_save_path = join(ROOT_FOLDER, display_visualized_save_path)


        //        data = ts.decode_save_s(join(save_folder_path, SAVE_FILE_NAME_DATA, ), 1)

        //        # check save version
        //        try: save_version = str(data["save_version"])
        //        except KeyError: save_version = "0.0"
        //        load_continue = True
        //        if save_version != SAVE_VERSION:
        //            is_older = ts.is_up_to_date(save_version, SAVE_VERSION)
        //            ans = sfm.UI_list(["No", "Yes"], f"\"{save_name}\" is {('an older version' if is_older else 'a newer version')} than what it should be! ({save_version}->{SAVE_VERSION}) Do you want to continue?").display()
        //            if ans == 0:
        //                load_continue = False
        //        #load
        //        if load_continue:
        //            # display_name
        //            display_name = str(data["display_name"])
        //            # last access
        //            last_access: list[int] = data["last_access"]
        //            # player
        //            player_data: dict[str, Any] = data["player"]
        //            player = _load_player_json(player_data)
        //            # seeds
        //            seeds = data["seeds"]
        //            main_seed = ts.np.random.RandomState()
        //            world_seed = ts.np.random.RandomState()
        //            main_seed.set_state(ts.json_to_random_state(seeds["main_seed"]))
        //            world_seed.set_state(ts.json_to_random_state(seeds["world_seed"]))
        //            tile_type_noise_seeds = seeds["tile_type_noise_seeds"]
        //            Save_data(save_name, display_name, last_access, player, main_seed, world_seed, tile_type_noise_seeds)
        //            World()

        //            # display
        //            ttn_seed_txt = str(Save_data.tile_type_noise_seeds).replace(",", ",\n")
        //            text = f"---------------------------------------------------------------------------------------------------------------\n"\
        //                    f"EXPORTED DATA FROM \"{save_name}\"\n"\
        //                    f"Loaded {SAVE_FILE_NAME_DATA}.{SAVE_EXT}:\n"\
        //                    f"Save name: {Save_data.save_name}\n"\
        //                    f"Display save name: {Save_data.display_save_name}\n"\
        //                    f"Last saved: {make_date(Save_data.last_access, '.')} {make_time(Save_data.last_access[3:])}\n"\
        //                    f"\nPlayer:\n{Save_data.player}\n"\
        //                    f"\nMain seed:\n{ts.random_state_to_json(Save_data.main_seed)}\n"\
        //                    f"\nWorld seed:\n{ts.random_state_to_json(Save_data.world_seed)}\n"\
        //                    f"\nTile type noise seeds:\n{ttn_seed_txt}"\
        //                    f"\n---------------------------------------------------------------------------------------------------------------"
        //            input(text)
        //            ans = sfm.UI_list(["Yes", "No"], f"Do you want export the data from \"{save_name}\" into \"{join(display_visualized_save_path, EXPORT_DATA_FILE)}\"?").display()
        //            if ans == 0:
        //                ts.recreate_folder(EXPORT_FOLDER)
        //                ts.recreate_folder(visualized_save_name, join(ROOT_FOLDER, EXPORT_FOLDER))
        //                with open(join(visualized_save_path, EXPORT_DATA_FILE), "a") as f:
        //                    f.write(text + "\n\n")


        //            ans = sfm.UI_list(["Yes", "No"], f"Do you want export the world data from \"{save_name}\" into an image at \"{display_visualized_save_path}\"?").display()
        //            if ans == 0:
        //                ts.recreate_folder(EXPORT_FOLDER)
        //                ts.recreate_folder(visualized_save_name, join(ROOT_FOLDER, EXPORT_FOLDER))
        //                # get chunks data
        //                World.load_all_chunks_from_folder(show_progress_text = "Getting chunk data...")
        //                # fill
        //                ans = sfm.UI_list(["No", "Yes"], f"Do you want to fill in ALL tiles in ALL generated chunks?").display()
        //                if ans == 1:
        //                    ans = sfm.UI_list(["No", "Yes"], f"Do you want to generates the rest of the chunks in a way that makes the world rectangle shaped?").display()
        //                    if ans == 1:
        //                        print("Generating chunks...", end = "", flush = True)
        //                        World.make_rectangle()
        //                        print("DONE!")
        //                    World.fill_all_chunks("Filling chunks...")
        //                # generate images
        //                # terrain
        //                ans = sfm.UI_list(["Yes", "No"], f"Do you want export the terrain data into \"{EXPORT_TERRAIN_FILE}\"?").display()
        //                if ans == 0:
        //                    make_img("terrain", EXPORT_TERRAIN_FILE)
        //                    input()
        //                # structure
        //                ans = sfm.UI_list(["Yes", "No"], f"Do you want export the structure data into \"{EXPORT_STRUCTURE_FILE}\"?").display()
        //                if ans == 0:
        //                    make_img("structure", EXPORT_STRUCTURE_FILE)
        //                    input()
        //                # population
        //                ans = sfm.UI_list(["Yes", "No"], f"Do you want export the population data into \"{EXPORT_POPULATOIN_FILE}\"?").display()
        //                if ans == 0:
        //                    make_img("population", EXPORT_POPULATOIN_FILE)
        //                    input()
        //                ans = sfm.UI_list(["Yes", "No"], f"Do you want export a combined image into \"{EXPORT_COMBINED_FILE}\"?").display()
        //                if ans == 0:
        //                    make_combined_img(EXPORT_COMBINED_FILE)
        //                    input()
        //    except FileNotFoundError:
        //        print(f"ERROR: {exc_info()[1]}")
        //    ts.threading.current_thread().name = MAIN_THREAD_NAME
    }
}
