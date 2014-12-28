using System.IO;
using System.Text;

namespace FtexTool.Ftexs
{
    public class FtexsFileChunk
    {
        public const int IndexSize = 8;

        public short CompressedChunkSize { get; set; }
        public short DecompressedCunkSize { get; set; }
        public int Offset { get; set; }

        public byte[] ChunkData { get; set; }

        public static FtexsFileChunk Read(Stream inputStream, bool seekOffset)
        {
            FtexsFileChunk result = new FtexsFileChunk();
            BinaryReader reader = new BinaryReader(inputStream, Encoding.Default, true);
            result.CompressedChunkSize = reader.ReadInt16();
            result.DecompressedCunkSize = reader.ReadInt16();
            result.Offset = reader.ReadInt32();

            long position = reader.BaseStream.Position;

            if (seekOffset)
                reader.BaseStream.Seek(result.Offset, SeekOrigin.Begin);

            byte[] data = reader.ReadBytes(result.CompressedChunkSize);
            result.ChunkData = result.CompressedChunkSize == result.DecompressedCunkSize
                ? data
                : ZipUtility.Inflate(data);

            reader.BaseStream.Seek(position, SeekOrigin.Begin);
            return result;
        }

        public void Write(Stream outputStream)
        {
            BinaryWriter writer = new BinaryWriter(outputStream, Encoding.Default, true);
            writer.Write(CompressedChunkSize);
            writer.Write(DecompressedCunkSize);
            writer.Write(Offset);
        }

        public void WriteData(Stream outputStream)
        {
            BinaryWriter writer = new BinaryWriter(outputStream, Encoding.Default, true);
            writer.Write(ChunkData);
        }
    }
}