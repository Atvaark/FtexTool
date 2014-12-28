using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FtexTool.Ftexs;

namespace FtexTool.Ftex
{
    public class FtexFile
    {
        private const long MagicNumber = 4612226451348214854; // FTEX 85 EB 01 40

        private readonly List<FtexFileMipMapInfo> _mipMapInfos;
        private readonly List<FtexsFile> _ftexsFiles;

        public short DtxType { get; set; }
        public short Width { get; set; }
        public short Height { get; set; }
        public byte MipMapCount { get; set; }
        public byte FtexsFileCount { get; set; }

        // FtexsFileCount - 1
        public byte UnknownCount { get; set; }

        public List<FtexFileMipMapInfo> MipMapInfos
        {
            get { return _mipMapInfos; }
        }

        public List<FtexsFile> FtexsFiles
        {
            get { return _ftexsFiles; }
        }

        public byte[] Data
        {
            get
            {
                MemoryStream stream = new MemoryStream();
                for (int i = FtexsFiles.Count - 1; i >= 0; i--)
                {
                    FtexsFile ftexsFile = FtexsFiles[i];
                    byte[] ftexsFileData = ftexsFile.Data;
                    stream.Write(ftexsFileData, 0, ftexsFileData.Length);
                }
                return stream.ToArray();
            }
        }

        public FtexFile()
        {
            _ftexsFiles = new List<FtexsFile>();
            _mipMapInfos = new List<FtexFileMipMapInfo>();
        }

        public static FtexFile Read(Stream inputStream)
        {
            FtexFile result = new FtexFile();
            BinaryReader reader = new BinaryReader(inputStream, Encoding.Default, true);
            reader.Assert(MagicNumber);
            result.DtxType = reader.ReadInt16();
            result.Width = reader.ReadInt16();
            result.Height = reader.ReadInt16();
            reader.Skip(2);
            result.MipMapCount = reader.ReadByte();
            reader.Skip(15);
            result.FtexsFileCount = reader.ReadByte();
            result.UnknownCount = reader.ReadByte();
            reader.Skip(30);

            for (int i = 0; i < result.MipMapCount; i++)
            {
                FtexFileMipMapInfo fileMipMapInfo = FtexFileMipMapInfo.Read(inputStream);
                result.MipMapInfos.Add(fileMipMapInfo);
            }

            return result;
        }

        public void Write(Stream outputStream)
        {
            BinaryWriter writer = new BinaryWriter(outputStream, Encoding.Default, true);
            writer.Write(MagicNumber);
            writer.Write(DtxType);
            writer.Write(Width);
            writer.Write(Height);
            writer.WriteZeros(2);
            writer.Write(MipMapCount);
            writer.WriteZeros(15);
            writer.Write(FtexsFileCount);
            writer.Write(UnknownCount);
            writer.WriteZeros(30);
            foreach (var mipMap in MipMapInfos)
            {
                mipMap.Write(outputStream);
            }
        }

        public void UpdateOffsets()
        {
            byte ftexsFileNr = 1;
            int mipMapIndex = 0;
            foreach (var ftexsFile in FtexsFiles)
            {
                foreach (var ftexsFileMipMap in ftexsFile.MipMaps)
                {
                    FtexFileMipMapInfo ftexMipMapInfo = MipMapInfos[mipMapIndex];
                    ftexMipMapInfo.ChunkCount = Convert.ToByte(ftexsFileMipMap.Chunks.Count());
                    ftexMipMapInfo.Offset = ftexsFileMipMap.Chunks.First().Offset - ftexsFileMipMap.IndexBlockSize;
                    ftexMipMapInfo.FtexsFileNr = ftexsFileNr;
                    ++mipMapIndex;
                }
                ++ftexsFileNr;
            }
        }
    }
}