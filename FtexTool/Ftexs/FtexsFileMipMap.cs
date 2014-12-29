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
        private readonly List<FtexsFileChunk> _chunks;

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

        internal int IndexBlockSize
        {
            get { return FtexsFileChunk.IndexSize * _chunks.Count; }
        }

        public int Offset { get; set; }

        public FtexsFileMipMap()
        {
            _chunks = new List<FtexsFileChunk>();
        }

        public static FtexsFileMipMap ReadFtexsFileMipMap(Stream inputStream, short chunkCount, bool absoluteOffset)
        {
            FtexsFileMipMap result = new FtexsFileMipMap();
            result.Read(inputStream, chunkCount, absoluteOffset);
            return result;
        }

        public void Read(Stream inputStream, short chunkCount, bool absoluteOffset)
        {
            for (int i = 0; i < chunkCount; i++)
            {
                FtexsFileChunk chunk = FtexsFileChunk.ReadFtexsFileChunk(inputStream, absoluteOffset);
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

        public void Write(Stream outputStream, bool absoluteOffset)
        {
            BinaryWriter writer = new BinaryWriter(outputStream, Encoding.Default, true);
            Offset = Convert.ToInt32(writer.BaseStream.Position);
            writer.BaseStream.Position += IndexBlockSize;
            foreach (var chunk in Chunks)
            {
                if (absoluteOffset)
                    chunk.Offset = Convert.ToInt32(writer.BaseStream.Position);
                else
                    // HACK: Offset is only 8 when there are no other chunks. Offset is 0x80000008 when it's the smallest mipmap.
                    chunk.Offset = DefaultRelativeOffset; 
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