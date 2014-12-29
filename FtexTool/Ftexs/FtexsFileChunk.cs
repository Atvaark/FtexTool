using System.IO;
using System.Text;

namespace FtexTool.Ftexs
{
    public class FtexsFileChunk
    {
        public const int IndexSize = 8;

        public short CompressedChunkSize { get; set; }
        public short DecompressedChunkSize { get; set; }
        public int Offset { get; set; }

        public byte[] ChunkData { get; set; }

        public static FtexsFileChunk Read(Stream inputStream, bool absoluteOffset)
        {
            FtexsFileChunk result = new FtexsFileChunk();
            BinaryReader reader = new BinaryReader(inputStream, Encoding.Default, true);
            result.CompressedChunkSize = reader.ReadInt16();
            result.DecompressedChunkSize = reader.ReadInt16();
            result.Offset = reader.ReadInt32();

            long indexEndPosition = reader.BaseStream.Position;

            if (absoluteOffset)
            {
                reader.BaseStream.Position = result.Offset;
            }
            else
            {
                // HACK: result.Offset could be 0x80000008
                reader.BaseStream.Position = indexEndPosition + (result.Offset & 0xFFFF ) - IndexSize;
            }
                

            byte[] data = reader.ReadBytes(result.CompressedChunkSize);
            result.ChunkData = result.CompressedChunkSize == result.DecompressedChunkSize
                ? data
                : ZipUtility.Inflate(data);

            reader.BaseStream.Position = indexEndPosition;
            return result;
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