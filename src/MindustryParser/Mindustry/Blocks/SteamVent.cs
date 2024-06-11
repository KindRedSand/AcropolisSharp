using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Playground.Mindustry.Blocks;

public class SteamVent : Block
{
    // private static Image<Rgba32> _region;
    //
    // static SteamVent()
    // {
    //     _region = new Image<Rgba32>(TileSize * 3, TileSize * 3);
    //     _region.ProcessPixelRows(pixels =>
    //     {
    //         var c = new Rgba32(0.5f, 0.5f, 0.5f, 0.5f);
    //         
    //         for (int y = 0; y < pixels.Height; y++)
    //         {
    //             var row = pixels.GetRowSpan(y);
    //             for (int x = 0; x < pixels.Width; x++)
    //             {
    //                 row[x] = c;
    //             }
    //         }
    //     });
    //     
    // }

    public SteamVent()
    {
        Size = 2;
    }
    
    // public override void LoadRegions()
    // {
    //     Regions.Add(BlockName, _region);
    // }

    // public override IEnumerable<Image<Rgba32>> GetRenderSprites()
    // {
    //     return [_region];
    // }
}