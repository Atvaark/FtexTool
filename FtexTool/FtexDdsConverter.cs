using System;
using System.Collections.Generic;
using System.Linq;
using FtexTool.Dds;
using FtexTool.Dds.Enum;
using FtexTool.Ftex;
using FtexTool.Ftexs;

namespace FtexTool
{
    internal static class FtexDdsConverter
    {

        public static DdsFile ConvertToDds(FtexFile file)
        {
            DdsFile result = new DdsFile
            {
                Header = new DdsFileHeader
                {
                    Size = DdsFileHeader.DefaultHeaderSize,
                    Flags = DdsFileHeaderFlags.Texture | DdsFileHeaderFlags.MipMap,
                    Height = file.Height,
                    Width = file.Width,
                    MipMapCount = file.MipMapCount
                }
            };

            switch (file.DtxType)
            {
                case 0:
                    // TODO: Find out which DTX Type 0 is.
                    result.Header.PixelFormat = DdsPixelFormat.DdsPfDxt1();
                    break;
                case 1:
                    // TODO: Find out which DTX Type 1 is.
                    result.Header.PixelFormat = DdsPixelFormat.DdsPfDxt1();
                    break;
                case 2:
                    result.Header.PixelFormat = DdsPixelFormat.DdsPfDxt1();
                    break;
                case 4:
                    result.Header.PixelFormat = DdsPixelFormat.DdsPfDxt5();
                    break;
                default:
                    throw new NotImplementedException(String.Format("Unknown DtxType {0}", file.DtxType));
            }
            result.Header.Caps = DdsSurfaceFlags.Texture | DdsSurfaceFlags.MipMap;
            result.Data = file.Data;
            return result;
        }

        public static FtexFile ConvertToFtex(DdsFile file)
        {
            FtexFile result = new FtexFile();
            result.DtxType = 4;
            result.Height = Convert.ToInt16(file.Header.Height);
            result.Width = Convert.ToInt16(file.Header.Width);
            result.MipMapCount = Convert.ToByte(file.Header.MipMapCount);

            var mipMapData = GetMipMapData(file);
            var mipMaps = GetMipMapInfos(mipMapData);
            var ftexsFiles = GetFtexsFiles(mipMaps, mipMapData);

            result.MipMapInfos.AddRange(mipMaps);
            result.FtexsFiles.AddRange(ftexsFiles);
            result.FtexsFileCount = Convert.ToByte(ftexsFiles.Count());
            return result;
        }

        private static List<FtexsFile> GetFtexsFiles(List<FtexFileMipMapInfo> mipMapInfos, List<byte[]> mipMapDatas)
        {
            List<FtexsFile> ftexsFiles = new List<FtexsFile>();
            // HACK: For testing reasons all get put in one ftexs file
            FtexsFile ftexsFile = new FtexsFile();
            for (int i = 0; i < mipMapInfos.Count; i++)
            {
                FtexFileMipMapInfo mipMapInfo = mipMapInfos[i];
                byte[] mipMapData = mipMapDatas[i];
                FtexsFileMipMap ftexsFileMipMap = new FtexsFileMipMap();
                List<FtexsFileChunk> chunks = GetFtexsChunks(mipMapInfo, mipMapData);
                ftexsFileMipMap.Chunks.AddRange(chunks);
                ftexsFile.MipMaps.Add(ftexsFileMipMap);
            }
            ftexsFiles.Add(ftexsFile);
            return ftexsFiles;
        }

        private static List<FtexsFileChunk> GetFtexsChunks(FtexFileMipMapInfo mipMapInfo, byte[] mipMapData)
        {
            List<FtexsFileChunk> ftexsFileChunks = new List<FtexsFileChunk>();
            const int maxChunkSize = short.MaxValue;
            int requiredChunks = (int) Math.Ceiling((double) mipMapData.Length/maxChunkSize);
            int mipMapDataOffset = 0;
            for (int i = 0; i < requiredChunks; i++)
            {
                FtexsFileChunk chunk = new FtexsFileChunk();
                int chunkSize = Math.Min(mipMapData.Length - mipMapDataOffset, maxChunkSize);
                byte[] chunkData = new byte[chunkSize];
                Array.Copy(mipMapData, mipMapDataOffset, chunkData, 0, chunkSize);
                chunk.ChunkData = chunkData;
                chunk.CompressedChunkSize = Convert.ToInt16(chunkSize);
                chunk.DecompressedCunkSize = Convert.ToInt16(chunkSize);
                ftexsFileChunks.Add(chunk);
                mipMapDataOffset += chunkSize;
            }
            return ftexsFileChunks;
        }

        private static List<FtexFileMipMapInfo> GetMipMapInfos(List<byte[]> levelData)
        {
            List<FtexFileMipMapInfo> mipMapsInfos = new List<FtexFileMipMapInfo>();
            for (int i = 0; i < levelData.Count; i++)
            {
                FtexFileMipMapInfo mipMapInfo = new FtexFileMipMapInfo();
                mipMapInfo.FileSize1 = levelData[i].Length;
                mipMapInfo.FileSize2 = levelData[i].Length;
                mipMapInfo.Index = Convert.ToByte(i);
                mipMapsInfos.Add(mipMapInfo);
            }
            return mipMapsInfos;
        }

        private static List<byte[]> GetMipMapData(DdsFile file)
        {
            List<byte[]> mipMapDatas = new List<byte[]>();
            byte[] data = file.Data;
            int dataOffset = 0;
            int size = (file.Header.Height*file.Header.Width)/2;
            for (int i = 0; i < file.Header.MipMapCount; i++)
            {
                var buffer = new byte[size];
                Array.Copy(data, dataOffset, buffer, 0, size);
                mipMapDatas.Add(buffer);
                dataOffset += size;
                // TODO: Check if fox engine works without this hack.
                size = size == 16 ? 8 : size/4;
            }
            return mipMapDatas;
        }
    }
}