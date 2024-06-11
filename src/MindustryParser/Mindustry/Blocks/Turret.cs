using Playground.Extensions;
using Playground.Resource;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Playground.Mindustry.Blocks
{
    public class Turret : Block
    {
        public Image<Rgba32> Region, turret, block;

        protected static readonly Rgba32 OutlieColor = Rgba32.ParseHex("404049");
        protected static readonly Rgba32 ReinforcedOutlieColor = Rgba32.ParseHex("2D2F39");

        public override void LoadRegions()
        {
            var path = DllResource.GetAvailableResources()
                .FirstOrDefault(x => Path.GetFileNameWithoutExtension(x) == $"{BlockName}-preview");
            if (!string.IsNullOrEmpty(path))
            {
                turret = LoadImage(path);
            }
            else
            {
                path = DllResource.GetAvailableResources()
                    .First(x => Path.GetFileNameWithoutExtension(x) == $"{BlockName}");
                turret = LoadImage(path);
            }

            var regionPrefix = DarkRegion ? "reinforced-" : string.Empty;

            path = DllResource.GetAvailableResources()
                .FirstOrDefault(x => Path.GetFileNameWithoutExtension(x) == $"{BlockName}-base");
            if (!string.IsNullOrEmpty(path))
            {
                block = LoadImage(path);
            }
            else
            {
                path = DllResource.GetAvailableResources()
                    .First(x => Path.GetFileNameWithoutExtension(x) == $"{regionPrefix}block-{Size}");
                block = LoadImage(path);
            }

            Region = block.Clone();
            Region.ProcessPixelRows(pixels =>
            {
                pixels.RenderTileOverlayWithShadow(turret, DarkRegion ? ReinforcedOutlieColor : OutlieColor, 2);
            });
            Regions.Add(BlockName, Region);
        }
    }
}
