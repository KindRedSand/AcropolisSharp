using Playground.Resource;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playground.Extensions;

namespace Playground.Mindustry.Blocks
{
    public class Unloader : Block
    {
        public Image<Rgba32> Center;

        public override void RenderBlock(PixelAccessor<Rgba32> pixels, TileData tileData, Point renderPosition)
        {
            var content = (Content)tileData.config;
            if (content != null && content.type == ContentType.Item)
            {
                if (Items.itemsById.TryGetValue(content.id, out var item))
                {
                    pixels.RenderTile(renderPosition, Regions[BlockName], Point.Empty);
                    pixels.RenderTileMask(renderPosition, Regions[$"{BlockName}-center"], Point.Empty, item.Color);
                    return;
                }
            }
            base.RenderBlock(pixels, tileData, renderPosition);
        }

        public override IEnumerable<Image<Rgba32>> GetRenderSprites()
        {
            return new Image<Rgba32>[]
            {
                Regions[BlockName]
            };
        }
    }
}
