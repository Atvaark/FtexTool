using System.IO;
using System.Text;

namespace PftxsTool.Psub
{
    public class PsubFileIndex
    {
        public int Offset { get; set; }
        public int Size { get; set; }
        public byte[] Data { get; set; }

        public void Read(Stream input)
        {
            BinaryReader reader = new BinaryReader(input, Encoding.Default, true);
            Offset = reader.ReadInt32();
            Size = reader.ReadInt32();
        }
    }
}
