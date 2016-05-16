using System;
using System.IO;
using System.Text;

namespace FtexTool.Ftexs
{
    public class FtexsFileChunk
    {
        public short WrittenChunkSize => _index.WrittenChunkSize;

        private byte[] ChunkData { get; set; }

        private readonly FtexsFileChunkIndex _index = new FtexsFileChunkIndex();
        
        public static FtexsFileChunk ReadFtexsFileChunk(Stream inputStream, int baseOffset)
        {
            FtexsFileChunk result = new FtexsFileChunk();
            result.Read(inputStream, baseOffset);
            return result;
        }

        public void Read(Stream inputStream, int baseOffset)
        {
            BinaryReader reader = new BinaryReader(inputStream, Encoding.ASCII, true);

            _index.Read(reader, baseOffset);
            long indexEndPosition = reader.BaseStream.Position;

            reader.BaseStream.Position = _index.DataOffset;
            byte[] data = reader.ReadBytes(_index.CompressedChunkSize);
            bool compressed = _index.CompressedChunkSize != _index.ChunkSize;
            SetData(data, compressed);

            reader.BaseStream.Position = indexEndPosition;
        }
        
        public void SetData(byte[] chunkData, bool compressed)
        {
            if (compressed)
            {
                ChunkData = ZipUtility.Inflate(chunkData);

                _index.ChunkSize = Convert.ToInt16(ChunkData.Length);
                _index.CompressedChunkSize = Convert.ToInt16(chunkData.Length);
            }
            else
            {
                ChunkData = chunkData;

                _index.ChunkSize = Convert.ToInt16(chunkData.Length);
                _index.CompressedChunkSize = Convert.ToInt16(ZipUtility.Deflate(chunkData).Length);
            }
        }

        public void WriteData(Stream outputStream)
        {
            outputStream.Position = _index.DataOffset;
            byte[] chunkData = _index.CompressData ? ZipUtility.Deflate(ChunkData) : ChunkData;
            outputStream.Write(chunkData, 0, chunkData.Length);
        }

        public void UpdateDataOffset(Stream outputStream, uint baseOffset, bool isSingleChunk)
        {
            _index.SetDataOffset(outputStream, baseOffset, isSingleChunk);
        }

        public void WriteIndex(BinaryWriter writer)
        {
            _index.Write(writer);
        }

        public void CopyTo(Stream stream)
        {
            stream.Write(ChunkData, 0, ChunkData.Length);
        }
    }
}
