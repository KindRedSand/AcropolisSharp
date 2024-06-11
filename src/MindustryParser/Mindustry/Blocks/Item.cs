using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;

namespace Playground.Mindustry.Blocks
{
    public class Item
    {
        public string ItemName { get; init; }
        public short ItemId { get; set; }
        public Color Color { get; set; }
    }

    public class ItemStack
    {
        public Item Item { get; set; }
        public int Count { get; set; }

        public ItemStack(Item item, int count)
        {
            Item = item;
            Count = count;
        }
    }
}
