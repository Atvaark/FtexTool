using System.IO;
using System.Text;
using PftxsTool.Psub;

namespace PftxTool.Pftx
{
    public class PftxsFileIndex
    {
        public int FileNameOffset { get; set; }
        public int FileSize { get; set; }
        public string FileName { get; set; }
        public byte[] Data { get; set; }
        public PsubFile PsubFile { get; set; }

        public void Read(Stream input)
        {
            BinaryReader reader = new BinaryReader(input, Encoding.Default, true);
            FileNameOffset = reader.ReadInt32();
            FileSize = reader.ReadInt32();

            long position = input.Position;
            input.Position = FileNameOffset;
            FileName = reader.ReadNullTerminatedString();
            input.Position = position;
        }
    }
}
