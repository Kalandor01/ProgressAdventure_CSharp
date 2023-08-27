using ProgressAdventure;
using ProgressAdventure.WorldManagement;
using ProgressAdventure.WorldManagement.Content;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using PAConstants = ProgressAdventure.Constants;
using PAUtils = ProgressAdventure.Utils;
using PATools = ProgressAdventure.Tools;
using System.Linq;
using ProgressAdventure.Extensions;
using ProgressAdventure.Entity;

namespace PAVisualiser
{
    public static class LegacyVisualiser
    {
        public static ColorData GetTileColor(Tile tile, WorldLayer layer, Dictionary<ContentTypeID, long> tileTypeCounts, double opacityMultiplier = 1)
        {
            ContentTypeID subtype;
            if (layer == WorldLayer.Structure)
            {
                subtype = tile.structure.subtype;
            }
            else if (layer == WorldLayer.Population)
            {
                subtype = tile.population.subtype;
            }
            else
            {
                subtype = tile.terrain.subtype;
            }

            if (tileTypeCounts.ContainsKey(subtype))
            {
                tileTypeCounts[subtype]++;
            }
            else
            {
                tileTypeCounts[subtype] = 1;
            }

            ColorData rawColor = VisualiserUtils.contentSubtypeColorMap.TryGetValue(subtype, out ColorData rColor) ? rColor : Constants.Colors.MAGENTA;

            return new ColorData(rawColor.R, rawColor.G, rawColor.B, (byte)Math.Clamp(rawColor.A * opacityMultiplier, 0, 255));
        }

        /// <summary>
        /// Genarates an image, representing the different types of tiles, and their placements in the world.
        /// </summary>
        /// <param name="layer">Sets witch layer to export.</param>
        /// <param name="image">The generated image.</param>
        /// <param name="opacityMultiplier">The opacity multiplier for the tiles.</param>
        /// <returns>The tile count for all tile types.</returns>
        public static (Dictionary<ContentTypeID, long> tileTypeCounts, long totalTileCount)? CreateWorldLayerImage(WorldLayer layer, out Bitmap? image, double opacityMultiplier = 1)
        {
            (int x, int y) tileSize = (1, 1);


            var tileTypeCounts = new Dictionary<ContentTypeID, long>();
            var totalTileCount = 0L;

            var worldCorners = World.GetCorners();

            if (worldCorners is null)
            {
                image = null;
                return null;
            }

            var (minX, minY, maxX, maxY) = worldCorners.Value;

            (long x, long y) size = ((maxX - minX + 1) * tileSize.x, (maxY - minY + 1) * tileSize.y);

            image = new Bitmap((int)size.x, (int)size.y);
            var drawer = Graphics.FromImage(image);

            foreach (var chunk in World.Chunks.Values)
            {
                foreach (var tile in chunk.tiles.Values)
                {
                    var x = chunk.basePosition.x + tile.relativePosition.x - minX;
                    var y = chunk.basePosition.y + tile.relativePosition.y - minY;
                    var startX = x * tileSize.x;
                    var startY = size.y - y * tileSize.y - 1;
                    // find type
                    totalTileCount++;
                    var color = GetTileColor(tile, layer, tileTypeCounts, opacityMultiplier);
                    drawer.DrawRectangle(new Pen(color.ToDrawingColor()), startX, startY, tileSize.x, tileSize.y);
                }
            }
            return (tileTypeCounts, totalTileCount);
        }

        private static void CombineImageWithNewLayerImage(List<WorldLayer> layers, WorldLayer layer, Image? image, Dictionary<WorldLayer,(Dictionary<ContentTypeID, long> tileTypeCounts, long totalTileCount)> layerCounts)
        {
            if (layers.Contains(layer))
            {
                var structureTileTypeCounts = CreateWorldLayerImage(layer, out Bitmap? structureImage, layers.Count);

                if (structureTileTypeCounts is null || structureImage is null)
                {
                    return;
                }

                layerCounts.Add(layer, ((Dictionary<ContentTypeID, long> tileTypeCounts, long totalTileCount))structureTileTypeCounts);

                if (image is null)
                {
                    image = structureImage;
                }
                else
                {
                    var grfx = Graphics.FromImage(image);
                    grfx.DrawImage(structureImage, 0, 0);
                }
            }
        }

        public static Dictionary<WorldLayer, (Dictionary<ContentTypeID, long> tileTypeCounts, long totalTileCount)> CreateCombinedImage(List<WorldLayer> layers, out Image? image)
        {
            image = null;

            var layerCounts = new Dictionary<WorldLayer, (Dictionary<ContentTypeID, long> tileTypeCounts, long totalTileCount)>();

            foreach (var layer in Enum.GetValues<WorldLayer>())
            {
                CombineImageWithNewLayerImage(layers, layer, image, layerCounts);
            }

            return layerCounts;
        }

        public static void MakeImage(List<WorldLayer> layers, string exportPath)
        {
            Console.Write("Generating image...");
            var tileTypeCounts = CreateCombinedImage(layers, out Image? image);
            Console.WriteLine("DONE!");

            if (tileTypeCounts is null || image is null)
            {
                return;
            }

            var txt = new StringBuilder();
            foreach (var layer in tileTypeCounts)
            {
                txt.AppendLine($"{layer.Key} tile types:");
                foreach (var type in layer.Value.tileTypeCounts)
                {
                    txt.AppendLine($"\t{type.Key}: {type.Value}");
                }
                txt.AppendLine($"\tTOTAL: {layer.Value.totalTileCount}\n");
            }
            Console.WriteLine(txt);

            image.Save(exportPath);
        }

        /// <summary>
        /// Visualises the data in a save file.
        /// </summary>
        /// <param name="saveName">The name of the save to read.</param>
        public static void SaveVisualizer(string saveName)
        {
            var saveFolderPath = Path.Join(PAConstants.SAVES_FOLDER_PATH, saveName);
            var now = DateTime.Now;
            var visualizedSaveFolderName = $"{saveName}_{PAUtils.MakeDate(now)}_{PAUtils.MakeTime(now, ";")}";
            var displayVisualizedSavePath = Path.Join(Constants.EXPORT_FOLDER, visualizedSaveFolderName);
            var visualizedSavePath = Path.Join(PAConstants.ROOT_FOLDER, displayVisualizedSavePath);

            // load
            try
            {
                SaveManager.LoadSave(saveName, false, false);
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR: {e}");
                return;
            }

            // display
            var txt = new StringBuilder();
            txt.AppendLine($"---------------------------------------------------------------------------------------------------------------");
            txt.AppendLine($"EXPORTED DATA FROM \"{SaveData.saveName}\"");
            txt.AppendLine($"Loaded {PAConstants.SAVE_FILE_NAME_DATA}.{PAConstants.SAVE_EXT}:");
            txt.AppendLine($"Save name: {SaveData.saveName}");
            txt.AppendLine($"Display save name: {SaveData.displaySaveName}");
            txt.AppendLine($"Last saved: {PAUtils.MakeDate(SaveData.LastSave, ".")} {PAUtils.MakeTime(SaveData.LastSave)}");
            txt.AppendLine($"\nPlayer:\n{SaveData.player}");
            txt.AppendLine($"\nMain seed: {PATools.SerializeRandom(RandomStates.MainRandom)}");
            txt.AppendLine($"World seed: {PATools.SerializeRandom(RandomStates.WorldRandom)}");
            txt.AppendLine($"Misc seed: {PATools.SerializeRandom(RandomStates.MiscRandom)}");
            txt.AppendLine($"Chunk seed modifier: {PATools.SerializeRandom(RandomStates.WorldRandom)}");
            txt.AppendLine($"\nTile type noise seeds:\n{string.Join("\n", RandomStates.TileTypeNoiseSeeds.Select(ttns => $"{ttns.Key.ToString().Capitalize()} seed: {ttns.Value}"))}");
            txt.Append("\n---------------------------------------------------------------------------------------------------------------");
            PAUtils.PressKey(txt.ToString());
            //ans = sfm.UI_list(["Yes", "No"], f"Do you want export the data from \"{save_name}\" into \"{join(display_visualized_save_path, EXPORT_DATA_FILE)}\"?").display()
            //if ans == 0:
            //    ts.recreate_folder(EXPORT_FOLDER)
            //    ts.recreate_folder(visualized_save_name, join(ROOT_FOLDER, EXPORT_FOLDER))
            //    with open(join(visualized_save_path, EXPORT_DATA_FILE), "a") as f:
            //        f.write(text + "\n\n")


            //ans = sfm.UI_list(["Yes", "No"], f"Do you want export the world data from \"{save_name}\" into an image at \"{display_visualized_save_path}\"?").display()
            //if ans == 0:
            //    ts.recreate_folder(EXPORT_FOLDER)
            //    ts.recreate_folder(visualized_save_name, join(ROOT_FOLDER, EXPORT_FOLDER))
            //    // get chunks data
            //    World.load_all_chunks_from_folder(show_progress_text = "Getting chunk data...")
            //    // fill
            //    ans = sfm.UI_list(["No", "Yes"], f"Do you want to fill in ALL tiles in ALL generated chunks?").display()
            //    if ans == 1:
            //        ans = sfm.UI_list(["No", "Yes"], f"Do you want to generates the rest of the chunks in a way that makes the world rectangle shaped?").display()
            //        if ans == 1:
            //            print("Generating chunks...", end = "", flush = True)
            //            World.make_rectangle()
            //            print("DONE!")
            //        World.fill_all_chunks("Filling chunks...")
            //    // generate images
            //    // terrain
            //    ans = sfm.UI_list(["Yes", "No"], f"Do you want export the terrain data into \"{EXPORT_TERRAIN_FILE}\"?").display()
            //    if ans == 0:
            //        make_img("terrain", EXPORT_TERRAIN_FILE)
            //        input()
            //    // structure
            //    ans = sfm.UI_list(["Yes", "No"], f"Do you want export the structure data into \"{EXPORT_STRUCTURE_FILE}\"?").display()
            //    if ans == 0:
            //        make_img("structure", EXPORT_STRUCTURE_FILE)
            //        input()
            //    // population
            //    ans = sfm.UI_list(["Yes", "No"], f"Do you want export the population data into \"{EXPORT_POPULATOIN_FILE}\"?").display()
            //    if ans == 0:
            //        make_img("population", EXPORT_POPULATOIN_FILE)
            //        input()
            //    ans = sfm.UI_list(["Yes", "No"], f"Do you want export a combined image into \"{EXPORT_COMBINED_FILE}\"?").display()
            //    if ans == 0:
            //        make_combined_img(EXPORT_COMBINED_FILE)
            //        input()
        }
    }
}
