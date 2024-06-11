using Playground.Extensions;
using Playground.Resource;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
// ReSharper disable PossibleLossOfFraction

namespace Playground.Mindustry.Blocks
{
    public class PowerNode : Block
    {
        public Image<Rgba32> laser, laser_end;
        private const float LaserOpacity = 0.25f;
        private readonly Rgba32 serpulo_back_color = Color.ParseHex("fbd367").WithAlpha(LaserOpacity), 
            serpulo_front_color = Color.ParseHex("ffffff").WithAlpha(LaserOpacity),
            erec_back_color = Color.ParseHex("CBFA7E").WithAlpha(LaserOpacity), 
            erec_front_color = Color.ParseHex("FFFDFD").WithAlpha(LaserOpacity);
        

        public override void LoadRegions()
        {
            base.LoadRegions();

            var path = DllResource.GetAvailableResources().First(x =>
                Path.GetFileNameWithoutExtension(x) == "laser");
            laser = LoadImage(path);
            path = DllResource.GetAvailableResources().First(x =>
                Path.GetFileNameWithoutExtension(x) == "laser-end");
            using var laser_end_big = LoadImage(path);
            laser_end = laser_end_big.Clone(x => x.Resize(18, 18, new BicubicResampler()));
        }

        public override void UpdateTiling(Schematic schem, TileData tile)
        {
            if (tile.config is not Point2[] points)
                return;
            Lines.Clear();
            // var pos = new PointF((tile.x + 1) * TileSize + (TileSize * Size) / 2, 
            //     (schem.height - tile.y) * TileSize + (TileSize * Size) / 2);
            var pos = new PointF((tile.x + 1) * TileSize + GetUniversalOffset(Size).X, 
                (schem.height - tile.y) * TileSize + GetUniversalOffset(Size).Y);
            foreach (var p in points)
            {
                var secTilePosX = tile.x + p.X;
                var secTilePosY = tile.y + p.Y;

                if(secTilePosX < 0 || secTilePosY < 0 ||
                   secTilePosX >= schem.width || secTilePosY >= schem.height)
                    continue;


                var b = Blocks.blocks[schem.Tiles[tile.x + p.X, tile.y + p.Y].block.name];

                if (b is PowerNode)
                {
                    if(secTilePosX > tile.x)
                        continue;
                    if (secTilePosX == tile.x && secTilePosY > tile.y)
                        continue;
                }
                
                var secPos = new PointF((secTilePosX + 1) * TileSize + GetUniversalOffset(b.Size).X,
                    (schem.height - secTilePosY) * TileSize + GetUniversalOffset(b.Size).Y);
                
                var angle = GetAngle(pos, secPos);

                var vx = (float) Math.Cos(angle);
                var vy = (float) Math.Sin(angle);

                var len1 = Size * TileSize / 2 - 1.5f;
                var len2 = b.Size * TileSize / 2 - 1.5f;

  
                Lines.Add(new []
                {
                    new PointF(pos.X - vx * len1, pos.Y - vy * len1),
                    new PointF(secPos.X + vx * len2, secPos.Y + vy * len2)
                });
            }
        }

        private readonly List<PointF[]> Lines = new();

        public override void PostRenderBlockDirect(Image<Rgba32> image, TileData tileData, Point renderPosition)
        {
            if(tileData.config is not Point2[])
                return;

            image.ProcessPixelRows(pixels =>
            {
                foreach (var p in Lines.SelectMany(line => line))
                {
                    pixels.RenderTileShifted((p.X - laser_end.Width / 2) / TileSize,
                        (p.Y - laser_end.Height / 2) / TileSize, laser_end, 
                        Point.Empty, Bridge.BridgeAlpha);
                }
            });

            image.Mutate(x =>
            {
                foreach (var line in Lines)
                {
                    if (BlockType == "LongPowerNode")
                    {
                        x.DrawLine(erec_back_color, 9, line);
                        x.DrawLine(erec_front_color, 5, line);
                    }else
                    {
                        x.DrawLine(serpulo_back_color, 7, line);
                        x.DrawLine(serpulo_front_color, 3, line);
                    }
                }
            });
        }


        public static float GetAngle(PointF start, PointF end)
        {
            return (float)Math.Atan2((start - end).Y, (start - end).X);
        }

        public const float mRad = 180 / (float) Math.PI;
    }
}
