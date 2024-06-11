using JavaStream;
using Playground.Mindustry.Blocks;

namespace Playground.Mindustry.Modules;

public class LiquidModule
{
    public Item current = null;
    public float[] liquids = new float[Items.liquids.Count];
    
    public void write(DataOutput write){
        int amount = liquids.Count(liquid => liquid > 0);

        write.WriteShort(amount); //amount of liquids

        for(int i = 0; i < liquids.Length; i++){
            if(liquids[i] > 0){
                write.WriteShort(i); //liquid ID
                write.WriteFloat(liquids[i]); //liquid amount
            }
        }
    }
    
    
    public int read(DataInput read, bool legacy = false){
        Array.Clear(liquids);
        var readed = legacy ? 1 : 2;
        int count = legacy ? read.ReadUnsignedByte() : read.ReadShort();

        for(int j = 0; j < count; j++)
        {
            readed += legacy ? 1 : 2;
            var fluidid = (short) (legacy ? read.ReadUnsignedByte() : read.ReadShort());
            Item liq = Items.liquidsById[fluidid];
            readed += 4;
            float amount = read.ReadFloat();
            if(liq != null){
                liquids[fluidid] = amount;
                if(amount > liquids[fluidid]){
                    current = liq;
                }
            }
        }

        return readed;
    }
}