using Playground.Extensions;
using Playground.Resource;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Playground.Mindustry.Blocks;

public class HeatProducer : Block
{
    public static readonly Rgba32 HeatColor = Color.ParseHex("BC5452");
    private Image<Rgba32> region;
    private Image<Rgba32> heat_right, heat_up, heat_left, heat_down;

    public override void LoadRegions()
    {
        var path = DllResource.GetAvailableResources()
            .First(x => Path.GetFileNameWithoutExtension(x) == BlockName);
        region = LoadImage(path);
        
        path = DllResource.GetAvailableResources()
            .First(x => Path.GetFileNameWithoutExtension(x) == $"{BlockName}-top1");

        heat_right = LoadImage(path);
        heat_up = heat_right.Clone(x => x.Rotate(RotateMode.Rotate270));
        
        path = DllResource.GetAvailableResources()
            .First(x => Path.GetFileNameWithoutExtension(x) == $"{BlockName}-top2");
        using var img = LoadImage(path);
        heat_left = img.Clone(x => x.Rotate(RotateMode.Rotate180));
        heat_down = img.Clone(x => x.Rotate(RotateMode.Rotate90));
    }

    public override void RenderBlock(PixelAccessor<Rgba32> pixels, TileData tileData, Point renderPosition)
    {
        int tileOffset = getTileOffset();

        for (int j = 0; j < Size; j++)
        {
            for (int i = 0; i < Size; i++)
            {
                pixels.RenderTile(new Point(renderPosition.X + i - tileOffset,
                    renderPosition.Y - j + tileOffset), region, new Point(i, Size - j - 1));
                switch (tileData.rotation)
                {
                    case 0:
                    pixels.RenderTile(new Point(renderPosition.X + i - tileOffset,
                        renderPosition.Y - j + tileOffset), heat_right, new Point(i, Size - j - 1));
                        break;
                    case 1:
                        pixels.RenderTile(new Point(renderPosition.X + i - tileOffset,
                            renderPosition.Y - j + tileOffset), heat_up, new Point(i, Size - j - 1));
                        break;
                    case 2:
                        pixels.RenderTile(new Point(renderPosition.X + i - tileOffset,
                            renderPosition.Y - j + tileOffset), heat_left, new Point(i, Size - j - 1));
                        break;
                    case 3:
                        pixels.RenderTile(new Point(renderPosition.X + i - tileOffset,
                            renderPosition.Y - j + tileOffset), heat_down, new Point(i, Size - j - 1));
                        break;
                }
                
            }
        }
    }
}