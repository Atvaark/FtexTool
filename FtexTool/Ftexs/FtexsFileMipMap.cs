using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FtexTool.Ftexs
{
    public class FtexsFileMipMap
    {
        private const int DefaultRelativeOffset = 8;
        private const uint UncompressedFlag = 0x80000000;
        private readonly List<FtexsFileChunk> _chunks;

        public FtexsFileMipMap()
        {
            _chunks = new List<FtexsFileChunk>();
        }

        public IEnumerable<FtexsFileChunk> Chunks
        {
            get { return _chunks; }
        }

        public byte[] Data
        {
            get
            {
                MemoryStream stream = new MemoryStream();
                foreach (var chunk in Chunks)
                {
                    stream.Write(chunk.ChunkData, 0, chunk.ChunkData.Length);
                }
                return stream.ToArray();
            }
        }

        public int CompressedDataSize
        {
            get { return Chunks.Sum(chunk => chunk.CompressedChunkSize); }
        }

        public int IndexBlockSize
        {
            get { return FtexsFileChunk.IndexSize*_chunks.Count; }
        }

        public int Offset { get; set; }

        public static FtexsFileMipMap ReadFtexsFileMipMap(Stream inputStream, short chunkCount)
        {
            FtexsFileMipMap result = new FtexsFileMipMap();
            result.Read(inputStream, chunkCount);
            return result;
        }

        public void Read(Stream inputStream, short chunkCount)
        {
            bool absoluteOffseta = chunkCount != 1;
            for (int i = 0; i < chunkCount; i++)
            {
                FtexsFileChunk chunk = FtexsFileChunk.ReadFtexsFileChunk(inputStream, absoluteOffseta);
                AddChunk(chunk);
            }
        }

        public void AddChunk(FtexsFileChunk chunk)
        {
            _chunks.Add(chunk);
        }

        public void AddChunks(IEnumerable<FtexsFileChunk> chunks)
        {
            foreach (var chunk in chunks)
            {
                AddChunk(chunk);
            }
        }

        public void Write(Stream outputStream)
        {
            BinaryWriter writer = new BinaryWriter(outputStream, Encoding.Default, true);
            bool absoluteOffset = Chunks.Count() != 1;
            Offset = Convert.ToInt32(writer.BaseStream.Position);
            writer.BaseStream.Position += IndexBlockSize;

            foreach (var chunk in Chunks)
            {
                if (absoluteOffset)
                {
                    chunk.Offset = Convert.ToUInt32(writer.BaseStream.Position);
                }
                else
                {
                    chunk.Offset = DefaultRelativeOffset;
                    if (chunk.CompressedChunkSize == chunk.DecompressedChunkSize)
                        chunk.Offset = chunk.Offset | UncompressedFlag;
                }
                chunk.WriteData(outputStream);
            }
            long endPosition = writer.BaseStream.Position;
            writer.BaseStream.Position = Offset;
            foreach (var chunk in Chunks)
            {
                chunk.Write(outputStream);
            }
            writer.BaseStream.Position = endPosition;
        }
    }
}
