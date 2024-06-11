using System.Collections.Immutable;
using JavaStream;
using Playground.Extensions;
using Playground.Mindustry.Blocks.Build;
using Playground.Mindustry.Maps;
using Playground.Resource;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace Playground.Mindustry.Blocks
{
    public class Block
    {
        public const int TileSize = 32;

        protected static readonly Func<TileData, TileData, bool> DefaultPayloads;
        protected static readonly Func<TileData, bool> DefaultPayloadsOmnidir;

        protected static readonly Func<TileData, bool> DefaultDistributionOmnidir;

        protected static readonly Image<Rgba32> Shadow;

        public Rgba32 AverageColor { get; set; } = new Rgba32(0, 0, 0, 0);
        
        static Block()
        {
            DefaultPayloads = (_me, _tile) =>
            {
                var b = Blocks.blocks[_tile.block.name];

                return b.BlockType is "PayloadConveyor" or "PayloadRouter" or "PayloadMassDriver" or "Reconstructor"
                    or "UnitFactory" or "UnitAssembler" or "Constructor" or "PayloadUnloader" or "PayloadLoader";
            };
            DefaultPayloadsOmnidir = (_tile) =>
            {
                var b = Blocks.blocks[_tile.block.name];

                return b.BlockType is "PayloadRouter";
            };
            DefaultDistributionOmnidir = (_tile) =>
            {
                var b = Blocks.blocks[_tile.block.name];
                return b.BlockType is "Junction" or "BufferedItemBridge" or "ItemBridge" or "Sorter" or
                    "Router" or "OverflowGate" or "MassDriver" or "Unloader" or 
                    "Duct" or "DuctRouter" or "OverflowDuct";
            };
            
            var path = DllResource.GetAvailableResources()
                .First(x => Path.GetFileNameWithoutExtension(x) == "square-shadow-64");
            var img = LoadImage(path);
            img.Modulate(Color.Black.WithAlpha(0.5f));
            Shadow = img;
        }

        public string BlockName { get; init; } = "oh-no";
        public string BlockType { get; init; } = "oh-no";

        public short BlockId { get; init; }

        public bool DarkRegion { get; init; }

        public short Size;
        public ItemPrice[] Price = [];

        public Dictionary<string, Image<Rgba32>> Regions = new Dictionary<string, Image<Rgba32>>();

        public Color shardedColor = Color.Parse("ffd37f");

        public virtual void LoadRegions()
        {
            string[] list = DllResource.GetAvailableResources()
                .Where(x => Path.GetFileNameWithoutExtension(x).StartsWith(BlockName)).ToArray();

            foreach (string path in list)
            {
                string filename = Path.GetFileNameWithoutExtension(path);

                if(!BlockName.Contains("-large") && filename.Contains("-large"))
                    continue;
                if (!BlockName.Contains("-huge") && filename.Contains("-huge"))
                    continue;
                if (!BlockName.Contains("-gigantic") && filename.Contains("-gigantic"))
                    continue;
                if (!BlockName.Contains("-vent") && filename.Contains("-vent"))
                    continue;
                if (!BlockName.Contains("-wall") && filename.Contains("-wall"))
                    continue;

                if (!Regions.ContainsKey(filename))
                {
     
                    Regions.Add(filename, LoadImage(path));
                    if (filename.EndsWith("-team"))
                    {
                        //Regions[filename].Modulate(shardedColor);
                        Regions.Add($"{filename}-sharded", Regions[filename].Clone());
                        Regions[$"{filename}-sharded"].Modulate(shardedColor);
                    }
                }
            }
        }


        public virtual void RenderBlock(PixelAccessor<Rgba32> pixels, TileData tileData, Point renderPosition)
        {
            var imgs = GetRenderSprites();

            var tileOffset = getTileOffset();
            foreach (var img in imgs)
            {
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

        public virtual void RenderShadow(PixelAccessor<Rgba32> pixels, TileData tileData, Point renderPosition)
        {
            var tileOffset = getTileOffset();
            for (int j = -1; j < Size + 1; j++)
            {
                int tiley;
                if (j == -1)
                    tiley = 2;
                else if (j == Size)
                    tiley = 0;
                else
                    tiley = 1;

                for (int i = -1; i < Size + 1; i++)
                {
                    int tilex;
                    if (i == -1)
                        tilex = 0;
                    else if (i == Size)
                        tilex = 2;
                    else
                        tilex = 1;
                    
                    
                    pixels.RenderTile(new Point(renderPosition.X + i - tileOffset,
                        renderPosition.Y - j + tileOffset), Shadow, new Point(tilex, tiley));
                }
            }
        }
        
        
        public virtual void PostRenderBlock(PixelAccessor<Rgba32> pixels, TileData tileData, Point renderPosition)
        {
        }

        public virtual void PostRenderBlockDirect(Image<Rgba32> image, TileData tileData, Point renderPosition)
        {
        }


        public virtual void UpdateTiling(Schematic schem, TileData tile)
        {

        }

        public virtual IEnumerable<Image<Rgba32>> GetRenderSprites()
        {
            if (Regions.Count == 0)
            {
                using var str = DllResource.GetStream(
                    DllResource.GetAvailableResources()
                        .First(x => x.Contains("cat.png")))!;
                return  new []{ Image.Load<Rgba32>(str)};
            }
            var list = new List<Image<Rgba32>>();
            if(Regions.ContainsKey($"{BlockName}-bottom"))
                list.Add(Regions[$"{BlockName}-bottom"]);
            if (Regions.ContainsKey($"{BlockName}-weave"))
                list.Add(Regions[$"{BlockName}-weave"]);
            if (Regions.ContainsKey($"{BlockName}-base"))
                list.Add(Regions[$"{BlockName}-base"]);
            if (Regions.TryGetValue(BlockName, out var value))
                list.Add(value);
            if (Regions.ContainsKey($"{BlockName}-mid"))
                list.Add(Regions[$"{BlockName}-mid"]);
            if (Regions.ContainsKey($"{BlockName}-middle"))
                list.Add(Regions[$"{BlockName}-middle"]);
            if (Regions.ContainsKey($"{BlockName}-rotator"))
                list.Add(Regions[$"{BlockName}-rotator"]);
            if (Regions.ContainsKey($"{BlockName}-team"))
                list.Add(Regions[$"{BlockName}-team-sharded"]);
            if (BlockType is not ("OverdriveProjector") && Regions.ContainsKey($"{BlockName}-top"))
                list.Add(Regions[$"{BlockName}-top"]);
            if (Regions.ContainsKey($"{BlockName}-cap"))
                list.Add(Regions[$"{BlockName}-cap"]);

            return list.ToImmutableArray();
        }

        protected static Image<Rgba32> LoadImage(string path)
        {
            using var str = DllResource.GetStream(path);
            return Image.Load<Rgba32>(str);
        }

        public int getTileOffset()
        {
            switch (Size)
            {
                case 0:
                case 1:
                case 2:
                    break;
                case 3:
                case 4:
                    return 1;
                case 5:
                case 6:
                    return 2;
                case 7:
                case 8:
                    return 3;
                case 9:
                case 10:
                    return 4;
            }

            return 0;
        }


        private bool shouldLoad()
        {
            return BlockType is not ("Cliff" or "AirBlock" or "SpawnBlock" or "Floor" or
                "ShallowLiquid" or "StaticWall" or "SteamVent" or "StaticTree" or
                "Prop" or "Seaweed" or "SeaBush" or "TallBlock");
        }

        /// <summary>
        /// Return side if another <see cref="tile"/> are connected to <see cref="me"/> tile
        /// </summary>
        /// <param name="me">Base tile</param>
        /// <param name="tile">Tile for check</param>
        /// <param name="allowedFilter">Allowed tiles</param>
        /// <param name="ignoreRotationFilter">Tiles what are omnidirectional</param>
        /// <returns>Return side where tile are located or <see cref="Sides.None"/> if tile are not connected</returns>
        protected Sides isConnectable(TileData me, TileData tile, Func<TileData, TileData, bool> allowedFilter,
            Func<TileData, bool>? ignoreRotationFilter = null)
        {
            var b = Blocks.blocks[tile.block.name];

            if (allowedFilter.Invoke(me, tile))
            {
                int space = Size - 2;
                int halfSpace = (space - 1) / 2;
                int offset = (Size - 1) / 2 + (b.Size - 1) / 2 + 1;

                if (tile.x >= me.x - halfSpace && tile.x <= me.x + halfSpace)
                {
                    if (tile.y == me.y - offset)
                    {
                        if (ignoreRotationFilter?.Invoke(tile) ?? false)
                        {
                            return Sides.Down;
                        }
                        return tile.rotation == 1 ? Sides.Down : Sides.None;
                    }
                    if (tile.y == me.y + offset)
                    {
                        if (ignoreRotationFilter?.Invoke(tile) ?? false)
                        {
                            return Sides.Up;
                        }
                        return tile.rotation == 3 ? Sides.Up : Sides.None;
                    }
                }
                if (tile.y >= me.y - halfSpace && tile.y <= me.y + halfSpace)
                {
                    if (tile.x == me.x - offset)
                    {
                        if (ignoreRotationFilter?.Invoke(tile) ?? false)
                        {
                            return Sides.Left;
                        }
                        return tile.rotation == 0 ? Sides.Left : Sides.None;
                    }
                    if (tile.x == me.x + offset)
                    {
                        if (ignoreRotationFilter?.Invoke(tile) ?? false)
                        {
                            return Sides.Right;
                        }
                        return tile.rotation == 2 ? Sides.Right : Sides.None;
                    }
                }
            }
            return Sides.None;
        }
        
        
        protected static Point GetUniversalOffset(int size) => 
            size % 2 > 0 ? new Point(16, 16) : new Point(32, 0);



        public bool hasBuilding()
        {
            return BlockType is not ("AirBlock" or "Floor" or "Cliff" or "EmptyFloor" or
                "OreBlock" or "SteamVent" or "StaticWall" or
                "EmptyFloor" or "ShallowLiquid" or "SpawnBlock" or 
                "StaticTree" or "TreeBlock" or "Prop" or 
                "Seaweed" or "SeaBush" or "TallBlock" or
                "OverlayFloor");
        }

        public virtual Building? read(Tile tile, DataInput read, sbyte revision, int len)
        {
            Building? build = BlockType switch
            {
                _ => new SkipBuilding(),
                // "ConstructBlock" => new ConstructBlock(),
                // "LaunchPad" => new LaunchPad(),
                // "AutoDoor" or "Door" => new Door(),
                // "BaseShield" => new BaseShield(),
                // "BuildTurret" => new BuildTurret(),
                // "ForceProjector" or "DirectionalForceProjector" => new ForceProjector(),
                // "MendProjector" or "OverdriveProjector" => new MendProjector(),
                // "Radar" => new Radar(),
                // "ShieldWall" => new ShieldWall(),
                // "Turret" or "PowerTurret" or "LiquidTurret" or "LaserTurret" => new Build.Turret(),
                // "ItemTurret" => new ItemTurret(),
                // "ContinuousTurret" or "ContinuousLiquidTurret" => new ContinuousTurret(),
                // //"PayloadTurretBuild" => new PayloadAmmoTurret(),//Unused
                // "PointDefenseTurret" or "TractorBeamTurret" => new PointDefenseTurret(),
                // "ItemBridge" => new ItemBridge(),
                // "BufferedItemBridge" or "Junction" => new BufferedItemBridge(),
                // "Conveyor" or "ArmoredConveyor" => new Build.Conveyor(),
                // "Unloader" => new Build.Unloader(),
                // "DirectionalUnloader" => new DirectionalUnloader(),
                // "Duct" => new Duct(),
                // "MassDriver" => new MassDriver(),
                // "OverflowGate" => new OverflowGate(),
                // "Sorter" => new Build.Sorter(),
                // "DuctRouter" => new DuctRouter(),
                // "StackConveyor" => new Build.StackConveyor(),
                // "GenericCrafter" => new Build.GenericCrafter(),
                // "HeatProducer" => new HeatProducer(),
                // "LegacyCommandCenter" => new Command(),
                // "LegacyMechPad" => new LegacyMechPad(),
                // "LegacyUnitFactory" => new LegacyUnitFactory(),
                // "Canvas" => new Canvas(),
                // //"LogicBlock" => new LogicBlock(BlockName.Contains("world")),
                // //"LogicDisplay" => new LogicDisplay(),
                // "MemoryBlock" => new MemoryBlock(),
                // "MessageBlock" => new MessageBlock(),
                // "PayloadBlock" => new PayloadBlock(),
                // "Constructor" => new Build.Constructor(),
                // "PayloadConveyor" => new Build.PayloadConveyor(),
                // "PayloadDeconstructor" => new PayloadDeconstructor(),
                // "PayloadLoader" => new Build.PayloadLoader(),
                // "PayloadMassDriver" => new Build.PayloadMassDriver(),
                // "PayloadRouter" => new Build.PayloadRouter(),
                // "PayloadSource" => new PayloadSource(),
                // "PowerGenerator" or "ConsumeGenerator" or "SolarGenerator" or "ThermalGenerator" => new PowerGenerator(),
                // "HeaterGenerator" or "ImpactReactor" or "NuclearReactor" => new HeaterGenerator(),
                // "Light" => new Light(),
                // "VariableReactor" => new VariableReactor(),
                // "BeamDrill" or "Drill" => new Build.GenericCrafter(),
                // "ItemSource" => new ItemSource(),
                // "LiquidSource" => new LiquidSource(),
                // "DroneCenter" => new DroneCenter(),
                // "Separator" => new Separator(),
                // "Reconstructor" => new Build.Reconstructor(),
                // "RepairTurret" => new RepairTurret(),
                // "UnitAssembler" => new UnitAssembler(),
                // "UnitCargoLoader" => new UnitCargoLoader(),
                // "UnitCargoUnloadPoint" => new UnitCargoUnloadPoint(),
                // "UnitFactory" => new Build.UnitFactory(),
            };
            
            if(build != null)
                build?.readAll(read, revision, len);
            else
            {
                read.Skip(len);
            }
            return build;
        }

        public bool _hasBuilding()
        {
            return BlockType switch
            {
                "ConstructBlock" => true,
                "LaunchPad" =>true,
                "AutoDoor" or "Door" => true,
                "BaseShield" =>true,
                "BuildTurret" =>true,
                "ForceProjector" or "DirectionalForceProjector" => true,
                "MendProjector" or "OverdriveProjector" => true,
                "Radar" =>true,
                "ShieldWall" => true,
                "Turret" or "PowerTurret" or "LiquidTurret" or "LaserTurret" => true,
                "ItemTurret" => true,
                "ContinuousTurret" or "ContinuousLiquidTurret" => true,
                //"PayloadTurretBuild" => new PayloadAmmoTurret(),//Unused
                "PointDefenseTurret" or "TractorBeamTurret" => true,
                "ItemBridge" => true,
                "BufferedItemBridge" or "Junction" => true,
                "Conveyor" or "ArmoredConveyor" => true,
                "Unloader" =>true,
                "DirectionalUnloader" => true,
                "Duct" => true,
                "MassDriver" => true,
                "OverflowGate" => true,
                "Sorter" => true,
                "DuctRouter" => true,
                "StackConveyor" => true,
                "GenericCrafter" => true,
                "HeatProducer" => true,
                "LegacyCommandCenter" => true,
                "LegacyMechPad" => true,
                "LegacyUnitFactory" => true,
                "Canvas" => true,
                "LogicBlock" => true,
                "LogicDisplay" => true,
                "MemoryBlock" => true,
                "MessageBlock" => true,
                //"PayloadBlock" =>true,
                //"Constructor" => true,
                //"PayloadConveyor" => true,
                //"PayloadDeconstructor" => true,
                //"PayloadLoader" => true,
                //"PayloadMassDriver" => true,
                //"PayloadRouter" => true,
                //"PayloadSource" => true,
                "PowerGenerator" or "ConsumeGenerator" or "SolarGenerator" or "ThermalGenerator" => true,
                "HeaterGenerator" or "ImpactReactor" or "NuclearReactor" => true,
                "Light" => true,
                "VariableReactor" => true,
                "BeamDrill" or "Drill" => true,
                "ItemSource" => true,
                "LiquidSource" => true,
                //"DroneCenter" => true,
                "Separator" => true,
                //"Reconstructor" => true,
                "RepairTurret" => true,
                //"UnitAssembler" => true,
                //"UnitCargoLoader" => true,
                //"UnitCargoUnloadPoint" => true,
                //"UnitFactory" => true,
                _ => false,
            };
        }

        [Flags]
        public enum Sides
        {
            None = 0,
            Left = 1 << 0,
            Right = 1 << 1,
            Back = 1 << 2,

            Up = 10,
            Down = 11,
        }
    }
}
/*

var web = new WebClient();
using var st= web.OpenRead(
    "https://raw.githubusercontent.com/Anuken/Mindustry/master/core/src/mindustry/content/Blocks.java");
using var sr = new StreamReader(st);


var str = sr.ReadToEnd();
*/