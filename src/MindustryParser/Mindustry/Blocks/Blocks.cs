using System.Net;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace Playground.Mindustry.Blocks
{
    public static partial class Blocks
    {
        private const string _blocks_java =
            "https://raw.githubusercontent.com/Anuken/Mindustry/master/core/src/mindustry/content/Blocks.java";
        
        public static readonly Dictionary<short, Block> BlocksById = new();
        public static readonly Dictionary<string, Block> blocks = new();
        public static void Load()
        {
            if (!File.Exists("Blocks.java"))
            {
                _downloadBlocks_java()
                    .GetAwaiter().GetResult();
            }
            
            short id = 0;
            var _block = new Block()
            {
                BlockId = id,
                BlockName = "air",
                BlockType = "AirBlock",
                Size = 1,
            };
            blocks["air"] = _block;
            BlocksById[id++] = _block;

            _block = new Block()
            {
                BlockId = id,
                BlockName = "spawn",
                BlockType = "SpawnBlock",
                Size = 1,
            };
            blocks["spawn"] = _block;
            BlocksById[id++] = _block;
            
            _block = new Block()
            {
                BlockId = id,
                BlockName = "cliff",
                BlockType = "Cliff",
                Size = 1,
            };
            blocks["cliff"] = _block;
            BlocksById[id++] = _block;
            
            for (short i = 1; i <= 16; i++)
            {
                _block = new Block()
                {
                    BlockId = id,
                    BlockName = $"build{i}",
                    BlockType = "ConstructBlock",
                    Size = i,
                };
                blocks[$"build{i}"] = _block;
                BlocksById[id++] = _block;
            }
            
            var str = File.ReadAllText("Blocks.java");

            var matches = GetBlocksRegex().Matches(str);
            if (matches.Count == 0)
            {
                throw new Exception("Fuck this regex");
            }
            if (matches == null)
            {
                throw new Exception("Failed to get data");
            }
            
            foreach (Match match in matches)
            {
                if(match.Groups["type"].Value is "AirBlock" or "SpawnBlock" or "Cliff" or "RemoveWall" or "RemoveOre" //Skip this one since we already added them to list
                   or "DirectionalForceProjector")//Disabled 
                    continue;
                //var data = GetDataRegex().Match(match.Value);
                var dark = match.Value.Contains("new DrawTurret(\"reinforced-\")") ||
                           match.Value.Contains("\"-dark\"");
                var sizeMatch = GetSizeRegex().Match(match.Value);
                var ingMatch = GetIngRegex().Match(match.Value);

                var recMatches = RecipeRegex.Matches(ingMatch.Value);
                var ing = new List<ItemPrice>();
                foreach (Match match2 in recMatches)
                {
                    ing.Add( new ItemPrice(match2.Groups["item"].Value, int.Parse(match2.Groups["count"].Value)));
                }
                
                if (match.Groups["type"].Value == "GenericCrafter" && ing.Count == 0)
                {
                    throw new Exception("Empty ingridients where this can't be");
                }

                Block? b = null;
                switch (match.Groups["type"].Value)
                {
                    case "StaticWall" or "Floor" or "ShallowLiquid" or "EmptyFloor":
                        b = new EnvironmentTile()
                        {
                            BlockId = id,
                            BlockName = match.Groups["name"].Value,
                            BlockType = match.Groups["type"].Value,
                            Size = sizeMatch.Success ? short.Parse(sizeMatch.Groups["size"].Value) : (short) 1,
                        };
                        break;
                    case "SteamVent":
                        b = new EnvironmentTile()
                        {
                            BlockId = id,
                            BlockName = match.Groups["name"].Value,
                            BlockType = match.Groups["type"].Value,
                            Size = 3,
                        };
                        break;
                    case "Conveyor" or "ArmoredConveyor":
                        b = new Conveyor()
                        {
                            BlockId = id,
                            BlockName = match.Groups["name"].Value,
                            BlockType = match.Groups["type"].Value,
                            Size = sizeMatch.Success ? short.Parse(sizeMatch.Groups["size"].Value) : (short) 1,
                            Price = ing.ToArray(),
                            DarkRegion = dark,
                        };
                        break;
                    case "StackConveyor":
                        b = new StackConveyor()
                        {
                            BlockId = id,
                            BlockName = match.Groups["name"].Value,
                            BlockType = match.Groups["type"].Value,
                            Size = sizeMatch.Success ? short.Parse(sizeMatch.Groups["size"].Value) : (short)1,
                            Price = ing.ToArray(),
                            DarkRegion = dark,
                        };
                        break;
                    case "PayloadConveyor":
                        b = new PayloadConveyor()
                        {
                            BlockId = id,
                            BlockName = match.Groups["name"].Value,
                            BlockType = match.Groups["type"].Value,
                            Size = sizeMatch.Success ? short.Parse(sizeMatch.Groups["size"].Value) : (short)1,
                            Price = ing.ToArray(),
                            DarkRegion = dark,
                        };
                        break;
                    case "PayloadRouter":
                        b = new PayloadRouter()
                        {
                            BlockId = id,
                            BlockName = match.Groups["name"].Value,
                            BlockType = match.Groups["type"].Value,
                            Size = sizeMatch.Success ? short.Parse(sizeMatch.Groups["size"].Value) : (short)1,
                            Price = ing.ToArray(),
                            DarkRegion = dark,
                        };
                        break;
                    case "HeatProducer" or "HeaterGenerator" or "HeatConductor":
                        b = new HeatProducer()
                        {
                            BlockId = id,
                            BlockName = match.Groups["name"].Value,
                            BlockType = match.Groups["type"].Value,
                            Size = sizeMatch.Success ? short.Parse(sizeMatch.Groups["size"].Value) : (short)1,
                            Price = ing.ToArray(),
                            DarkRegion = dark,
                        };
                        break;
                    case "Reconstructor" or "UnitAssembler":
                        b = new Reconstructor()
                        {
                            BlockId = id,
                            BlockName = match.Groups["name"].Value,
                            BlockType = match.Groups["type"].Value,
                            Size = sizeMatch.Success ? short.Parse(sizeMatch.Groups["size"].Value) : (short)1,
                            Price = ing.ToArray(),
                            DarkRegion = dark,
                        };
                        break;
                    case "UnitFactory":
                        b = new UnitFactory()
                        {
                            BlockId = id,
                            BlockName = match.Groups["name"].Value,
                            BlockType = match.Groups["type"].Value,
                            Size = sizeMatch.Success ? short.Parse(sizeMatch.Groups["size"].Value) : (short)1,
                            Price = ing.ToArray(),
                            DarkRegion = dark,
                        };
                        break;
                    case "PayloadLoader" or "PayloadUnloader":
                        b = new PayloadLoader()
                        {
                            BlockId = id,
                            BlockName = match.Groups["name"].Value,
                            BlockType = match.Groups["type"].Value,
                            Size = sizeMatch.Success ? short.Parse(sizeMatch.Groups["size"].Value) : (short)1,
                            Price = ing.ToArray(),
                            DarkRegion = dark,
                        };
                        break;
                    case "PayloadMassDriver":
                        b = new PayloadMassDriver()
                        {
                            BlockId = id,
                            BlockName = match.Groups["name"].Value,
                            BlockType = match.Groups["type"].Value,
                            Size = sizeMatch.Success ? short.Parse(sizeMatch.Groups["size"].Value) : (short)1,
                            Price = ing.ToArray(),
                            DarkRegion = dark,
                        };
                        break;
                    case "Constructor":
                        b = new Constructor()
                        {
                            BlockId = id,
                            BlockName = match.Groups["name"].Value,
                            BlockType = match.Groups["type"].Value,
                            Size = sizeMatch.Success ? short.Parse(sizeMatch.Groups["size"].Value) : (short)1,
                            Price = ing.ToArray(),
                            DarkRegion = dark,
                        };
                        break;
                    case "PayloadDeconstructor":
                        b = new Deconstructor()
                        {
                            BlockId = id,
                            BlockName = match.Groups["name"].Value,
                            BlockType = match.Groups["type"].Value,
                            Size = sizeMatch.Success ? short.Parse(sizeMatch.Groups["size"].Value) : (short)1,
                            Price = ing.ToArray(),
                            DarkRegion = dark,
                        };
                        break;
                    case "Unloader":
                        b = new Unloader()
                        {
                            BlockId = id,
                            BlockName = match.Groups["name"].Value,
                            BlockType = match.Groups["type"].Value,
                            Size = sizeMatch.Success ? short.Parse(sizeMatch.Groups["size"].Value) : (short)1,
                            Price = ing.ToArray(),
                            DarkRegion = dark,
                        };
                        break;
                    case "Conduit" or "ArmoredConduit":
                        b = new Conduit()
                        {
                            BlockId = id,
                            BlockName = match.Groups["name"].Value,
                            BlockType = match.Groups["type"].Value,
                            Size = sizeMatch.Success ? short.Parse(sizeMatch.Groups["size"].Value) : (short)1,
                            Price = ing.ToArray(),
                            DarkRegion = dark,
                        };
                        break;
                    case "Duct" or "DuctConduit":
                        b = new Duct()
                        {
                            BlockId = id,
                            BlockName = match.Groups["name"].Value,
                            BlockType = match.Groups["type"].Value,
                            Size = sizeMatch.Success ? short.Parse(sizeMatch.Groups["size"].Value) : (short)1,
                            Price = ing.ToArray(),
                            DarkRegion = dark,
                        };
                        break;
                    case "DuctRouter" or "DirectionalUnloader" or "OverflowDuct" or "StackRouter"
                        or "PowerDiode" or "BeamDrill"://Tbh this is hack, but it works
                        b = new DuctRouter()
                        {
                            BlockId = id,
                            BlockName = match.Groups["name"].Value,
                            BlockType = match.Groups["type"].Value,
                            Size = sizeMatch.Success ? short.Parse(sizeMatch.Groups["size"].Value) : (short)1,
                            Price = ing.ToArray(),
                            DarkRegion = dark,
                        };
                        break;
                    case "DuctBridge" or "DirectionLiquidBridge":
                        b = new DuctBridge()
                        {
                            BlockId = id,
                            BlockName = match.Groups["name"].Value,
                            BlockType = match.Groups["type"].Value,
                            Size = sizeMatch.Success ? short.Parse(sizeMatch.Groups["size"].Value) : (short)1,
                            Price = ing.ToArray(),
                            DarkRegion = dark,
                        };
                        break;
                    case "Sorter" or "ItemSource" or "LiquidSource":
                        b = new Sorter()
                        {
                            BlockId = id,
                            BlockName = match.Groups["name"].Value,
                            BlockType = match.Groups["type"].Value,
                            Size = sizeMatch.Success ? short.Parse(sizeMatch.Groups["size"].Value) : (short)1,
                            Price = ing.ToArray(),
                            DarkRegion = dark,
                        };
                        break;
                    case "PowerNode" or "LongPowerNode":
                        b = new PowerNode()
                        {
                            BlockId = id,
                            BlockName = match.Groups["name"].Value,
                            BlockType = match.Groups["type"].Value,
                            Size = sizeMatch.Success ? short.Parse(sizeMatch.Groups["size"].Value) : (short)1,
                            Price = ing.ToArray(),
                            DarkRegion = dark,
                        };
                        break;
                    case "OreBlock":
                        b = new Block()
                        {
                            BlockId = id,
                            BlockName = match.Groups["name"].Success ? match.Groups["name"].Value : $"ore-{match.Groups["ore"].Value}",
                            BlockType = match.Groups["type"].Value,
                            Size = sizeMatch.Success ? short.Parse(sizeMatch.Groups["size"].Value) : (short) 1,
                            Price = ing.ToArray(),
                            DarkRegion = dark,
                        };
                        break;
                    default:
                    {
                        if (match.Groups["type"].Value.Contains("Turret"))
                        {
                            b = new Turret()
                            {
                                BlockId = id,
                                BlockName = match.Groups["name"].Value,
                                BlockType = match.Groups["type"].Value,
                                Size = sizeMatch.Success ? short.Parse(sizeMatch.Groups["size"].Value) : (short)1,
                                Price = ing.ToArray(),
                                DarkRegion = dark,
                            };
                        }
                        else if (match.Groups["type"].Value.Contains("Bridge"))
                        {
                            b = new Bridge()
                            {
                                BlockId = id,
                                BlockName = match.Groups["name"].Value,
                                BlockType = match.Groups["type"].Value,
                                Size = sizeMatch.Success ? short.Parse(sizeMatch.Groups["size"].Value) : (short)1,
                                Price = ing.ToArray(),
                                DarkRegion = dark,
                            };
                        }
                        else if (match.Groups["type"].Value is "SteamVent")
                        {
                            b = new SteamVent()
                            {
                                BlockId = id,
                                BlockName = match.Groups["name"].Value,
                                BlockType = match.Groups["type"].Value,
                            };
                        }
                        else if (match.Groups["type"].Value is not ("AirBlock" or "SpawnBlock" or "Cliff"))
                        {
                            b = new Block()
                            {
                                BlockId = id,
                                BlockName = match.Groups["name"].Value,
                                BlockType = match.Groups["type"].Value,
                                Size = sizeMatch.Success ? short.Parse(sizeMatch.Groups["size"].Value) : (short) 1,
                                Price = ing.ToArray(),
                                DarkRegion = dark,
                            };
                        }

                        break;
                    }
                }
                
                if(b != null)
                {
                    blocks.Add(b.BlockName, b);
                    BlocksById.Add(id, b);
                }

                id++;
            }
            
            //Idk wtf is this
            _block = new Block()
            {
                BlockId = id,
                BlockName = "removeWall",
                BlockType = "RemoveWall",
                Size = 1,
            };
            blocks["removeWall"] = _block;
            BlocksById[id++] = _block;
            
            
            _block = new Block()
            {
                BlockId = id,
                BlockName = "remove-ore",
                BlockType = "RemoveOre",
                Size = 1,
            };
            blocks["remove-ore"] = _block;
            BlocksById[id++] = _block;
            
            // var sb = new StringBuilder();
            foreach (var (_, block) in blocks)
            {
                block.LoadRegions();
                switch (block.BlockType)
                {
                    case "SteamVent":
                    {
                        using var txt = block.Regions.First().Value.Clone();
                        txt.Mutate(x => x
                            .Resize(new ResizeOptions() { Sampler = KnownResamplers.NearestNeighbor, Size = new Size(32, 0) })
                            .Quantize(new OctreeQuantizer(new QuantizerOptions()
                            {
                                Dither = null,
                                MaxColors = 1,
                            })));
                        //Make it little lighter
                        var blender = PixelOperations<Rgba32>.Instance.GetPixelBlender(PixelColorBlendingMode.Normal, PixelAlphaCompositionMode.SrcOver);
                        block.AverageColor = blender.Blend(txt[0, 0], new Rgba32(255,255,255), 0.2f); //new Rgba32(txt[0, 0].R + 20,txt[0, 0].G + 20,txt[0, 0].B + 20);
                        break;
                    }
                    case "StaticTree":
                    {
                        var t = block.Regions.First().Value;
                        var y = t.Height / 2 - 1;
                        block.AverageColor = t[t.Width / 2, t.Height / 2];
                        //Look upward for our pixel
                        while (y > 0 && (block.AverageColor.A == 0 || 
                                         block.AverageColor is {R: 0, G: 0, B: 0}))
                        {
                            block.AverageColor = t[t.Width / 2, y];
                            y--;
                        }

                        break;
                    }
                    default:
                    {
                        if (block.Regions.Count > 0)
                        {


                            using var txt = block.Regions.First().Value.Clone();
                            txt.Mutate(x => x
                                .Resize(new ResizeOptions()
                                    {Sampler = KnownResamplers.NearestNeighbor, Size = new Size(32, 0)})
                                .Quantize(new OctreeQuantizer(new QuantizerOptions()
                                {
                                    Dither = null,
                                    MaxColors = 1,
                                })));

                            block.AverageColor = txt[0, 0];
                        }

                        break;
                    }
                }
            }
        }


        // [GeneratedRegex("""(new (?<type>.*)\("(?<name>.*)"\)({{)(?=(.|\n)*requirements\(Category\..*, ?with\((?<ing>.*)\)\);)(?=(.|\n)*size ? = ?(?<size>\d*);)(.|\n)+?(}};))""", RegexOptions.ExplicitCapture | RegexOptions.Multiline)]
        // private static partial Regex GetBlocksRegexBad();
        //
        // [GeneratedRegex("""( new (?<type>.*)\("(?<name>.*)"\){{(.|\n)+?(^        }};))""", RegexOptions.ExplicitCapture | RegexOptions.Multiline)]
        // private static partial Regex OldGetBlocksRegex();
        
        [GeneratedRegex("""(new (?<type>.*)\((("(?<name>.*)"(, ?.*)?)|(Items\.(?<ore>.*)))\))({{(.|\n)+?(^        }};)|(;))""", RegexOptions.ExplicitCapture | RegexOptions.Multiline)]
        private static partial Regex GetBlocksRegex();
        
        // [GeneratedRegex("""((((.|\n)*requirements\(Category\..*, ?with\((?<ing>.*)(\)\);))|(.|\n)*)((.|\n)*size ? = ?(?<size>\d*);)|(.|\n)*)""", RegexOptions.ExplicitCapture | RegexOptions.Multiline | RegexOptions.NonBacktracking)]
        // private static partial Regex GetDataRegex();

        [GeneratedRegex("(size ? = ?(?<size>\\d*);)")]
        private static partial Regex GetSizeRegex();


        [GeneratedRegex("(requirements\\(Category\\..*, ?with\\((?<ing>.*)(\\)\\);))")]
        private static partial Regex GetIngRegex();


        // private static Regex BlocksRegex =
        //     new Regex(
        //         """(new (?<type>.*)\("(?<name>.*)"\){{(?=(.|\n)*requirements\(Category\..*, ?with\((?<ing>.*)\)\);)(?=(.|\n)*size ? = ?(?<size>\d*))(.|\n)+?(}};))""", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
        //
        private static readonly Regex RecipeRegex = 
            new Regex("""(Items\.(?<item>[a-zA-Z]*), ?(?<count>\d*))""");

        private static async Task _downloadBlocks_java()
        {
            Console.WriteLine("Blocks.java was not found. Downloading new from:");
            Console.WriteLine(_blocks_java);
            var http = new HttpClient();
            var req = await http.GetAsync(_blocks_java, HttpCompletionOption.ResponseHeadersRead);
            if (!req.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to get Blocks.java file from github! Status code is {req.StatusCode}");
                throw new HttpRequestException(req.ReasonPhrase);
            }
            
            await File.WriteAllTextAsync("Blocks.java", await req.Content.ReadAsStringAsync());
        }
    }
}
