using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PftxsTool.Psub
{
    public class PsubFile
    {
        private const int MagicNumber = 0x42555350; // PSUB
        private readonly List<PsubFileIndex> _indices;

        public PsubFile()
        {
            _indices = new List<PsubFileIndex>();
        }

        public IEnumerable<PsubFileIndex> Indices
        {
            get { return _indices; }
        }

        public static PsubFile ReadPsubFile(Stream input)
        {
            PsubFile psubFile = new PsubFile();
            psubFile.Read(input);
            return psubFile;
        }

        public void Read(Stream input)
        {
            BinaryReader reader = new BinaryReader(input, Encoding.Default, true);
            int magicNumber = reader.ReadInt32();
            int indexCount = reader.ReadInt32();
            for (int i = 0; i < indexCount; i++)
            {
                PsubFileIndex index = PsubFileIndex.ReadPsubFileIndex(input);
                AddPsubFileIndex(index);
            }
            input.AlignRead(16);
            foreach (var index in Indices)
            {
                index.Data = reader.ReadBytes(index.Size);
                input.AlignRead(16);
            }
        }

        public void AddPsubFileIndex(PsubFileIndex index)
        {
            _indices.Add(index);
        }

        public void Write(Stream output)
        {
            BinaryWriter writer = new BinaryWriter(output, Encoding.Default, true);
            writer.Write(MagicNumber);
            writer.Write(Indices.Count());
            long indexPosition = output.Position;
            output.Position += PsubFileIndex.PsubFileIndexSize*Indices.Count();
            output.AlignWrite(16, 0xCC);
            foreach (var index in Indices)
            {
                index.Offset = Convert.ToInt32(output.Position);
                index.Size = index.Data.Length;
                index.WriteData(output);
                output.AlignWrite(16, 0xCC);
            }
            long endPosition = output.Position;
            output.Position = indexPosition;
            foreach (var index in Indices)
            {
                index.Write(output);
            }
            output.Position = endPosition;
        }
    }
}
