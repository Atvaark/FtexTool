using System.IO;
using System.Text;

namespace FtexTool.Ftexs
{
    public class FtexsFileChunk
    {
        public const int IndexSize = 8;
        private const int OffsetBitMask = 0xFFFF;

        public short CompressedChunkSize { get; set; }
        public short DecompressedChunkSize { get; set; }
        public int Offset { get; set; }

        public byte[] ChunkData { get; set; }

        public static FtexsFileChunk ReadFtexsFileChunk(Stream inputStream, bool absoluteOffset)
        {
            FtexsFileChunk result = new FtexsFileChunk();
            result.Read(inputStream, absoluteOffset);
            return result;
        }

        public void Read(Stream inputStream, bool absoluteOffset)
        {
            BinaryReader reader = new BinaryReader(inputStream, Encoding.Default, true);
            CompressedChunkSize = reader.ReadInt16();
            DecompressedChunkSize = reader.ReadInt16();
            Offset = reader.ReadInt32();

            long indexEndPosition = reader.BaseStream.Position;

            if (absoluteOffset)
            {
                reader.BaseStream.Position = Offset;
            }
            else
            {
                // HACK: result.Offset could be 0x80000008
                reader.BaseStream.Position = indexEndPosition + (Offset & OffsetBitMask ) - IndexSize;
            }

            byte[] data = reader.ReadBytes(CompressedChunkSize);
            ChunkData = CompressedChunkSize == DecompressedChunkSize
                ? data
                : ZipUtility.Inflate(data);

            reader.BaseStream.Position = indexEndPosition;
        }

        public void Write(Stream outputStream)
        {
            BinaryWriter writer = new BinaryWriter(outputStream, Encoding.Default, true);
            writer.Write(CompressedChunkSize);
            writer.Write(DecompressedChunkSize);
            writer.Write(Offset);
        }

        public void WriteData(Stream outputStream)
        {
            BinaryWriter writer = new BinaryWriter(outputStream, Encoding.Default, true);
            writer.Write(ChunkData);
        }
    }
}