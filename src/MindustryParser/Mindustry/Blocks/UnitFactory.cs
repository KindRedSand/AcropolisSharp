using Playground.Extensions;
using Playground.Resource;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Playground.Mindustry.Blocks
{
    public class UnitFactory : Block
    {
        public Image<Rgba32>[] Region = new Image<Rgba32>[4];
        public Image<Rgba32> factory_out_right, factory_out_up, factory_out_left, factory_out_down, factory_top;

        public override void LoadRegions()
        {
            var regionSuffix = DarkRegion ? "-dark" : string.Empty;

            var path = DllResource.GetAvailableResources()
                .First(x => Path.GetFileNameWithoutExtension(x) == BlockName);
            var baseImage = LoadImage(path);

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

            
            if (BlockType is "UnitAssembler")
            {
                path = DllResource.GetAvailableResources()
                    .First(x => Path.GetFileNameWithoutExtension(x) == $"{BlockName}-side1");
                factory_out_right = LoadImage(path);
                factory_out_up = factory_out_right.Clone(x => x.Rotate(RotateMode.Rotate270));
                factory_out_left = factory_out_right.Clone(x => x.Rotate(RotateMode.Rotate180));
                factory_out_down = factory_out_right.Clone(x => x.Rotate(RotateMode.Rotate90));
            }
            else
            {
                path = DllResource.GetAvailableResources()
                    .First(x => Path.GetFileNameWithoutExtension(x) == $"factory-out-{Size}{regionSuffix}");
                factory_out_right = LoadImage(path);
                factory_out_up = factory_out_right.Clone(x => x.Rotate(RotateMode.Rotate270));
                factory_out_left = factory_out_right.Clone(x => x.Rotate(RotateMode.Rotate180));
                factory_out_down = factory_out_right.Clone(x => x.Rotate(RotateMode.Rotate90));
            }
            

            Region[0] = baseImage.Clone();
            Region[0].ProcessPixelRows(pixels =>
            {
                pixels.RenderTileOverlay(factory_out_right);
                pixels.RenderTileOverlay(factory_top);
            });
            Region[1] = baseImage.Clone();
            Region[1].ProcessPixelRows(pixels =>
            {
                pixels.RenderTileOverlay(factory_out_up);
                pixels.RenderTileOverlay(factory_top);
            });
            Region[2] = baseImage.Clone();
            Region[2].ProcessPixelRows(pixels =>
            {
                pixels.RenderTileOverlay(factory_out_left);
                pixels.RenderTileOverlay(factory_top);
            });
            Region[3] = baseImage.Clone();
            Region[3].ProcessPixelRows(pixels =>
            {
                pixels.RenderTileOverlay(factory_out_down);
                pixels.RenderTileOverlay(factory_top);
            });
        }

        public override void RenderBlock(PixelAccessor<Rgba32> pixels, TileData tileData, Point renderPosition)
        {
            int tileOffset = getTileOffset();

            for (int j = 0; j < Size; j++)
            {
                for (int i = 0; i < Size; i++)
                {
                    pixels.RenderTile(new Point(renderPosition.X + i - tileOffset,
                        renderPosition.Y - j + tileOffset), Region[tileData.rotation], new Point(i, Size - j - 1));
                }
            }
        }
    }
}
