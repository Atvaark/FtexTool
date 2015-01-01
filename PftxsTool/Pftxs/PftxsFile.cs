using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PftxsTool.Psub;

namespace PftxsTool.Pftxs
{
    public class PftxsFile
    {
        public const int HeaderSize = 20;
        private const int PftxMagicNumber = 0x58544650; //PFTX
        private const int MagicNumber2 = 0x3F800000; // float 1
        private const int EndOfPackFileMagicNumber = 0x46504F45; //EOPF
        private readonly List<PftxsFileIndex> _filesIndices;

        public PftxsFile()
        {
            _filesIndices = new List<PftxsFileIndex>();
        }

        public int Size { get; set; }
        public int FileCount { get; set; }
        public int DataOffset { get; set; }

        public IEnumerable<PftxsFileIndex> FilesIndices
        {
            get { return _filesIndices; }
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
                AddPftxsFileIndex(pftxsFileIndex);
            }
            input.Position = DataOffset;
            foreach (var file in FilesIndices)
            {
                file.Data = reader.ReadBytes(file.FileSize);
                file.PsubFile = PsubFile.ReadPsubFile(input);
            }
            int magicNumber3 = reader.ReadInt32();
        }

        public void AddPftxsFileIndex(PftxsFileIndex pftxsFileIndex)
        {
            _filesIndices.Add(pftxsFileIndex);
        }

        public void Write(Stream output)
        {
            BinaryWriter writer = new BinaryWriter(output, Encoding.Default, true);
            long headerPosition = output.Position;
            output.Position += HeaderSize;
            long fileIndicesHeaderSize = PftxsFileIndex.HeaderSize*FilesIndices.Count();
            output.Position += fileIndicesHeaderSize;
            output.AlignWrite(16, 0xCC);

            foreach (var fileIndex in FilesIndices)
            {
                fileIndex.FileNameOffset = Convert.ToInt32(output.Position);
                fileIndex.WriteFileName(output);
            }
            output.AlignWrite(16, 0xCC);
            DataOffset = Convert.ToInt32(output.Position);
            foreach (var fileIndex in FilesIndices)
            {
                fileIndex.WriteData(output);
                fileIndex.WritePsubFile(output);
            }
            writer.Write(EndOfPackFileMagicNumber);
            output.AlignWrite(2048, 0xCC);
            long endPosition = output.Position;
            Size = Convert.ToInt32(endPosition);
            output.Position = headerPosition;
            writer.Write(PftxMagicNumber);
            writer.Write(MagicNumber2);
            writer.Write(Size);
            writer.Write(FileCount);
            writer.Write(DataOffset);
            foreach (var fileIndex in FilesIndices)
            {
                fileIndex.Write(output);
            }
            output.Position = endPosition;
        }
    }
}
