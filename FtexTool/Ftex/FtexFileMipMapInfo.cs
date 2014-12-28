using System;
using System.IO;
using System.Text;

namespace FtexTool.Ftex
{
    public class FtexFileMipMapInfo
    {
        public int Offset { get; set; }
        public int FileSize1 { get; set; }
        public int FileSize2 { get; set; }
        public byte Index { get; set; }
        public byte FtexsFileNr { get; set; }
        public short ChunkCount { get; set; }
        //public byte ChunkCount { get; set; }
        //public bool NotChunked { get; set; }

        public static FtexFileMipMapInfo Read(Stream inputStream)
        {
            FtexFileMipMapInfo result = new FtexFileMipMapInfo();
            BinaryReader reader = new BinaryReader(inputStream, Encoding.Default, true);
            result.Offset = reader.ReadInt32();
            result.FileSize1 = reader.ReadInt32();
            result.FileSize2 = reader.ReadInt32();
            result.Index = reader.ReadByte();
            result.FtexsFileNr = reader.ReadByte();
            result.ChunkCount = reader.ReadInt16();
            //result.NotChunked = Convert.ToBoolean(reader.ReadByte());
            return result;
        }

        public void Write(Stream outputStream)
        {
            BinaryWriter writer = new BinaryWriter(outputStream, Encoding.Default, true);
            writer.Write(Offset);
            writer.Write(FileSize1);
            writer.Write(FileSize2);
            writer.Write(Index);
            writer.Write(FtexsFileNr);
            writer.Write(ChunkCount);
        }
    }
}