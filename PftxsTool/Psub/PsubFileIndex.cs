using System.IO;
using System.Text;

namespace PftxsTool.Psub
{
    public class PsubFileIndex
    {
        public const int PsubFileIndexSize = 8;

        public int Offset { get; set; }
        public int Size { get; set; }
        public byte[] Data { get; set; }

        public static PsubFileIndex ReadPsubFileIndex(Stream input)
        {
            PsubFileIndex psubFileIndex = new PsubFileIndex();
            psubFileIndex.Read(input);
            return psubFileIndex;
        }

        public void Read(Stream input)
        {
            BinaryReader reader = new BinaryReader(input, Encoding.Default, true);
            Offset = reader.ReadInt32();
            Size = reader.ReadInt32();
        }

        public void Write(Stream output)
        {
            BinaryWriter writer = new BinaryWriter(output, Encoding.Default, true);
            writer.Write(Offset);
            writer.Write(Size);
        }

        public void WriteData(Stream output)
        {
            BinaryWriter writer = new BinaryWriter(output, Encoding.Default, true);
            writer.Write(Data);
        }
    }
}
