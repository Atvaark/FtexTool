using System.Collections.Generic;
using System.IO;

namespace FtexTool.Ftexs
{
    public class FtexsFile
    {
        private readonly List<FtexsFileMipMap> _mipMaps;

        public List<FtexsFileMipMap> MipMaps
        {
            get { return _mipMaps; }
        }

        public byte FileNumber { get; set; }


        public byte[] Data
        {
            get
            {
                MemoryStream stream = new MemoryStream();
                foreach (var mipMap in MipMaps)
                {
                    stream.Write(mipMap.Data, 0, mipMap.Data.Length);
                }
                return stream.ToArray();
            }
        }


        public FtexsFile()
        {
            _mipMaps = new List<FtexsFileMipMap>();
        }

        public static FtexsFile Read(Stream inputStream, byte chunkCount, bool absoluteOffset)
        {
            FtexsFile ftexsFile = new FtexsFile();
            return Read(ftexsFile, inputStream, chunkCount, absoluteOffset);
        }

        public static FtexsFile Read(FtexsFile ftexsFile, Stream inputStream, short chunkCount, bool absoluteOffset)
        {
            FtexsFileMipMap mipMap = FtexsFileMipMap.Read(inputStream, chunkCount, absoluteOffset);
            ftexsFile.MipMaps.Add(mipMap);
            return ftexsFile;
        }

        public void Write(Stream outputStream)
        {
            // HACK: Save the mipmaps in ascending order 
            for (int i = MipMaps.Count - 1; i >= 0; i--)
            {
                FtexsFileMipMap mipMap = MipMaps[i];
                bool absoluteOffset = FileNumber != 1;
                mipMap.Write(outputStream, absoluteOffset);
            }
        }
    }
}