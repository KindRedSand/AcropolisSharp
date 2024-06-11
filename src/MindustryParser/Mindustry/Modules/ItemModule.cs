using JavaStream;
using Playground.Mindustry.Blocks;

namespace Playground.Mindustry.Modules;

public class ItemModule
{
    public readonly int[] items = new int[Items.items.Count];
    protected int total = 0;

    public void write(DataOutput write)
    {
        int amount = 0;
        foreach(int item in items){
            if(item > 0) amount++;
        }

        write.WriteShort(amount);

        for(int i = 0; i < items.Length; i++){
            if(items[i] > 0){
                write.WriteShort(i); //item ID
                write.WriteInt(items[i]); //item amount
            }
        }
    }

    public int read(DataInput read, bool legacy = false)
    {
        var readed = 0;
        Array.Clear(items);
        readed += legacy ? 1 : 2;
        int count = legacy ? read.ReadUnsignedByte() : read.ReadShort();
        total = 0;

        for(int j = 0; j < count; j++){
            readed += legacy ? 1 : 2;
            short itemid = (short)(legacy ? read.ReadUnsignedByte() : read.ReadShort());
            readed += 4;
            int itemamount = read.ReadInt();
            Item item = Items.itemsById[itemid];
            if(item != null){
                items[itemid] = itemamount;
                total += itemamount;
            }
        }

        return readed;
    }
}