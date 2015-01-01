using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using PftxsTool.Psub;

namespace PftxTool.Pftx
{
    public class PftxsFile
    {
        private const int MagicNumber1 = 0x58544650; //PFTX
        private const int MagicNumber2 = 0x3F800000;
        private const int MagicNumber3 = 0x46504F45; //46504F45
        private readonly List<PftxsFileIndex> _files;

        public PftxsFile()
        {
            _files = new List<PftxsFileIndex>();
        }

        public int Size { get; set; }
        public int FileCount { get; set; }
        public int DataOffset { get; set; }

        public IEnumerable<PftxsFileIndex> Files
        {
            get { return _files; }
        }

        public static PftxsFile ReadPftxsFile(Stream input)
        {
            PftxsFile pftxsFile = new PftxsFile();
            pftxsFile.Read(input);
            return pftxsFile;
        }

        public void Read(Stream input)
        {
            BinaryReader reader = new BinaryReader(input, Encoding.Default, true);
            int magicNumber1 = reader.ReadInt32();
            int magicNumber2 = reader.ReadInt32();
            Size = reader.ReadInt32();
            FileCount = reader.ReadInt32();
            DataOffset = reader.ReadInt32();
            for (int i = 0; i < FileCount; i++)
            {
                PftxsFileIndex pftxsFileIndex = new PftxsFileIndex();
                pftxsFileIndex.Read(input);
                _files.Add(pftxsFileIndex);
            }
            input.Position = DataOffset;
            foreach (var file in Files)
            {
                file.Data = reader.ReadBytes(file.FileSize);
                file.PsubFile = PsubFile.ReadPsubFile(input);
            }
            int magicNumber3 = reader.ReadInt32();
        }
    }
}
