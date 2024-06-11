using Playground.Extensions;
using Playground.Resource;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Playground.Mindustry.Blocks;

public class DuctRouter : Block
{
    private Image<Rgba32> Center;
    private Image<Rgba32>? arrow_right, arrow_up,arrow_left,arrow_down;
    private Image<Rgba32>? top_right, top_up,top_left,top_down;
    
    public override void LoadRegions()
    {
        base.LoadRegions();
        var path = DllResource.GetAvailableResources().First(x => x.EndsWith("/duct-unloader-center.png"));

        using var stream = DllResource.GetStream(path);
        Center = Image.Load<Rgba32>(stream);
                
        if(Regions.ContainsKey($"{BlockName}-top"))
        {
            top_right = Regions[$"{BlockName}-top"];
            top_up = top_right.Clone(x => x.Rotate(RotateMode.Rotate270));
            top_left = top_right.Clone(x => x.Rotate(RotateMode.Rotate180));
            top_down = top_right.Clone(x => x.Rotate(RotateMode.Rotate90));
        }

        if (Regions.Any(x => x.Key.EndsWith($"{BlockName}-arrow")))
        {
            arrow_right = Regions[$"{BlockName}-arrow"];
            arrow_up = arrow_right.Clone(x => x.Rotate(RotateMode.Rotate270));
            arrow_left = arrow_right.Clone(x => x.Rotate(RotateMode.Rotate180));
            arrow_down = arrow_right.Clone(x => x.Rotate(RotateMode.Rotate90));
        }
        
        
    }

    public override void RenderBlock(PixelAccessor<Rgba32> pixels, TileData tileData, Point renderPosition)
    {
        pixels.RenderTile(renderPosition, Regions[BlockName], Point.Empty);

        //If filter is set - render filter region
        if (tileData.config is Content {type: ContentType.Item} content)
        {
            if (Items.itemsById.TryGetValue(content.id, out var item))
            {
                pixels.RenderTileMask(renderPosition, Center, Point.Empty, item.Color);
            }
            else
            {
                pixels.RenderTile(renderPosition, Center, Point.Empty);
            }
            if (BlockName.Contains("router"))
                return;
        }
        else if(arrow_right != null)//Else draw arrow region
        {
            switch (tileData.rotation)
            {
                case 0:
                    pixels.RenderTile(renderPosition, arrow_right, Point.Empty);
                    return;
                case 1:
                    pixels.RenderTile(renderPosition, arrow_up, Point.Empty);
                    return;
                case 2:
                    pixels.RenderTile(renderPosition, arrow_left, Point.Empty);
                    return;
                case 3:
                    pixels.RenderTile(renderPosition, arrow_down, Point.Empty);
                    return;
            }
        }
        if(top_right != null)
        {
            switch (tileData.rotation)
            {
                case 0:
                    pixels.RenderTile(renderPosition, top_right, Point.Empty);
                    return;
                case 1:
                    pixels.RenderTile(renderPosition, top_up, Point.Empty);
                    return;
                case 2:
                    pixels.RenderTile(renderPosition, top_left, Point.Empty);
                    return;
                case 3:
                    pixels.RenderTile(renderPosition, top_down, Point.Empty);
                    return;
            }
        }
    }
}