using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Playground.Extensions;
using Playground.Resource;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;


namespace Playground.Mindustry.Blocks
{
    public static class SchematicDrawer
    {

        private static Image<Rgba32>? BackgroundTile;
        
        public static Image DrawSchemePreview(Schematic scheme)
        {

            if (BackgroundTile == null)
            {
                //dark-panel-3
                using var str = DllResource.GetStream(
                    DllResource.GetAvailableResources()
                        .First(x => x.Contains("dark-panel-3")));
                if (str == null)
                    throw new MissingMemberException("Unable to load background sprite!");
                BackgroundTile = Image.Load<Rgba32>(str);
            }

            var image = new Image<Rgba32>((scheme.width + 2) * Block.TileSize, (scheme.height + 2) * Block.TileSize);

            image.ProcessPixelRows(pixels =>
            {
                for (int i = 0; i < scheme.width + 2; i++)
                {
                    for (int j = 0; j < scheme.height + 2; j++)
                    {
                        pixels.RenderTile(new Point(i, j), BackgroundTile, Point.Empty);
                    }
                }
            });
            
            image.ProcessPixelRows(pixels =>
            {
                foreach (var tile in scheme.tiles)
                {
                    try
                    {
                        var block = Blocks.blocks[tile.block.name];
                        var pos = new Point(tile.x + 1, (scheme.height - tile.y));
                        block.RenderShadow(pixels, tile, pos);
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }
                }
            });
            
            // image.Save("stub.png");

            image.ProcessPixelRows(pixels =>
            {
                foreach (var tile in scheme.tiles)
                {
                    Block block = null;
                    try
                    {
                        block = Blocks.blocks[tile.block.name];
                        var pos = new Point(tile.x + 1, (scheme.height - tile.y));
                        block.UpdateTiling(scheme, tile);
                        block.RenderBlock(pixels, tile, pos);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(block != null
                            ? $"Failed to render block {block.BlockName}"
                            : $"Failed to retrieve block info for {tile.block.name}");
                    }
                }
            });

            foreach (var tile in scheme.tiles)
            {
                try
                {
                    var block = Blocks.blocks[tile.block.name];

                    var pos = new Point(tile.x + 1, (scheme.height - tile.y));
                    block.UpdateTiling(scheme, tile);
                    block.PostRenderBlockDirect(image, tile, pos);
                }
                catch (Exception e)
                {
                    // ignored
                }
            }

            image.ProcessPixelRows(pixels =>
            {
                foreach (var tile in scheme.tiles)
                {
                    try
                    {
                        var block = Blocks.blocks[tile.block.name];
                        var pos = new Point(tile.x + 1, (scheme.height - tile.y));
                        block.UpdateTiling(scheme, tile);
                        block.PostRenderBlock(pixels, tile, pos);
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }
                }
            });

            return image;
        }
        
    }
}
