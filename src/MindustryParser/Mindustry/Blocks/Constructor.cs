using Playground.Extensions;
using Playground.Resource;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Playground.Mindustry.Blocks
{
    public class Constructor : UnitFactory
    {
    }

    public class Deconstructor : Block
    {
        public Image<Rgba32> baseImage, factory_in_right, factory_in_up, factory_in_left, factory_in_down, factory_top;

        public Dictionary<Sides, Image<Rgba32>> RegionMap = new();


        public override void LoadRegions()
        {
            var regionSuffix = DarkRegion ? "-dark" : string.Empty;

            var path = DllResource.GetAvailableResources()
                .First(x => Path.GetFileNameWithoutExtension(x) == BlockName);
            baseImage = LoadImage(path);

            path = DllResource.GetAvailableResources()
                .First(x => Path.GetFileNameWithoutExtension(x) == $"factory-in-{Size}{regionSuffix}");
            factory_in_left = LoadImage(path);
            factory_in_up = factory_in_left.Clone(x => x.Rotate(RotateMode.Rotate270));
            factory_in_right = factory_in_left.Clone(x => x.Rotate(RotateMode.Rotate180));
            factory_in_down = factory_in_left.Clone(x => x.Rotate(RotateMode.Rotate90));

            path = DllResource.GetAvailableResources()
                .FirstOrDefault(x => Path.GetFileNameWithoutExtension(x) == $"{BlockName}-top");
            if (!string.IsNullOrEmpty(path))
            {
                factory_top = LoadImage(path);
            }
            else if (Size is 3 or 5)
            {
                path = DllResource.GetAvailableResources()
                    .First(x => Path.GetFileNameWithoutExtension(x) == $"factory-top-{Size}");
                factory_top = LoadImage(path);
            }
            {
                var img = new Image<Rgba32>(baseImage.Size.Width, baseImage.Height);
                img.ProcessPixelRows(pixels =>
                {
                    // pixels.RenderTileOverlay(factory_top);
                });
                RegionMap.Add(Sides.None, img);
            }
            {
                var img = new Image<Rgba32>(baseImage.Size.Width, baseImage.Height);
                img.ProcessPixelRows(pixels =>
                {
                    pixels.RenderTileOverlay(factory_in_right);
                    // pixels.RenderTileOverlay(factory_top);
                });
   
                RegionMap.Add(Sides.Right, img);
            }
        }

        private Sides side = Sides.None;

        public override void RenderBlock(PixelAccessor<Rgba32> pixels, TileData tileData, Point renderPosition)
        {
            var tileOffset = getTileOffset();

            for (int j = 0; j < Size; j++)
            {
                for (int i = 0; i < Size; i++)
                {
                    pixels.RenderTile(new Point(renderPosition.X + i - tileOffset,
                        renderPosition.Y - j + tileOffset), baseImage, new Point(i, Size - j - 1));

                    if (side.HasFlag(Sides.Right))
                    {
                        pixels.RenderTile(new Point(renderPosition.X + i - tileOffset,
                            renderPosition.Y - j + tileOffset), factory_in_right, new Point(i, Size - j - 1));
                    }
                    if (side.HasFlag(Sides.Left))
                    {
                        pixels.RenderTile(new Point(renderPosition.X + i - tileOffset,
                            renderPosition.Y - j + tileOffset), factory_in_left, new Point(i, Size - j - 1));
                    }
                    if (side.HasFlag(Sides.Up))
                    {
                        pixels.RenderTile(new Point(renderPosition.X + i - tileOffset,
                            renderPosition.Y - j + tileOffset), factory_in_up, new Point(i, Size - j - 1));
                    }
                    if (side.HasFlag(Sides.Down))
                    {
                        pixels.RenderTile(new Point(renderPosition.X + i - tileOffset,
                            renderPosition.Y - j + tileOffset), factory_in_down, new Point(i, Size - j - 1));
                    }
                    pixels.RenderTile(new Point(renderPosition.X + i - tileOffset,
                        renderPosition.Y - j + tileOffset), factory_top, new Point(i, Size - j - 1));
                }
            }
        }



        public override void UpdateTiling(Schematic schem, TileData tile)
        {
            TileData? up = null, left = null, right = null, down = null;
            side = Sides.None;

            foreach (var t in schem.tiles)
            {
                var conn = isConnectable(tile, t);
                switch (conn)
                {
                    case Sides.Up:
                        side |= Sides.Down; break;
                    case Sides.Down:
                        side |= Sides.Up; break;
                    case Sides.Left:
                        side |= Sides.Right; break;
                    case Sides.Right:
                        side |= Sides.Left; break;
                }
            }
        }

        private Sides isConnectable(TileData me, TileData tile)
        {
            var b = Blocks.blocks[tile.block.name];

            if (b.BlockType is "PayloadConveyor" or "PayloadRouter" or "PayloadMassDriver" or "Reconstructor" 
                or "UnitFactory" or "Constructor" or "PayloadUnloader" or "PayloadLoader")
            {
                int space = Size - 2;
                int halfSpace = (space - 1) / 2;
                //int offset = (Size - 3) / 2 + 3;
                int offset = (Size - 1) / 2 + (b.Size - 1) / 2 + 1;

                if (tile.x >= me.x - halfSpace && tile.x <= me.x + halfSpace)
                {
                    if (tile.y == me.y - offset)
                    {
                        return b.BlockType == "PayloadRouter" || tile.rotation == 3 ? Sides.Down : Sides.None;
                    }
                    if (tile.y == me.y + offset)
                    {
                        return b.BlockType == "PayloadRouter" || tile.rotation == 1 ? Sides.Up : Sides.None;
                    }
                }
                if (tile.y >= me.y - halfSpace && tile.y <= me.y + halfSpace)
                {
                    if (tile.x == me.x - offset)
                    {
                        return b.BlockType == "PayloadRouter" || tile.rotation == 0 ? Sides.Right : Sides.None;
                    }
                    if (tile.x == me.x + offset)
                    {
                        return b.BlockType == "PayloadRouter" || tile.rotation == 2 ? Sides.Left : Sides.None;
                    }
                }
            }

            return Sides.None;
        }

        public override IEnumerable<Image<Rgba32>> GetRenderSprites()
        {
            return new[] { RegionMap.First().Value };
        }



        [Flags]
        public enum Sides
        {
            None = 0,
            Left = 1 << 0,
            Right = 1 << 1,
            Back = 1 << 2,

            Up = 10,
            Down = 11,
        }
    }
}
