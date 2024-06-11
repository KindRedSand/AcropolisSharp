using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using static System.Reflection.Metadata.BlobBuilder;

namespace Playground.Mindustry.Blocks
{
    public static partial class Items
    {
        private const string _items_java =
            "https://raw.githubusercontent.com/Anuken/Mindustry/master/core/src/mindustry/content/Items.java";
        private const string _liquids_java =
            "https://raw.githubusercontent.com/Anuken/Mindustry/master/core/src/mindustry/content/Liquids.java";
        
        public static Dictionary<short, Item> itemsById = new();
        public static Dictionary<string, Item> items = new();
        
        public static Dictionary<short, Item> liquidsById = new();
        public static Dictionary<string, Item> liquids = new();

        public static readonly List<string> serpuloItems = new();
        public static readonly List<string> erekirItems = new();
        public static readonly List<string> erekirOnlyItems = new();

        public static void Load()
        {
            LoadItems();
            LoadLiquids();
        }
        
        private static void LoadItems()
        {
            if (!File.Exists("Items.java"))
            {
                _downloadItems_java()
                    .GetAwaiter().GetResult();
            }

            var str = File.ReadAllText("Items.java");

            var matches = GetItemsRegex().Matches(str);
            if (matches.Count == 0)
            {
                throw new Exception("Fuck this regex");
            }
            if (matches == null)
            {
                throw new Exception("Failed to get data");
            }
            
            short id = 0;
            foreach (Match match in matches)
            {
                var item = new Item()
                {
                    ItemId = id,
                    ItemName = match.Groups["name"].Value,
                    Color = Color.Parse(match.Groups["color"].Value),
                };

                items.Add(item.ItemName, item);
                itemsById.Add(id, item);

                id++;
            }

            var itemMatch = GetSerpuloItemsRegex().Match(str);
            if (!itemMatch.Success)
            {
                throw new Exception("Fuck this regex (serpulo)");
            }
            if (itemMatch == null)
            {
                throw new Exception("Failed to get serpulo items data");
            }
            
            serpuloItems.AddRange(Mindustrilize(itemMatch.Groups["items"].Value));

            itemMatch = GetErekirItemsRegex().Match(str);
            if (!itemMatch.Success)
            {
                throw new Exception("Fuck this regex (erekir)");
            }
            if (itemMatch == null)
            {
                throw new Exception("Failed to get erekir items data");
            }
            
            erekirItems.AddRange(Mindustrilize(itemMatch.Groups["items"].Value));
        }


        private static void LoadLiquids()
        {
            if (!File.Exists("Liquids.java"))
            {
                _downloadLiquids_java()
                    .GetAwaiter().GetResult();
            }

            var str = File.ReadAllText("Liquids.java");

            var matches = GetLiquidsRegex().Matches(str);
            if (matches.Count == 0)
            {
                throw new Exception("Fuck this regex");
            }
            if (matches == null)
            {
                throw new Exception("Failed to get data");
            }
            
            short id = 0;
            foreach (Match match in matches)
            {
                var item = new Item()
                {
                    ItemId = id,
                    ItemName = match.Groups["name"].Value,
                    Color = Color.Parse(match.Groups["color"].Value),
                };

                liquids.Add(item.ItemName, item);
                liquidsById.Add(id, item);

                id++;
            }
        }
        
        public static IEnumerable<string> Mindustrilize(string input) =>
            input.Replace("\n", "").Replace(" ", "").Split(',')
                .Select(x =>
                {
                    var sb = new StringBuilder();
                    foreach (var c in x)
                    {
                        if (char.IsUpper(c))
                        {
                            sb.Append('-');
                            sb.Append(char.ToLower(c));
                        }
                        else
                            sb.Append(c);

                    }

                    return sb.ToString();
                });

        [GeneratedRegex("""(new Item\("(?<name>.*)", ?Color\.valueOf\("(?<color>.*)"\)\){{)""", RegexOptions.ExplicitCapture | RegexOptions.Multiline)]
        private static partial Regex GetItemsRegex();
        
        [GeneratedRegex("""serpuloItems\.addAll\((?<items>(.|\n)*?)\);""", RegexOptions.ExplicitCapture | RegexOptions.Multiline)]
        private static partial Regex GetSerpuloItemsRegex();
        
        [GeneratedRegex("""erekirItems\.addAll\((?<items>(.|\n)*?)\);""", RegexOptions.ExplicitCapture | RegexOptions.Multiline)]
        private static partial Regex GetErekirItemsRegex();

        [GeneratedRegex("""new (?<type>.*?)\("(?<name>.*?)", ?Color\.valueOf\("(?<color>.*?)"\)\){{""", RegexOptions.ExplicitCapture | RegexOptions.Multiline)]
        private static partial Regex GetLiquidsRegex();
        
        
        private static async Task _downloadItems_java()
        {
            Console.WriteLine("Items.java was not found. Downloading new from:");
            Console.WriteLine(_items_java);
            var http = new HttpClient();
            var req = await http.GetAsync(_items_java, HttpCompletionOption.ResponseHeadersRead);
            if (!req.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to get Items.java file from github! Status code is {req.StatusCode}");
                throw new HttpRequestException(req.ReasonPhrase);
            }
            
            await File.WriteAllTextAsync("Items.java", await req.Content.ReadAsStringAsync());
        }
        
        private static async Task _downloadLiquids_java()
        {
            Console.WriteLine("Liquids.java was not found. Downloading new from:");
            Console.WriteLine(_liquids_java);
            var http = new HttpClient();
            var req = await http.GetAsync(_liquids_java, HttpCompletionOption.ResponseHeadersRead);
            if (!req.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to get Liquids.java file from github! Status code is {req.StatusCode}");
                throw new HttpRequestException(req.ReasonPhrase);
            }
            
            await File.WriteAllTextAsync("Liquids.java", await req.Content.ReadAsStringAsync());
        }
    }
}
