using System;
using System.IO;
using System.Text;

namespace FtexTool.Ftexs
{
    public class FtexsFileChunk
    {
        public const int IndexSize = 8;

        private const uint RelativeOffsetValue = 0x80000000;

        public short WrittenChunkSize => CompressData ? CompressedChunkSize : ChunkSize;

        private short CompressedChunkSize { get; set; }

        private short ChunkSize { get; set; }

        private long DataOffset { get; set; }

        private uint EncodedDataOffset { get; set; }

        private byte[] ChunkData { get; set; }

        private bool CompressData { get; set; }
        
        public static FtexsFileChunk ReadFtexsFileChunk(Stream inputStream, int baseOffset)
        {
            FtexsFileChunk result = new FtexsFileChunk();
            result.Read(inputStream, baseOffset);
            return result;
        }

        public void Read(Stream inputStream, int baseOffset)
        {
            BinaryReader reader = new BinaryReader(inputStream, Encoding.ASCII, true);
            CompressedChunkSize = reader.ReadInt16();
            ChunkSize = reader.ReadInt16();
            EncodedDataOffset = reader.ReadUInt32();

            long indexEndPosition = reader.BaseStream.Position;
            if (EncodedDataOffset > RelativeOffsetValue)
            {
                DataOffset = baseOffset + (EncodedDataOffset - RelativeOffsetValue);
            }
            else
            {
                DataOffset = baseOffset + EncodedDataOffset;
            }

            reader.BaseStream.Position = DataOffset;
            byte[] data = reader.ReadBytes(CompressedChunkSize);
            bool compressed = CompressedChunkSize != ChunkSize;
            SetData(data, compressed);

            reader.BaseStream.Position = indexEndPosition;
        }

        public void WriteIndex(BinaryWriter writer)
        {
            writer.Write(CompressData ? CompressedChunkSize : ChunkSize);
            writer.Write(ChunkSize);
            writer.Write(EncodedDataOffset);
        }

        public void WriteData(Stream outputStream)
        {
            outputStream.Position = DataOffset;
            byte[] chunkData = CompressData ? ZipUtility.Deflate(ChunkData) : ChunkData;
            outputStream.Write(chunkData, 0, chunkData.Length);
        }

        public void PrepareWrite(Stream outputStream, uint baseOffset, bool isSingleChunk)
        {
            DataOffset = outputStream.Position;

            CompressData = true;
            if (isSingleChunk && ChunkSize <= CompressedChunkSize)
            {
                EncodedDataOffset = IndexSize | RelativeOffsetValue;
                CompressData = false;
            }
            else
            {
                EncodedDataOffset = Convert.ToUInt32(outputStream.Position) - baseOffset;
            }
        }

        public void SetData(byte[] chunkData, bool compressed)
        {
            if (compressed)
            {
                ChunkData = ZipUtility.Inflate(chunkData);

                ChunkSize = Convert.ToInt16(ChunkData.Length);
                CompressedChunkSize = Convert.ToInt16(chunkData.Length);
            }
            else
            {
                ChunkData = chunkData;

                ChunkSize = Convert.ToInt16(chunkData.Length);
                CompressedChunkSize = Convert.ToInt16(ZipUtility.Deflate(chunkData).Length);
            }
        }

        public void CopyTo(Stream stream)
        {
            stream.Write(ChunkData, 0, ChunkData.Length);
        }
    }
}
