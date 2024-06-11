using Playground.Extensions;
using Playground.Resource;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;  

namespace Playground.Mindustry.Blocks
{
    public class PayloadRouter : Block
    {
        public Image<Rgba32>[] Region = new Image<Rgba32>[4];


        public override void LoadRegions()
        {
            Size = 3;

            var path = DllResource.GetAvailableResources()
                .First(x => Path.GetFileNameWithoutExtension(x) == BlockName);
            var back = LoadImage(path);

            path = DllResource.GetAvailableResources()
                .First(x => Path.GetFileNameWithoutExtension(x) == $"{BlockName}-over");
            var over = LoadImage(path);

            path = DllResource.GetAvailableResources()
                .First(x => Path.GetFileNameWithoutExtension(x) == $"{BlockName}-top");
            var arrow = LoadImage(path);

            back.ProcessPixelRows(pixels =>
            {
                pixels.RenderTileOverlay(over);
            });

            Region[0] = back.Clone();
            Region[0].ProcessPixelRows(pixels =>
            {
                pixels.RenderTileOverlay(arrow);
            });
            Region[1] = back.Clone();
            Region[1].ProcessPixelRows(pixels =>
            {
                using var rot = arrow.Clone(x => x.Rotate(RotateMode.Rotate270));
                pixels.RenderTileOverlay(rot);
            });
            Region[2] = back.Clone();
            Region[2].ProcessPixelRows(pixels =>
            {
                using var rot = arrow.Clone(x => x.Rotate(RotateMode.Rotate180));
                pixels.RenderTileOverlay(rot);
            });
            Region[3] = back.Clone();
            Region[3].ProcessPixelRows(pixels =>
            {
                using var rot = arrow.Clone(x => x.Rotate(RotateMode.Rotate90));
                pixels.RenderTileOverlay(rot);
            });
        }

        public override void RenderBlock(PixelAccessor<Rgba32> pixels, TileData tileData, Point renderPosition)
        {
            var tileOffset = getTileOffset();
            var img = Region[tileData.rotation];

            for (int j = 0; j < Size; j++)
            {
                for (int i = 0; i < Size; i++)
                {
                    pixels.RenderTile(new Point(renderPosition.X + i - tileOffset,
                        renderPosition.Y - j + tileOffset), img, new Point(i, Size - j - 1));
                }
            }
        }
    }
}
