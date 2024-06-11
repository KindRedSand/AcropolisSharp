using JavaStream;

namespace Playground.Mindustry.Modules;

public class PowerModule
{
    public float status = 0.0f;
    public List<int> links = new();
    
    public void write(DataOutput write){
        write.WriteShort(links.Count);
        for(int i = 0; i < links.Count; i++){
            write.WriteInt(links[i]);
        }
        write.WriteFloat(status);
    }
    
    public int read(DataInput read, bool legacy = false){
        links.Clear();
        var readed = 2;
        short amount = read.ReadShort();
        for(int i = 0; i < amount; i++)
        {
            readed += 4;
            links.Add(read.ReadInt());
        }
        readed += 4;
        status = read.ReadFloat();
        if(float.IsNaN(status) || float.IsInfinity(status)) status = 0f;
        return readed;
    }
}