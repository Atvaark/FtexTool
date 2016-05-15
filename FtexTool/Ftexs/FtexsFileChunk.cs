using System;
using System.IO;
using System.Text;

namespace FtexTool.Ftexs
{
    public class FtexsFileChunk
    {
        public const int IndexSize = 8;
        public const uint RelativeOffsetValue = 0x80000000;

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

        public static FtexsFileChunk ReadFtexsFileChunk(Stream inputStream, int baseOffset)
        {
            FtexsFileChunk result = new FtexsFileChunk();
            result.Read(inputStream, baseOffset);
            return result;
        }

        public void Read(Stream inputStream, int baseOffset)
        {
            BinaryReader reader = new BinaryReader(inputStream, Encoding.ASCII, true);
            short compressedChunkSize = reader.ReadInt16();
            short decompressedChunkSize = reader.ReadInt16();
            Offset = reader.ReadUInt32();

            long indexEndPosition = reader.BaseStream.Position;

            if (Offset > RelativeOffsetValue)
            {
                reader.BaseStream.Position = baseOffset + (Offset - RelativeOffsetValue);
            }
            else
            {
                reader.BaseStream.Position = baseOffset + Offset;
            }

            byte[] data = reader.ReadBytes(compressedChunkSize);
            bool dataCompressed = compressedChunkSize != decompressedChunkSize;
            SetData(data, dataCompressed, false);

            reader.BaseStream.Position = indexEndPosition;
        }

        public void WriteIndex(Stream outputStream)
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

        public void SetData(byte[] chunkData, bool compressed, bool forWriting)
        {
            // TODO: Refactor this whole method.
            // - Only keep the uncompressed in memory.
            if (compressed)
            {
                CompressedChunkData = chunkData;
                ChunkData = ZipUtility.Inflate(chunkData);
            }
            else
            {
                byte[] compressedChunkData = ZipUtility.Deflate(chunkData);
                if (forWriting && compressedChunkData.Length >= chunkData.Length)
                {
                    CompressedChunkData = chunkData;
                }
                else
                {
                    CompressedChunkData = compressedChunkData;
                }

                ChunkData = chunkData;
            }
        }
    }
}
