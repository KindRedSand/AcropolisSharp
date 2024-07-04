using System.Collections.Immutable;
using Playground.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing;

namespace Playground.Mindustry.Blocks;

public class DuctBridge : Block
{
        // private Image<Rgba32> end_up, end_down, end_left, end_right;
        private Image<Rgba32> dir_arrow_up, dir_arrow_down, dir_arrow_left, dir_arrow_right;
        private Image<Rgba32> arrow_up, arrow_down, arrow_left, arrow_right;
        private Image<Rgba32> bridge_hor, bridge_vert;

        public override void UpdateTiling(Schematic schem, TileData tile)
        {
            Point2? dir = point = tile.rotation switch
            {
                0 => new Point2(1, 0),
                1 => new Point2(0, 1),
                2 => new Point2(-1, 0),
                3 => new Point2(0, -1),
                _ => null,
            };
            
            if(dir == null) 
                return;

            for (int i = 1; i <= 4; i++)
            {
                var x = tile.x + point!.Value.X;
                var y = tile.y + point!.Value.Y;
                if((x >= 0 && x < schem.width) && (y >= 0 && y < schem.height))
                {
                    if (schem.Tiles[x, y]?.block?.name == BlockName)
                    {
                        return;
                    }
                }
                point += dir;
            }

            point = null;
        }

        private Point2? point = null;
        
        public override void PostRenderBlock(PixelAccessor<Rgba32> pixels, TileData tileData, Point renderPosition)
        {
            if (bridge_hor == null)
            {
                bridge_hor = Regions[$"{BlockName}-bridge"];
                bridge_vert = bridge_hor.Clone(x => x.Rotate(RotateMode.Rotate90));
                
                dir_arrow_right = Regions[$"{BlockName}-dir"];
                dir_arrow_up = dir_arrow_right.Clone(x => x.Rotate(RotateMode.Rotate90));
                dir_arrow_left = dir_arrow_right.Clone(x => x.Rotate(RotateMode.Rotate180));
                dir_arrow_down = dir_arrow_right.Clone(x => x.Rotate(RotateMode.Rotate270));
                
                arrow_right = Regions[$"{BlockName}-arrow"];
                arrow_up = arrow_right.Clone(x => x.Rotate(RotateMode.Rotate90));
                arrow_left = arrow_right.Clone(x => x.Rotate(RotateMode.Rotate180));
                arrow_down = arrow_right.Clone(x => x.Rotate(RotateMode.Rotate270));
            }
            
            switch (tileData.rotation)
            {
                case 0:
                    pixels.RenderTile(new Point(renderPosition.X, renderPosition.Y), dir_arrow_right, Point.Empty);
                    break;
                case 1:
                    pixels.RenderTile(new Point(renderPosition.X, renderPosition.Y), dir_arrow_down, Point.Empty);
                    break;
                case 2:
                    pixels.RenderTile(new Point(renderPosition.X, renderPosition.Y), dir_arrow_left, Point.Empty);
                    break;
                case 3:
                    pixels.RenderTile(new Point(renderPosition.X, renderPosition.Y), dir_arrow_up, Point.Empty);
                    break;
            }
            
            if(point == null)
                return;

            Image<Rgba32> /*start, end,*/ bridge, arrow;
            if (point.Value.Y == 0)
            {
                bridge = bridge_hor;
                arrow = point.Value.X > 0 ? arrow_right : arrow_left;
            }
            else
            {
                bridge = bridge_vert;
                arrow = point.Value.Y > 0 ? arrow_down : arrow_up;
            }

            int px = 0;
            int py = 0;
            
            //pixels.RenderTile(new Point(renderPosition.X, renderPosition.Y), start, Point.Empty, Bridge.BridgeAlpha);
            do
            {
                if (point.Value.X != 0)
                {
                    px += Math.Sign(point.Value.X);
                }
                else
                {
                    py += -Math.Sign(point.Value.Y);
                }

                if (point.Value.X == px && point.Value.Y == -py)
                {
                    //pixels.RenderTile(new Point(renderPosition.X + px, renderPosition.Y + py), end, Point.Empty, Bridge.BridgeAlpha);
                }
                else
                {
                    pixels.RenderTile(new Point(renderPosition.X + px, renderPosition.Y + py), bridge, Point.Empty, Bridge.BridgeAlpha);
                }


            } while (point.Value.X != px || point.Value.Y != -py);

          
            
            var arrowX = (point.Value.X / 2f) + renderPosition.X;
            var arrowY = -(point.Value.Y / 2f) + renderPosition.Y;
            
            pixels.RenderTileShifted(arrowX, arrowY, arrow, Point.Empty, Bridge.BridgeAlpha);
        }
}