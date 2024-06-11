using System.Collections.Immutable;
using Playground.Extensions;
using Playground.Resource;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Playground.Mindustry.Blocks
{
    public class Sorter : Block
    {
        public Image<Rgba32> Center;
        public Image<Rgba32> Cross;

        public override void LoadRegions()
        {
            base.LoadRegions();
            var path = DllResource.GetAvailableResources().First(x => x.EndsWith("/center.png"));

            using var stream = DllResource.GetStream(path);
            Center = Image.Load<Rgba32>(stream);

            path = DllResource.GetAvailableResources().First(x => x.EndsWith("/cross-full.png"));

            using var stream2 = DllResource.GetStream(path);
            Cross = Image.Load<Rgba32>(stream2);
        }

        public override void RenderBlock(PixelAccessor<Rgba32> pixels, TileData tileData, Point renderPosition)
        {
            var content = (Content) tileData.config;
            if (content != null)
            {
                if (content.type == ContentType.Item && Items.itemsById.TryGetValue(content.id, out var item))
                {
                    pixels.RenderTileMask(renderPosition, Center, Point.Empty, item.Color);
                    pixels.RenderTile(renderPosition, Regions[BlockName], Point.Empty);
                    return;
                }
                else
                if (content.type == ContentType.Liquid && Items.liquidsById.TryGetValue(content.id, out var liquid))
                {
                    pixels.RenderTileMask(renderPosition, Center, Point.Empty, liquid.Color);
                    pixels.RenderTile(renderPosition, Regions[BlockName], Point.Empty);
                    return;
                }
            }
            base.RenderBlock(pixels, tileData, renderPosition);
        }

        public override IEnumerable<Image<Rgba32>> GetRenderSprites()
        {
            return new Image<Rgba32>[]
            {
                Cross, Regions[BlockName]
            };
        }
    }
}
