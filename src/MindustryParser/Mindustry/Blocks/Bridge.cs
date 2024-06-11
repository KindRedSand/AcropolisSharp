using Playground.Extensions;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing;

namespace Playground.Mindustry.Blocks
{
    public class Bridge : Block
    {
        public const byte BridgeAlpha = 200;
        public const float BridgeAlphaf = 0.75f;

        private Image<Rgba32> end_up, end_down, end_left, end_right;
        private Image<Rgba32> arrow_up, arrow_down, arrow_left, arrow_right;
        private Image<Rgba32> bridge_hor, bridge_vert;

        public  void PostRenderBlock(Image<Rgba32> pixels, TileData tileData, Point renderPosition)
        {
            pixels.Mutate(x =>
            {
                var point = tileData.config != null ? ((Point2)tileData.config) : new Point2(255, 255);
                if (Math.Abs(point.X) > 32 || Math.Abs(point.Y) > 32 || (point.X != 0 && point.Y != 0))
                    return;
                if (bridge_hor == null || end_down == null)
                {
                    bridge_hor = Regions[$"{BlockName}-bridge"];
                    bridge_vert = bridge_hor.Clone(x => x.Rotate(RotateMode.Rotate90));

                    end_down = Regions[$"{BlockName}-end"];
                    end_right = end_down.Clone(x => x.Rotate(RotateMode.Rotate90));
                    end_up = end_down.Clone(x => x.Rotate(RotateMode.Rotate180));
                    end_left = end_down.Clone(x => x.Rotate(RotateMode.Rotate270));

                    arrow_right = Regions[$"{BlockName}-arrow"];
                    arrow_up = arrow_right.Clone(x => x.Rotate(RotateMode.Rotate90));
                    arrow_left = arrow_right.Clone(x => x.Rotate(RotateMode.Rotate180));
                    arrow_down = arrow_right.Clone(x => x.Rotate(RotateMode.Rotate270));
                }

                Image<Rgba32> start, end, bridge, arrow;
                if (point.Y == 0)
                {
                    bridge = bridge_hor;
                    if (point.X > 0)
                    {
                        start = end_left;
                        end = end_right;
                        arrow = arrow_right;
                    }
                    else
                    {
                        start = end_right;
                        end = end_left;
                        arrow = arrow_left;
                    }
                }
                else
                {
                    bridge = bridge_vert;
                    if (point.Y > 0)
                    {
                        start = end_up;
                        end = end_down;
                        arrow = arrow_down;
                    }
                    else
                    {
                        start = end_down;
                        end = end_up;
                        arrow = arrow_up;
                    }
                }

                int px = 0;
                int py = 0;

                x.DrawImage(start, new Rectangle(renderPosition.X * 32, renderPosition.Y * 32, 32, 32), BridgeAlphaf);//(new Point(renderPosition.X, renderPosition.Y), start, Point.Empty, BridgeAlpha);
                do
                {
                    if (point.X != 0)
                    {
                        px += Math.Sign(point.X);
                    }
                    else
                    {
                        py += -Math.Sign(point.Y);
                    }

                    if (point.X == px && point.Y == -py)
                    {
                        x.DrawImage(end, new Rectangle((renderPosition.X + px) * 32, (renderPosition.Y + py) * 32, 32, 32), BridgeAlphaf);
                    }
                    else
                    {
                        x.DrawImage(bridge, new Rectangle((renderPosition.X + px) * 32, (renderPosition.Y + py) * 32, 32, 32), BridgeAlphaf);
                    }


                } while (point.X != px || point.Y != -py);

                var arrowX = (point.X / 2f) + renderPosition.X;
                var arrowY = -(point.Y / 2f) + renderPosition.Y;

                x.DrawImage(arrow, new Rectangle((int)((arrowX) * 32), (int)((arrowY) * 32), 32, 32), BridgeAlphaf);
            });
        }

        public override void PostRenderBlock(PixelAccessor<Rgba32> pixels, TileData tileData, Point renderPosition)
        {
            var point = tileData.config != null ? ((Point2)tileData.config) : new Point2(255, 255);
            if (Math.Abs(point.X) > 32 || Math.Abs(point.Y) > 32 || (point.X != 0 && point.Y != 0))
                return;
            if(bridge_hor == null || end_down == null)
            {
                bridge_hor = Regions[$"{BlockName}-bridge"];
                bridge_vert = bridge_hor.Clone(x => x.Rotate(RotateMode.Rotate90));

                end_down = Regions[$"{BlockName}-end"];
                end_right = end_down.Clone(x => x.Rotate(RotateMode.Rotate90));
                end_up = end_down.Clone(x => x.Rotate(RotateMode.Rotate180));
                end_left = end_down.Clone(x => x.Rotate(RotateMode.Rotate270));

                arrow_right = Regions[$"{BlockName}-arrow"];
                arrow_up = arrow_right.Clone(x => x.Rotate(RotateMode.Rotate90));
                arrow_left = arrow_right.Clone(x => x.Rotate(RotateMode.Rotate180));
                arrow_down = arrow_right.Clone(x => x.Rotate(RotateMode.Rotate270));
            }

            Image<Rgba32> start, end, bridge, arrow;
            if (point.Y == 0)
            {
                bridge = bridge_hor;
                if (point.X > 0)
                {
                    start = end_left;
                    end = end_right;
                    arrow = arrow_right;
                }
                else
                {
                    start = end_right;
                    end = end_left;
                    arrow = arrow_left;
                }
            }
            else
            {
                bridge = bridge_vert;
                if (point.Y > 0)
                {
                    start = end_up;
                    end = end_down;
                    arrow = arrow_down;
                }
                else
                {
                    start = end_down;
                    end = end_up;
                    arrow = arrow_up;
                }
            }

            int px = 0;
            int py = 0;

            pixels.RenderTile(new Point(renderPosition.X, renderPosition.Y), start, Point.Empty, BridgeAlpha);
            do
            {
                if (point.X != 0)
                {
                    px += Math.Sign(point.X);
                }
                else
                {
                    py += -Math.Sign(point.Y);
                }

                if (point.X == px && point.Y == -py)
                {
                    pixels.RenderTile(new Point(renderPosition.X + px, renderPosition.Y + py), end, Point.Empty, BridgeAlpha);
                }
                else
                {
                    pixels.RenderTile(new Point(renderPosition.X + px, renderPosition.Y + py), bridge, Point.Empty, BridgeAlpha);
                }


            } while (point.X != px || point.Y != -py);

            var arrowX = (point.X / 2f) + renderPosition.X;
            var arrowY = -(point.Y / 2f) + renderPosition.Y;

            pixels.RenderTileShifted(arrowX, arrowY, arrow, Point.Empty, BridgeAlpha);
        }
    }
}
