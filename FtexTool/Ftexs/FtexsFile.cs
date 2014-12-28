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

        public static FtexsFile Read(Stream inputStream, byte chunkCount, bool seekOffset)
        {
            FtexsFile ftexsFile = new FtexsFile();
            return Read(ftexsFile, inputStream, chunkCount, seekOffset);
        }

        public static FtexsFile Read(FtexsFile ftexsFile, Stream inputStream, short chunkCount, bool seekOffset)
        {
            FtexsFileMipMap mipMap = FtexsFileMipMap.Read(inputStream, chunkCount, seekOffset);
            ftexsFile.MipMaps.Add(mipMap);
            return ftexsFile;
        }

        public void Write(Stream outputStream)
        {
            foreach (var mipMap in MipMaps)
            {
                mipMap.Write(outputStream);
            }
        }
    }
}