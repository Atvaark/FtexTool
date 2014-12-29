using System;
using System.IO;
using System.Text;

namespace FtexTool.Ftex
{
    public class FtexFileMipMapInfo
    {
        public int Offset { get; set; }
        public int DecompressedFileSize { get; set; }
        public int CompressedFileSize { get; set; }
        public byte Index { get; set; }
        public byte FtexsFileNr { get; set; }
        public short ChunkCount { get; set; }

        public static FtexFileMipMapInfo Read(Stream inputStream)
        {
            FtexFileMipMapInfo result = new FtexFileMipMapInfo();
            BinaryReader reader = new BinaryReader(inputStream, Encoding.Default, true);
            result.Offset = reader.ReadInt32();
            result.DecompressedFileSize = reader.ReadInt32();
            result.CompressedFileSize = reader.ReadInt32();
            result.Index = reader.ReadByte();
            result.FtexsFileNr = reader.ReadByte();
            result.ChunkCount = reader.ReadInt16();
            return result;
        }

        public void Write(Stream outputStream)
        {
            BinaryWriter writer = new BinaryWriter(outputStream, Encoding.Default, true);
            writer.Write(Offset);
            writer.Write(DecompressedFileSize);
            writer.Write(CompressedFileSize);
            writer.Write(Index);
            writer.Write(FtexsFileNr);
            writer.Write(ChunkCount);
        }
    }
}