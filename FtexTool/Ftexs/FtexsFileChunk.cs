using System;
using System.IO;
using System.Text;

namespace FtexTool.Ftexs
{
    public class FtexsFileChunk
    {
        public const int IndexSize = 8;
        private const int OffsetBitMask = 0xFFFF;

        public short CompressedChunkSize
        {
            get { return Convert.ToInt16(CompressedChunkData.Length); }
        }

        public short ChunkSize
        {
            get { return Convert.ToInt16(ChunkData.Length); }
        }

        public uint Offset { get; set; }
        public byte[] ChunkData { get; private set; }
        public byte[] CompressedChunkData { get; private set; }

        public static FtexsFileChunk ReadFtexsFileChunk(Stream inputStream, bool absoluteOffset)
        {
            FtexsFileChunk result = new FtexsFileChunk();
            result.Read(inputStream, absoluteOffset);
            return result;
        }

        public void Read(Stream inputStream, bool absoluteOffset)
        {
            BinaryReader reader = new BinaryReader(inputStream, Encoding.Default, true);
            short compressedChunkSize = reader.ReadInt16();
            short decompressedChunkSize = reader.ReadInt16();
            Offset = reader.ReadUInt32();

            long indexEndPosition = reader.BaseStream.Position;

            if (absoluteOffset)
            {
                reader.BaseStream.Position = Offset;
            }
            else
            {
                // HACK: result.Offset could be 0x80000008
                reader.BaseStream.Position = indexEndPosition + (Offset & OffsetBitMask) - IndexSize;
            }

            byte[] data = reader.ReadBytes(compressedChunkSize);
            bool dataCompressed = compressedChunkSize != decompressedChunkSize;
            SetData(data, dataCompressed);
            reader.BaseStream.Position = indexEndPosition;
        }

        public void Write(Stream outputStream)
        {
            BinaryWriter writer = new BinaryWriter(outputStream, Encoding.Default, true);
            writer.Write(CompressedChunkSize);
            writer.Write(ChunkSize);
            writer.Write(Offset);
        }

        public void WriteData(Stream outputStream, bool writeCompressedData)
        {
            BinaryWriter writer = new BinaryWriter(outputStream, Encoding.Default, true);
            if (writeCompressedData)
            {
                writer.Write(CompressedChunkData);
            }
            else
            {
                writer.Write(ChunkData);
            }
        }

        public void SetData(byte[] chunkData, bool compressed)
        {
            if (compressed)
            {
                CompressedChunkData = chunkData;
                ChunkData = ZipUtility.Inflate(chunkData);
            }
            else
            {
                CompressedChunkData = ZipUtility.Deflate(chunkData);
                ChunkData = chunkData;
            }
        }
    }
}
